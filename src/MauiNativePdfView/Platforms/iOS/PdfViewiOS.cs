using MauiNativePdfView.Abstractions;
using PdfKit;
using UIKit;
using Foundation;
using ObjCRuntime;

namespace MauiNativePdfView.Platforms.iOS;

/// <summary>
/// iOS implementation of IPdfView using PdfKit's PdfView.
/// </summary>
public class PdfViewiOS : IPdfView, IDisposable
{
    private readonly NativePdfView _pdfView;
    private PdfSource? _source;
    private bool _disposed;
    private NSObject? _pageChangedObserver;
    private NSObject? _annotationHitObserver;
    private UITapGestureRecognizer? _tapGestureRecognizer;
    private PdfScrollOrientation _scrollOrientation = PdfScrollOrientation.Vertical;
    private int _defaultPage = 0;
    private bool _documentLoaded = false;
    private bool _enableAnnotationRendering = true;
    private bool _enableTapGestures = true;
    private FitPolicy _fitPolicy = FitPolicy.Width;
    private bool _needsFitReapply = false;
    private PageAlignment _pageAlignment = PageAlignment.Default;
    private Color? _backgroundColor;
    private bool _enableZoom = true;
    private float _minZoom = 1.0f;
    private float _maxZoom = 3.0f;
    private nfloat _manualFitScale;
    private nfloat _appliedFitScale;
    private float _zoom = 1.0f;
    private bool _zoomNeedsApply;
    private int? _pendingPage;
    private NSObject? _scaleChangedObserver;
    private float _lastReportedZoom = 1.0f;
    private UIScrollView? _zoomScrollView;
    private ZoomReportingScrollViewDelegate? _zoomDelegateProxy;
    private bool _reportingZoom;

    /// <summary>
    /// Smallest zoom movement worth publishing. A pinch posts a scale change per frame, and
    /// without a floor the last few bits of float noise would republish on every one of them.
    /// </summary>
    private const float ZoomReportThreshold = 0.001f;

    public PdfViewiOS()
    {
        _pdfView = new NativePdfView
        {
            AutoScales = true,
            DisplayMode = PdfKit.PdfDisplayMode.SinglePageContinuous,
            DisplayDirection = PdfDisplayDirection.Vertical,
            // PdfKit paints an opaque grey backdrop by default, which hides anything sharing
            // the PdfView's grid cell. Start transparent to match the Android control so an
            // unset MAUI BackgroundColor composites identically on both platforms.
            BackgroundColor = UIColor.Clear
        };

        // PdfKit re-fits the document during its own layout, which rewrites ScaleFactor and
        // with it the relative zoom. Capture what is on screen before that happens so the
        // post-layout hook below can put it back.
        _pdfView.WillLayoutSubviewsAction = CaptureZoom;

        // Re-apply deferred fit policy once the view has been laid out and has real bounds.
        // Page alignment is re-applied every layout pass so it survives PdfKit's
        // internal re-centering when the document or scale changes.
        _pdfView.LayoutSubviewsAction = () =>
        {
            if (_needsFitReapply)
                ApplyFitPolicy();

            // PdfKit builds its scroll view lazily and can replace it across a document
            // load, so the zoom hook is refreshed from here rather than the ctor.
            // UpdateZoomSampling();
            EnsureZoomDelegateProxy();

            // MinZoom/MaxZoom are multiples of the fit scale, and the fit scale moves
            // whenever the view resizes (rotation, split view, first layout). Re-derive
            // the native bounds whenever it does — this also covers the initial pass,
            // where the fit scale isn't computable until the view has real bounds.
            if (GetFitScale() != _appliedFitScale)
            {
                // That re-fit dropped the relative zoom back to 1. Restore the captured
                // level so rotating doesn't silently zoom the user out — the Android control
                // keeps its zoom across a size change, and this keeps the two matching.
                _zoomNeedsApply = true;
                ApplyZoomConstraints();
            }

            // Replays a level assigned before the view had bounds or a document to fit.
            SyncZoom();

            ApplyPageAlignment();
        };

        // Subscribe to page change notifications
        _pageChangedObserver = NSNotificationCenter.DefaultCenter.AddObserver(
            PdfKit.PdfView.PageChangedNotification,
            OnPageChangedNotification,
            _pdfView);

        // Subscribe to annotation hit notifications
        _annotationHitObserver = PdfKit.PdfView.Notifications.ObserveAnnotationHit(OnAnnotationHit);

        // PdfKit posts this from its own ScaleFactor bookkeeping, which covers a pinch but
        // NOT the double-tap zoom — that one drives PdfKit's internal scroll view directly
        // and posts nothing, which is what the zoom sampler is for. Kept as the cheap pinch
        // path so a pinch reports without waiting on the next sampled frame.
        // Scoped to this view so a second PdfView on screen does not report through us.
        _scaleChangedObserver = PdfKit.PdfView.Notifications.ObserveScaleChanged(_pdfView, (_, _) => ReportZoomIfChanged());

        // Set delegate to intercept link clicks
        _pdfView.WeakDelegate = new PdfViewDelegateImpl(this);

        // Add tap gesture recognizer
        _tapGestureRecognizer = new UITapGestureRecognizer(HandleTap);
        _pdfView.AddGestureRecognizer(_tapGestureRecognizer);
    }

    /// <summary>
    /// Gets the native PdfView instance.
    /// </summary>
    public PdfKit.PdfView NativeView => _pdfView;

    public PdfSource? Source
    {
        get => _source;
        set
        {
            if (ReferenceEquals(_source, value))
                return;

            _source = value;
            // A different document starts fitted; only an internal reload keeps the level
            // the user was at.
            LoadDocument(preserveZoom: false);
        }
    }

    /// <inheritdoc />
    public int CurrentPage
    {
        get => _pdfView.Document != null && _pdfView.CurrentPage != null
            ? (int)_pdfView.Document.GetPageIndex(_pdfView.CurrentPage)
            : _pendingPage ?? 0;
        set
        {
            if (_pdfView.Document != null)
            {
                GoToPage(value);
                return;
            }

            // No document yet. LoadDocument applies this once one is in, so a page assigned
            // before the load is honoured rather than dropped.
            if (value >= 0)
                _pendingPage = value;
        }
    }

    public int PageCount => _pdfView.Document != null ? (int)_pdfView.Document.PageCount : 0;

    public bool EnableZoom
    {
        get => _enableZoom;
        set
        {
            if (_enableZoom == value)
                return;

            _enableZoom = value;
            ApplyZoomConstraints();
        }
    }

    public bool EnableSwipe { get; set; } = true;

    public bool EnableTapGestures
    {
        get => _enableTapGestures;
        set
        {
            if (_enableTapGestures == value)
            {
                return;
            }

            _enableTapGestures = value;

            if (_tapGestureRecognizer != null)
            {
                if (_enableTapGestures && !_pdfView.GestureRecognizers.Contains(_tapGestureRecognizer))
                {
                    _pdfView.AddGestureRecognizer(_tapGestureRecognizer);
                }
                else if (!_enableTapGestures && _pdfView.GestureRecognizers.Contains(_tapGestureRecognizer))
                {
                    _pdfView.RemoveGestureRecognizer(_tapGestureRecognizer);
                }
            }
        }
    }

    public bool EnableLinkNavigation
    {
        get => _pdfView.EnableDataDetectors;
        set => _pdfView.EnableDataDetectors = value;
    }

    /// <summary>
    /// Zoom level expressed as a multiple of the fit scale, matching <see cref="MinZoom"/>/
    /// <see cref="MaxZoom"/> and the Android control: <c>1.0</c> is the fitted document,
    /// <c>2.0</c> is twice that. This is deliberately NOT PdfKit's absolute
    /// <see cref="PdfKit.PdfView.ScaleFactor"/>, which is relative to the PDF's intrinsic size.
    /// </summary>
    public float Zoom
    {
        // While a level is waiting to be applied, ours is the truth: the control is still
        // showing whatever it re-fitted itself to.
        get => _zoomNeedsApply ? _zoom : ReadZoom();
        set
        {
            // The echo of our own report: the handler's MapZoom pushes the level we just
            // published straight back down. Writing ScaleFactor here would cancel PdfKit's
            // in-flight double-tap animation, and _zoom already holds the reported level,
            // so there is nothing to do. See ReportZoomIfChanged.
            if (_reportingZoom)
                return;

            _zoom = Math.Clamp(value, _minZoom, _maxZoom);
            _zoomNeedsApply = !TryApplyZoom(_zoom);

            // Caller-originated, so the caller already knows this level. Recording it keeps
            // ReportZoomIfChanged's threshold from swallowing a later gesture back to it.
            if (!_zoomNeedsApply)
                _lastReportedZoom = _zoom;
        }
    }

    // Divide by the fit scale ScaleFactor was last established against, not the live one:
    // during a re-fit the two differ, and only the former pairs with the current ScaleFactor.
    private float ReadZoom()
        => _appliedFitScale > 0 ? (float)(_pdfView.ScaleFactor / _appliedFitScale) : _zoom;

    /// <summary>
    /// Folds the level the control is currently showing — a pinch included — back into
    /// <see cref="_zoom"/>, so that whatever re-fits the control next can restore it.
    /// </summary>
    private void CaptureZoom()
    {
        if (_zoomNeedsApply || _appliedFitScale <= 0)
            return;

        _zoom = Math.Clamp(ReadZoom(), _minZoom, _maxZoom);
    }

    /// <summary>
    /// Re-clamps the current level after MinZoom/MaxZoom moved, and re-applies it.
    /// </summary>
    private void ReclampZoom()
    {
        CaptureZoom();
        Zoom = _zoom;
    }

    /// <summary>
    /// Pushes <see cref="_zoom"/> back to the control once a fit scale exists to express it
    /// against.
    /// </summary>
    private void SyncZoom()
    {
        if (_zoomNeedsApply && TryApplyZoom(_zoom))
        {
            _zoomNeedsApply = false;

            // The apply above posted its ScaleChanged while _zoomNeedsApply still muted
            // reporting, so publish the level that landed. Without this a re-fit — a reload,
            // a rotation — moves the control without the bound Zoom ever hearing about it.
            ReportZoomIfChanged();
        }
    }

    /// <returns><c>false</c> when the view has no fit scale yet, so nothing can be applied.</returns>
    private bool TryApplyZoom(float zoom)
    {
        var fitScale = GetFitScale();
        if (fitScale <= 0)
            return false;

        // ScaleFactor is now expressed against this fit scale; keep the pair in step.
        _appliedFitScale = fitScale;
        _pdfView.ScaleFactor = zoom * fitScale;
        return true;
    }

    public float MinZoom
    {
        get => _minZoom;
        set
        {
            if (Math.Abs(_minZoom - value) <= float.Epsilon)
                return;

            _minZoom = value;
            ApplyZoomConstraints();
            ReclampZoom();
        }
    }

    public float MaxZoom
    {
        get => _maxZoom;
        set
        {
            if (Math.Abs(_maxZoom - value) <= float.Epsilon)
                return;

            _maxZoom = value;
            ApplyZoomConstraints();
            ReclampZoom();
        }
    }

    /// <summary>
    /// The scale at which the document currently "fits" the view under the active
    /// <see cref="FitPolicy"/>. <see cref="MinZoom"/>/<see cref="MaxZoom"/> are multiples
    /// of this value — <c>MinZoom = 1.0</c> means "cannot zoom out past the fitted view" —
    /// which matches the semantics of the Android control. Returns 0 when the view has not
    /// been laid out yet and the scale is not computable.
    /// </summary>
    private nfloat GetFitScale()
        => _fitPolicy == FitPolicy.Width
            ? _pdfView.ScaleFactorForSizeToFit // AutoScales owns the scale in this mode.
            : _manualFitScale;                 // Computed by SetManualScale.

    /// <summary>
    /// Translates the relative <see cref="MinZoom"/>/<see cref="MaxZoom"/> multipliers into the
    /// absolute scale factors PdfKit expects. Without this, PdfKit's own defaults apply and the
    /// document can be pinched far below the fitted size regardless of what MinZoom is set to.
    /// </summary>
    private void ApplyZoomConstraints()
    {
        var fitScale = GetFitScale();
        if (fitScale <= 0)
            return; // Not laid out yet; retried from LayoutSubviewsAction.

        _appliedFitScale = fitScale;

        // Locking zoom pins both bounds to the fitted scale.
        _pdfView.MinScaleFactor = _enableZoom ? fitScale * _minZoom : fitScale;
        _pdfView.MaxScaleFactor = _enableZoom ? fitScale * _maxZoom : fitScale;

        // A Zoom that couldn't be resolved against a fit scale earlier, or one a re-fit
        // just discarded, is applied now that a scale exists.
        SyncZoom();
    }

    public int PageSpacing
    {
        get => (int)_pdfView.PageBreakMargins.Top;
        set
        {
            _pdfView.PageBreakMargins = new UIEdgeInsets(value, value, value, value);
        }
    }

    public FitPolicy FitPolicy
    {
        get => _fitPolicy;
        set
        {
            _fitPolicy = value;
            ApplyFitPolicy();
            _pdfView.SetNeedsLayout();
        }
    }

    private void ApplyFitPolicy()
    {
        // Fitting rewrites ScaleFactor, so bank the current level first; ApplyZoomConstraints
        // at the end of this method puts it back.
        CaptureZoom();
        _zoomNeedsApply = true;

        switch (_fitPolicy)
        {
            case FitPolicy.Width:
                _pdfView.AutoScales = true;
                _pdfView.DisplayMode = PdfKit.PdfDisplayMode.SinglePageContinuous;
                _needsFitReapply = false;
                break;

            case FitPolicy.Height:
                // PdfKit AutoScales only fits width; calculate scale manually.
                _pdfView.AutoScales = false;
                _pdfView.DisplayMode = PdfKit.PdfDisplayMode.SinglePageContinuous;
                _needsFitReapply = !SetManualScale(fitWidth: false, fitHeight: true);
                break;

            case FitPolicy.Both:
                // Fit to the smaller of the width/height scale factors so the whole page
                // is visible. Use SinglePageContinuous to avoid SinglePage's inflated
                // internal scroll content size.
                _pdfView.AutoScales = false;
                _pdfView.DisplayMode = PdfKit.PdfDisplayMode.SinglePageContinuous;
                _needsFitReapply = !SetManualScale(fitWidth: true, fitHeight: true);
                break;
        }

        // Zoom bounds are relative to the fit scale, which just changed.
        ApplyZoomConstraints();
    }

    /// <summary>
    /// Computes and applies ScaleFactor so the current page fits the requested axes.
    /// Returns <c>true</c> on success, <c>false</c> if the view or page isn't ready yet
    /// (caller should set <see cref="_needsFitReapply"/> and retry on next layout pass).
    /// </summary>
    private bool SetManualScale(bool fitWidth, bool fitHeight)
    {
        var page = _pdfView.CurrentPage;
        if (page == null)
            return false;

        var viewBounds = _pdfView.Bounds;
        if (viewBounds.Width <= 0 || viewBounds.Height <= 0)
            return false;

        var pageRect = page.GetBoundsForBox(PdfDisplayBox.Media);
        if (pageRect.Width <= 0 || pageRect.Height <= 0)
            return false;

        nfloat scale;
        if (fitWidth && fitHeight)
        {
            var scaleW = (nfloat)(viewBounds.Width / pageRect.Width);
            var scaleH = (nfloat)(viewBounds.Height / pageRect.Height);
            scale = scaleW < scaleH ? scaleW : scaleH;
        }
        else if (fitHeight)
        {
            scale = (nfloat)(viewBounds.Height / pageRect.Height);
        }
        else
        {
            scale = (nfloat)(viewBounds.Width / pageRect.Width);
        }

        _manualFitScale = scale;
        _pdfView.ScaleFactor = scale;
        return true;
    }

    public Abstractions.PdfDisplayMode DisplayMode
    {
        get
        {
            return _pdfView.DisplayMode switch
            {
                PdfKit.PdfDisplayMode.SinglePage => Abstractions.PdfDisplayMode.SinglePage,
                PdfKit.PdfDisplayMode.SinglePageContinuous => Abstractions.PdfDisplayMode.SinglePageContinuous,
                _ => Abstractions.PdfDisplayMode.SinglePageContinuous
            };
        }
        set
        {
            _pdfView.DisplayMode = value switch
            {
                Abstractions.PdfDisplayMode.SinglePage => PdfKit.PdfDisplayMode.SinglePage,
                Abstractions.PdfDisplayMode.SinglePageContinuous => PdfKit.PdfDisplayMode.SinglePageContinuous,
                _ => PdfKit.PdfDisplayMode.SinglePageContinuous
            };
            _pdfView.SetNeedsLayout();
        }
    }

    public PdfScrollOrientation ScrollOrientation
    {
        get => _scrollOrientation;
        set
        {
            _scrollOrientation = value;
            _pdfView.DisplayDirection = value == PdfScrollOrientation.Horizontal
                ? PdfDisplayDirection.Horizontal
                : PdfDisplayDirection.Vertical;
            _pdfView.SetNeedsLayout();
        }
    }

    public int DefaultPage
    {
        get => _defaultPage;
        set => _defaultPage = value;
    }

    public bool EnableAntialiasing
    {
        get => true; // iOS always uses antialiasing
        set { } // No-op on iOS
    }

    public bool UseBestQuality
    {
        get => true; // iOS always uses best quality
        set { } // No-op on iOS
    }

    public Color? BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            _backgroundColor = value;
            // Always apply, including for null: guarding on non-null left a previously
            // assigned colour in place, so clearing BackgroundColor had no effect.
            _pdfView.BackgroundColor = value != null
                ? UIColor.FromRGBA(
                    (float)value.Red,
                    (float)value.Green,
                    (float)value.Blue,
                    (float)value.Alpha)
                : UIColor.Clear;
            _pdfView.SetNeedsDisplay();
        }
    }

    public bool EnableAnnotationRendering
    {
        get => _enableAnnotationRendering;
        set
        {
            if (_enableAnnotationRendering != value)
            {
                _enableAnnotationRendering = value;
                UpdateAnnotationVisibility();
            }
        }
    }

    public PageAlignment PageAlignment
    {
        get => _pageAlignment;
        set
        {
            if (_pageAlignment == value)
                return;

            _pageAlignment = value;
            ApplyPageAlignment();
            // Force a layout pass so PdfKit immediately repositions content.
            // This is especially important when switching away from Top back to
            // Default/Center — without it the page stays pinned until the next
            // natural layout event (scroll, resize, etc.).
            _pdfView.SetNeedsLayout();
        }
    }

    /// <summary>
    /// PdfKit centers a short page by setting the inner PDFDocumentView's frame.Y
    /// to <c>(scrollView.Bounds.Height - pageHeight) / 2</c> on every layout pass.
    /// To pin the page to the top we walk the subview tree to find that document
    /// view and overwrite its frame's Y origin to 0. This must re-run on every
    /// layout pass because PdfKit re-applies its centering each time.
    ///
    /// HACK: relies on PdfKit's internal subview structure (UIScrollView > PDFDocumentView).
    /// Verified against iOS 16/17; revisit if PdfKit restructures.
    /// </summary>
    private void ApplyPageAlignment()
    {
        if (_pageAlignment != PageAlignment.Top)
            return;

        var scrollView = FindInnerScrollView(_pdfView);
        var documentView = FindDocumentView(scrollView);
        if (scrollView == null || documentView == null)
            return;

        var viewHeight = scrollView.Bounds.Height;
        var pageHeight = documentView.Frame.Height;
        if (viewHeight <= 0 || pageHeight <= 0 || pageHeight >= viewHeight)
            return; // content fills/exceeds viewport — nothing to align.

        var frame = documentView.Frame;
        if (frame.Y == 0)
            return; // already aligned to top this pass.

        documentView.Frame = new CoreGraphics.CGRect(frame.X, 0, frame.Width, frame.Height);
        // Reset scroll offset so the visible top of the page is at the viewport's top edge.
        scrollView.ContentOffset = new CoreGraphics.CGPoint(scrollView.ContentOffset.X, -scrollView.AdjustedContentInset.Top);
    }

    private static UIScrollView? FindInnerScrollView(UIView view)
    {
        if (view is UIScrollView sv)
            return sv;

        foreach (var sub in view.Subviews)
        {
            var found = FindInnerScrollView(sub);
            if (found != null)
                return found;
        }
        return null;
    }

    /// <summary>
    /// Locate PdfKit's PDFDocumentView — the direct child of the inner UIScrollView
    /// that hosts the rendered page(s). Identified by being the largest non-scroll-view
    /// subview, which is more robust than matching against a class name that PdfKit
    /// could change between iOS versions.
    /// </summary>
    private static UIView? FindDocumentView(UIScrollView? scrollView)
    {
        if (scrollView == null)
            return null;

        UIView? best = null;
        nfloat bestArea = 0;
        foreach (var sub in scrollView.Subviews)
        {
            if (sub is UIScrollView)
                continue;

            var area = sub.Frame.Width * sub.Frame.Height;
            if (area > bestArea)
            {
                bestArea = area;
                best = sub;
            }
        }
        return best;
    }

    public event EventHandler<DocumentLoadedEventArgs>? DocumentLoaded;
    public event EventHandler<PageChangedEventArgs>? PageChanged;
    public event EventHandler<PdfErrorEventArgs>? Error;
    public event EventHandler<LinkTappedEventArgs>? LinkTapped;
    public event EventHandler<PdfTappedEventArgs>? Tapped;
    public event EventHandler<RenderedEventArgs>? Rendered;
    public event EventHandler<ZoomChangedEventArgs>? ZoomChanged;
    public event EventHandler<AnnotationTappedEventArgs>? AnnotationTapped;

    public void GoToPage(int pageIndex)
    {
        if (_pdfView.Document == null)
            return;

        if (pageIndex < 0 || pageIndex >= PageCount)
            return;

        var page = _pdfView.Document.GetPage((nint)pageIndex);
        if (page != null)
        {
            _pdfView.GoToPage(page);
        }
    }

    public void Reload()
    {
        LoadDocument();
    }

    private void LoadDocument(bool preserveZoom = true)
    {
        // Assigning a new Document makes PdfKit re-fit, which resets the zoom. Settle on the
        // level to replay once the document is in and fitted — the Android control does the
        // same across its reload.
        if (preserveZoom)
            CaptureZoom();
        else
            _zoom = Math.Clamp(1.0f, _minZoom, _maxZoom);

        _zoomNeedsApply = true;

        if (_source == null)
        {
            _pdfView.Document = null;
            return;
        }

        try
        {
            PdfDocument? document = null;

            switch (_source)
            {
                case FilePdfSource fileSource:
                    var fileUrl = NSUrl.FromFilename(fileSource.FilePath);
                    document = new PdfDocument(fileUrl);
                    break;

                case UriPdfSource uriSource:
                    var url = new NSUrl(uriSource.Uri.AbsoluteUri);
                    document = new PdfDocument(url);
                    break;

                case StreamPdfSource streamSource:
                    document = new PdfDocument(NSData.FromStream(streamSource.Stream));
                    break;

                case BytesPdfSource bytesSource:
                    var bytesData = NSData.FromArray(bytesSource.Data);
                    document = new PdfDocument(bytesData);
                    break;

                case AssetPdfSource assetSource:
                    var assetPath = Path.Combine(NSBundle.MainBundle.BundlePath, assetSource.AssetName);
                    if (File.Exists(assetPath))
                    {
                        var assetUrl = NSUrl.FromFilename(assetPath);
                        document = new PdfDocument(assetUrl);
                    }
                    else
                    {
                        // Try Resources folder
                        var resourcePath = NSBundle.MainBundle.PathForResource(
                            Path.GetFileNameWithoutExtension(assetSource.AssetName),
                            Path.GetExtension(assetSource.AssetName));

                        if (!string.IsNullOrEmpty(resourcePath))
                        {
                            var resourceUrl = NSUrl.FromFilename(resourcePath);
                            document = new PdfDocument(resourceUrl);
                        }
                    }
                    break;
            }

            if (document != null)
            {
                // Check if document is locked and attempt to unlock with password
                if (document.IsLocked)
                {
                    if (!string.IsNullOrEmpty(_source.Password))
                    {
                        bool unlocked = document.Unlock(_source.Password);
                        if (!unlocked)
                        {
                            OnError(new PdfErrorEventArgs("Failed to unlock PDF: incorrect password"));
                            return;
                        }
                    }
                    else
                    {
                        OnError(new PdfErrorEventArgs("PDF is password-protected but no password was provided"));
                        return;
                    }
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _pdfView.Document = document;
                    // Re-apply fit policy now that the document is loaded and
                    // the view has valid bounds to calculate scale against.
                    ApplyFitPolicy();
                });

                // Get document metadata
                var pageCount = (int)document.PageCount;
                var title = document.DocumentAttributes?["Title"]?.ToString();
                var author = document.DocumentAttributes?["Author"]?.ToString();
                var subject = document.DocumentAttributes?["Subject"]?.ToString();

                DocumentLoaded?.Invoke(this, new DocumentLoadedEventArgs(
                    pageCount,
                    title,
                    author,
                    subject));

                // A page assigned through CurrentPage before the document arrived wins over
                // DefaultPage: it is the more specific, more recent instruction.
                var requestedPage = _pendingPage ?? _defaultPage;
                _pendingPage = null;

                if (requestedPage > 0 && requestedPage < pageCount)
                {
                    var page = document.GetPage((nint)requestedPage);
                    if (page != null)
                    {
                        _pdfView.GoToPage(page);
                    }
                }

                // Trigger initial page changed event
                var currentPageIndex = requestedPage > 0 && requestedPage < pageCount ? requestedPage : 0;
                PageChanged?.Invoke(this, new PageChangedEventArgs(currentPageIndex, pageCount));

                // Apply annotation visibility setting
                UpdateAnnotationVisibility();

                // Fire rendered event after a short delay to ensure rendering is complete
                if (!_documentLoaded)
                {
                    _documentLoaded = true;
                    Task.Delay(100).ContinueWith(_ =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            Rendered?.Invoke(this, new RenderedEventArgs(pageCount));
                        });
                    });
                }
            }
            else
            {
                Error?.Invoke(this, new PdfErrorEventArgs("Failed to load PDF document", null));
            }
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new PdfErrorEventArgs($"Error loading PDF: {ex.Message}", ex));
        }
    }

    private void UpdateAnnotationVisibility()
    {
        if (_pdfView.Document == null)
            return;

        // Iterate through all pages and hide/show annotations
        for (nint i = 0; i < _pdfView.Document.PageCount; i++)
        {
            var page = _pdfView.Document.GetPage(i);
            if (page?.Annotations != null)
            {
                foreach (var annotation in page.Annotations)
                {
                    // Set annotation to hidden or visible
                    annotation.ShouldDisplay = _enableAnnotationRendering;
                }
            }
        }

        // Force refresh the view
        MainThread.BeginInvokeOnMainThread(() => _pdfView.SetNeedsDisplay());
    }

    /// <summary>
    /// Publishes the level the control is actually showing, so a caller bound to Zoom sees
    /// a pinch or double-tap.
    ///
    /// PdfKit posts ScaleChanged for its own re-fits too — assigning a document, a layout
    /// pass, a rotation — and during those the fit scale ScaleFactor is expressed against is
    /// mid-flight, so the ratio would be nonsense. <see cref="_zoomNeedsApply"/> is set
    /// across exactly those windows, which is what makes it the right guard here.
    /// </summary>
    private void ReportZoomIfChanged()
    {
        if (_disposed || _zoomNeedsApply || _appliedFitScale <= 0)
            return;

        var zoom = Math.Clamp(ReadZoom(), _minZoom, _maxZoom);

        // Float noise from the ScaleFactor/fit-scale division would otherwise republish on
        // every frame of a pinch without the value meaningfully moving.
        if (Math.Abs(zoom - _lastReportedZoom) < ZoomReportThreshold)
            return;

        _lastReportedZoom = zoom;
        _zoom = zoom;

        // The report round-trips synchronously — ZoomChanged, the handler, PdfView's Zoom
        // property, MapZoom, and back into our own Zoom setter. MapZoom breaks the loop by
        // comparing against the live native scale, which is a moving target mid-animation,
        // so it would write ScaleFactor back and cancel PdfKit's double-tap zoom part-way.
        // The flag is the loop-breaker instead: the control is already at this level.
        _reportingZoom = true;
        try
        {
            ZoomChanged?.Invoke(this, new ZoomChangedEventArgs(zoom));
        }
        finally
        {
            _reportingZoom = false;
        }
    }

    /// <summary>
    /// Installs (or re-installs) the passthrough delegate on PdfKit's scroll view, which is
    /// where the double-tap zoom becomes observable: <c>scrollViewDidZoom:</c> fires for a
    /// programmatic animated zoom as well as for a pinch, unlike <c>scrollViewDidEndZooming:</c>.
    ///
    /// The slot has to be proxied rather than taken. It holds PdfKit's own delegate, without
    /// which the scroll view will not scroll, page or zoom, and .NET's <c>DidZoom</c> event
    /// would evict it — that is the "Event registration is overwriting existing delegate"
    /// exception. Assigning WeakDelegate directly avoids the event shim entirely.
    /// </summary>
    private void EnsureZoomDelegateProxy()
    {
        if (_disposed)
            return;

        if (_zoomScrollView?.Superview == null)
        {
            // The same scroll view ApplyPageAlignment reaches into, which keeps the two hacks
            // agreeing on which view PdfKit is actually driving. PdfKit builds it lazily and
            // can replace it across a document load, hence the re-check every layout pass.
            _zoomScrollView = FindInnerScrollView(_pdfView);
        }

        if (_zoomScrollView == null)
            return;

        // Already ours; a reference compare is the whole steady-state cost.
        if (ReferenceEquals(_zoomScrollView.WeakDelegate, _zoomDelegateProxy))
            return;

        // PdfKit has either not set its delegate yet or has reclaimed the slot. Wrap whatever
        // is in there now rather than reinstating a stale capture.
        var proxy = new ZoomReportingScrollViewDelegate(this, _zoomScrollView);
        _zoomScrollView.WeakDelegate = proxy;

        _zoomDelegateProxy?.Dispose();
        _zoomDelegateProxy = proxy;
    }

    /// <summary>
    /// Hands the delegate slot back to PdfKit. Restoring rather than nulling matters: leaving
    /// it empty would strip PdfKit of its own delegate on the way out.
    /// </summary>
    private void RemoveZoomDelegateProxy()
    {
        if (_zoomDelegateProxy == null)
            return;

        if (_zoomScrollView != null &&
            ReferenceEquals(_zoomScrollView.WeakDelegate, _zoomDelegateProxy))
        {
            _zoomScrollView.WeakDelegate = _zoomDelegateProxy.Original;
        }

        _zoomDelegateProxy.Dispose();
        _zoomDelegateProxy = null;
    }

    private void OnPageChangedNotification(NSNotification notification)
    {
        if (_pdfView.Document != null && _pdfView.CurrentPage != null)
        {
            var pageIndex = (int)_pdfView.Document.GetPageIndex(_pdfView.CurrentPage);
            var pageCount = (int)_pdfView.Document.PageCount;

            PageChanged?.Invoke(this, new PageChangedEventArgs(pageIndex, pageCount));
        }
    }

    private void OnAnnotationHit(object? sender, PdfViewAnnotationHitEventArgs e)
    {
        if (_pdfView.Document == null)
            return;

        // Extract annotation from the notification user info
        var userInfo = e.Notification.UserInfo;
        if (userInfo == null)
            return;

        // Get the annotation object from the user info dictionary
        var annotationKey = new NSString("PDFAnnotationHit");
        if (!userInfo.ContainsKey(annotationKey))
            return;

        var annotationObject = userInfo[annotationKey];
        if (annotationObject is PdfAnnotation annotation)
        {
            // Get the page index for this annotation
            var page = annotation.Page;
            if (page == null)
                return;

            var pageIndex = (int)_pdfView.Document.GetPageIndex(page);

            // Extract annotation information
            string annotationType;
            try
            {
                annotationType = annotation.AnnotationType.ToString();
            }
            catch (NotSupportedException)
            {
                // Use the runtime class name when PdfKit lacks a managed enum for the annotation.
                var runtimeName = annotation.GetType()?.Name;
                annotationType = !string.IsNullOrEmpty(runtimeName) ? $"Custom({runtimeName})" : "Unknown";
            }
            var contents = annotation.Contents ?? string.Empty;
            var bounds = annotation.Bounds;

            // Create and fire the event
            var args = new AnnotationTappedEventArgs(
                pageIndex,
                annotationType,
                contents,
                new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height)
            );

            AnnotationTapped?.Invoke(this, args);
        }
    }

    private void OnError(PdfErrorEventArgs args)
    {
        Error?.Invoke(this, args);
    }

    private void HandleTap(UITapGestureRecognizer recognizer)
    {
        var location = recognizer.LocationInView(_pdfView);
        var pageIndex = CurrentPage;

        // Convert location to page coordinates
        var page = _pdfView.CurrentPage;
        if (page != null)
        {
            var pagePoint = _pdfView.ConvertPointToPage(location, page);
            Tapped?.Invoke(this, new PdfTappedEventArgs(pageIndex, (float)pagePoint.X, (float)pagePoint.Y));
        }
        else
        {
            Tapped?.Invoke(this, new PdfTappedEventArgs(pageIndex, (float)location.X, (float)location.Y));
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_pageChangedObserver != null)
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(_pageChangedObserver);
            _pageChangedObserver?.Dispose();
            _pageChangedObserver = null;
        }

        if (_annotationHitObserver != null)
        {
            _annotationHitObserver?.Dispose();
            _annotationHitObserver = null;
        }

        if (_scaleChangedObserver != null)
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(_scaleChangedObserver);
            _scaleChangedObserver?.Dispose();
            _scaleChangedObserver = null;
        }

        if (_tapGestureRecognizer != null)
        {
            _pdfView.RemoveGestureRecognizer(_tapGestureRecognizer);
            _tapGestureRecognizer?.Dispose();
            _tapGestureRecognizer = null;
        }

        RemoveZoomDelegateProxy();
        _zoomScrollView = null;

        // Drop the layout hooks before disposing: UIKit can still lay the view out during
        // teardown, and they call back into this instance.
        _pdfView.WillLayoutSubviewsAction = null;
        _pdfView.LayoutSubviewsAction = null;
        _pdfView.WeakDelegate = null;
        _pdfView?.Dispose();
    }

    /// <summary>
    /// Custom PdfView subclass that fires a callback on each layout pass, allowing
    /// <see cref="PdfViewiOS"/> to defer fit-policy scale calculations until the view
    /// has non-zero bounds (which isn't guaranteed when the document first loads).
    /// </summary>
    private class NativePdfView : PdfKit.PdfView
    {
        internal Action? WillLayoutSubviewsAction { get; set; }

        internal Action? LayoutSubviewsAction { get; set; }

        public override void LayoutSubviews()
        {
            // Before base: PdfKit re-fits here, and the pre-fit scale is only readable now.
            WillLayoutSubviewsAction?.Invoke();
            base.LayoutSubviews();
            LayoutSubviewsAction?.Invoke();
        }
    }

    /// <summary>
    /// A passthrough <see cref="UIScrollViewDelegate"/> for PdfKit's internal scroll view:
    /// it captures <c>scrollViewDidZoom:</c> so a double-tap zoom can be reported, and
    /// forwards every other message to the delegate it displaced.
    ///
    /// Every protocol method is overridden and forwarded — a partial proxy would silently
    /// deny PdfKit the callbacks it relies on to scroll, page and zoom.
    /// </summary>
    private sealed class ZoomReportingScrollViewDelegate : UIScrollViewDelegate
    {
        private const string SelScrolled = "scrollViewDidScroll:";
        private const string SelDidZoom = "scrollViewDidZoom:";
        private const string SelDraggingStarted = "scrollViewWillBeginDragging:";
        private const string SelWillEndDragging = "scrollViewWillEndDragging:withVelocity:targetContentOffset:";
        private const string SelDraggingEnded = "scrollViewDidEndDragging:willDecelerate:";
        private const string SelDecelerationStarted = "scrollViewWillBeginDecelerating:";
        private const string SelDecelerationEnded = "scrollViewDidEndDecelerating:";
        private const string SelScrollAnimationEnded = "scrollViewDidEndScrollingAnimation:";
        private const string SelViewForZooming = "viewForZoomingInScrollView:";
        private const string SelZoomingStarted = "scrollViewWillBeginZooming:withView:";
        private const string SelZoomingEnded = "scrollViewDidEndZooming:withView:atScale:";
        private const string SelShouldScrollToTop = "scrollViewShouldScrollToTop:";
        private const string SelScrolledToTop = "scrollViewDidScrollToTop:";
        private const string SelDidChangeInset = "scrollViewDidChangeAdjustedContentInset:";

        /// <summary>
        /// The selectors this class both overrides and forwards, and therefore the only ones
        /// it will answer <c>respondsToSelector:</c> for on the original's behalf.
        /// </summary>
        private static readonly HashSet<string> ForwardableSelectors = new()
        {
            SelScrolled, SelDraggingStarted, SelWillEndDragging, SelDraggingEnded,
            SelDecelerationStarted, SelDecelerationEnded, SelScrollAnimationEnded,
            SelViewForZooming, SelZoomingStarted, SelZoomingEnded, SelShouldScrollToTop,
            SelScrolledToTop, SelDidChangeInset,
        };

        private readonly WeakReference<PdfViewiOS> _owner;
        private readonly IUIScrollViewDelegate? _original;

        public ZoomReportingScrollViewDelegate(PdfViewiOS owner, UIScrollView scrollView)
        {
            _owner = new WeakReference<PdfViewiOS>(owner);

            // Captured twice over: as NSObject to answer respondsToSelector: on its behalf,
            // and as the protocol interface to forward through with the right signatures.
            Original = scrollView.WeakDelegate;
            _original = Original != null
                ? Runtime.GetINativeObject<IUIScrollViewDelegate>(Original.Handle, owns: false)
                : null;
        }

        /// <summary>The delegate this proxy displaced, so Dispose can put it back.</summary>
        internal NSObject? Original { get; }

        /// <summary>
        /// The generated <see cref="IUIScrollViewDelegate"/> extension methods do not guard on
        /// <c>respondsToSelector:</c>, so forwarding blind to an original that lacks the method
        /// would be an unrecognised-selector crash.
        /// </summary>
        private bool Forwards(string selector)
            => _original != null && Original?.RespondsToSelector(new Selector(selector)) == true;

        /// <summary>
        /// UIScrollView branches on which delegate methods exist — most sharply
        /// <c>viewForZoomingInScrollView:</c>, which gates zooming altogether — so the proxy has
        /// to present the same profile as the delegate it stands in front of.
        /// </summary>
        public override bool RespondsToSelector(Selector? sel)
        {
            var name = sel?.Name;

            // Ours unconditionally: it is the whole point of the proxy.
            if (name == SelDidZoom)
                return true;

            // Answer for the original, but only for selectors this class actually overrides. An
            // unknown one — a method a future iOS adds that this class does not implement — is
            // declined rather than mirrored, because claiming a selector we cannot forward is an
            // unrecognised-selector crash. PdfKit loses that one callback, not the app.
            if (name != null && ForwardableSelectors.Contains(name))
                return Original?.RespondsToSelector(sel) ?? false;

            return base.RespondsToSelector(sel);
        }

        public override void DidZoom(UIScrollView scrollView)
        {
            if (Forwards(SelDidZoom))
                _original!.DidZoom(scrollView);

            Console.WriteLine("Got scrollViewDidZoom");

            if (_owner.TryGetTarget(out var owner))
                owner.ReportZoomIfChanged();
        }

        public override void Scrolled(UIScrollView scrollView)
        {
            if (Forwards(SelScrolled))
                _original!.Scrolled(scrollView);
        }

        public override void DraggingStarted(UIScrollView scrollView)
        {
            if (Forwards(SelDraggingStarted))
                _original!.DraggingStarted(scrollView);
        }

        public override void WillEndDragging(UIScrollView scrollView, CoreGraphics.CGPoint velocity, ref CoreGraphics.CGPoint targetContentOffset)
        {
            if (Forwards(SelWillEndDragging))
                _original!.WillEndDragging(scrollView, velocity, ref targetContentOffset);
        }

        public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate)
        {
            if (Forwards(SelDraggingEnded))
                _original!.DraggingEnded(scrollView, willDecelerate);
        }

        public override void DecelerationStarted(UIScrollView scrollView)
        {
            if (Forwards(SelDecelerationStarted))
                _original!.DecelerationStarted(scrollView);
        }

        public override void DecelerationEnded(UIScrollView scrollView)
        {
            if (Forwards(SelDecelerationEnded))
                _original!.DecelerationEnded(scrollView);
        }

        public override void ScrollAnimationEnded(UIScrollView scrollView)
        {
            if (Forwards(SelScrollAnimationEnded))
                _original!.ScrollAnimationEnded(scrollView);
        }

        // The forward that matters most: nil here disables zooming outright. The signature is
        // annotated non-nullable, but nil is a legal answer to UIKit and means "nothing to
        // zoom" — and the RespondsToSelector mirroring means UIScrollView only ever asks when
        // PdfKit's delegate answers, so the null branch is unreachable in practice.
        public override UIView ViewForZoomingInScrollView(UIScrollView scrollView)
            => Forwards(SelViewForZooming) ? _original!.ViewForZoomingInScrollView(scrollView) : null!;

        public override void ZoomingStarted(UIScrollView scrollView, UIView? view)
        {
            if (Forwards(SelZoomingStarted))
                _original!.ZoomingStarted(scrollView, view!);
        }

        public override void ZoomingEnded(UIScrollView scrollView, UIView? withView, nfloat atScale)
        {
            if (Forwards(SelZoomingEnded))
                _original!.ZoomingEnded(scrollView, withView!, atScale);
        }

        // true is UIKit's own default for an unimplemented delegate.
        public override bool ShouldScrollToTop(UIScrollView scrollView)
            => Forwards(SelShouldScrollToTop) ? _original!.ShouldScrollToTop(scrollView) : true;

        public override void ScrolledToTop(UIScrollView scrollView)
        {
            if (Forwards(SelScrolledToTop))
                _original!.ScrolledToTop(scrollView);
        }

        public override void DidChangeAdjustedContentInset(UIScrollView scrollView)
        {
            if (Forwards(SelDidChangeInset))
                _original!.DidChangeAdjustedContentInset(scrollView);
        }
    }

    /// <summary>
    /// Delegate implementation to intercept link clicks in PDFView.
    /// </summary>
    private class PdfViewDelegateImpl : PdfViewDelegate
    {
        private readonly WeakReference<PdfViewiOS> _owner;

        public PdfViewDelegateImpl(PdfViewiOS owner)
        {
            _owner = new WeakReference<PdfViewiOS>(owner);
        }

        [Export("PDFViewWillClickOnLink:withURL:")]
        public override void WillClickOnLink(PdfKit.PdfView sender, NSUrl url)
        {
            if (!_owner.TryGetTarget(out var owner))
            {
                return;
            }

            // Fire the LinkTapped event
            var args = new LinkTappedEventArgs(url.AbsoluteString, null);
            owner.LinkTapped?.Invoke(owner, args);

            // If the event was not handled and navigation is enabled, open the URL
            if (!args.Handled && owner.EnableLinkNavigation)
            {
                UIKit.UIApplication.SharedApplication.OpenUrl(url, new UIApplicationOpenUrlOptions { OpenInPlace = true }, _ => { });
            }
        }
    }
}





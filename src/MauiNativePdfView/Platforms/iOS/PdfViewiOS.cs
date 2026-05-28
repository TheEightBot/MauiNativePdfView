using MauiNativePdfView.Abstractions;
using PdfKit;
using UIKit;
using Foundation;

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

    public PdfViewiOS()
    {
        _pdfView = new NativePdfView
        {
            AutoScales = true,
            DisplayMode = PdfKit.PdfDisplayMode.SinglePageContinuous,
            DisplayDirection = PdfDisplayDirection.Vertical
        };

        // Re-apply deferred fit policy once the view has been laid out and has real bounds.
        // Page alignment is re-applied every layout pass so it survives PdfKit's
        // internal re-centering when the document or scale changes.
        _pdfView.LayoutSubviewsAction = () =>
        {
            if (_needsFitReapply)
                ApplyFitPolicy();

            ApplyPageAlignment();
        };

        // Subscribe to page change notifications
        _pageChangedObserver = NSNotificationCenter.DefaultCenter.AddObserver(
            PdfKit.PdfView.PageChangedNotification,
            OnPageChangedNotification,
            _pdfView);

        // Subscribe to annotation hit notifications
        _annotationHitObserver = PdfKit.PdfView.Notifications.ObserveAnnotationHit(OnAnnotationHit);

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
            LoadDocument();
        }
    }

    public int CurrentPage => _pdfView.Document != null && _pdfView.CurrentPage != null
        ? (int)_pdfView.Document.GetPageIndex(_pdfView.CurrentPage)
        : 0;

    public int PageCount => _pdfView.Document != null ? (int)_pdfView.Document.PageCount : 0;

    public bool EnableZoom
    {
        get => _pdfView.MinScaleFactor < _pdfView.MaxScaleFactor;
        set
        {
            if (value)
            {
                _pdfView.MinScaleFactor = MinZoom;
                _pdfView.MaxScaleFactor = MaxZoom;
            }
            else
            {
                var currentScale = _pdfView.ScaleFactor;
                _pdfView.MinScaleFactor = currentScale;
                _pdfView.MaxScaleFactor = currentScale;
            }
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

    public float Zoom
    {
        get => (float)_pdfView.ScaleFactor;
        set => _pdfView.ScaleFactor = value;
    }

    public float MinZoom { get; set; } = 1.0f;

    public float MaxZoom { get; set; } = 3.0f;

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
            if (value != null)
            {
                _pdfView.BackgroundColor = UIColor.FromRGBA(
                    (float)value.Red,
                    (float)value.Green,
                    (float)value.Blue,
                    (float)value.Alpha);
                _pdfView.SetNeedsDisplay();
            }
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

    private void LoadDocument()
    {
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

                // Navigate to default page if specified
                if (_defaultPage > 0 && _defaultPage < pageCount)
                {
                    var page = document.GetPage((nint)_defaultPage);
                    if (page != null)
                    {
                        _pdfView.GoToPage(page);
                    }
                }

                // Trigger initial page changed event
                var currentPageIndex = _defaultPage > 0 && _defaultPage < pageCount ? _defaultPage : 0;
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

        if (_tapGestureRecognizer != null)
        {
            _pdfView.RemoveGestureRecognizer(_tapGestureRecognizer);
            _tapGestureRecognizer?.Dispose();
            _tapGestureRecognizer = null;
        }

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
        internal Action? LayoutSubviewsAction { get; set; }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            LayoutSubviewsAction?.Invoke();
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
        public void WillClickOnLink(PdfKit.PdfView sender, NSUrl url)
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
                UIKit.UIApplication.SharedApplication.OpenUrl(url);
            }
        }
    }
}





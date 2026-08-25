using Android.Content;
using Android.Views;
using Com.Ahmer.Pdfviewer;
using Com.Ahmer.Pdfviewer.Link;
using Com.Ahmer.Pdfviewer.Listener;
using Com.Ahmer.Pdfviewer.Model;
using Com.Ahmer.Pdfviewer.Util;
using MauiNativePdfView.Abstractions;
using Java.IO;

namespace MauiNativePdfView.Platforms.Android;

/// <summary>
/// Android implementation of IPdfView using AhmerPdfium PDFView.
/// Wraps PDFView using composition since it's sealed.
/// </summary>
public class PdfViewAndroid : IPdfView, IDisposable
{
    private readonly PDFView _pdfView;
    private PdfSource? _source;
    private bool _enableZoom = true;
    private bool _enableSwipe = true;
    private bool _enableLinkNavigation = true;
    private bool _enableTapGestures = true;
    private float _minZoom = 1.0f;
    private float _maxZoom = 3.0f;
    private int _pageSpacing = 10;
    private Abstractions.FitPolicy _fitPolicy = Abstractions.FitPolicy.Width;
    private Abstractions.PdfDisplayMode _displayMode = Abstractions.PdfDisplayMode.SinglePageContinuous;
    private PdfScrollOrientation _scrollOrientation = PdfScrollOrientation.Vertical;
    private int _defaultPage = 0;
    private bool _enableAntialiasing = true;
    private bool _useBestQuality = true;
    private Color? _backgroundColor;
    private bool _enableAnnotationRendering = true;
    private PageAlignment _pageAlignment = PageAlignment.Default;
    private int _currentPage = 0;
    private int _pageCount = 0;
    private bool _disposed;
    private float _zoom = 1.0f;
    private bool _zoomNeedsApply;
    private readonly HashSet<int> _openedPages = new();

    private TapListener? _tapListener;

    public PdfViewAndroid(Context context)
    {
        _pdfView = new PDFView(context, null);
        // AhmerPdfViewer's native default paints an opaque background, which hides anything
        // sharing the PdfView's grid cell. Start transparent so an unset MAUI BackgroundColor
        // composites the way callers expect.
        _pdfView.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
    }

    /// <summary>
    /// Gets the native PDFView instance.
    /// </summary>
    public PDFView NativeView => _pdfView;

    #region IPdfView Implementation

    public PdfSource? Source
    {
        get => _source;
        set
        {
            if (_source != value)
            {
                _source = value;
                // A different document starts fitted; only an internal reload keeps the
                // level the user was at.
                LoadDocument(preserveZoom: false);
            }
        }
    }

    /// <inheritdoc />
    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (_pageCount > 0)
            {
                GoToPage(value);
                return;
            }

            // No document yet. LoadDocument replays _currentPage as the page to restore,
            // so recording it here is what makes a page assigned before the load stick.
            if (value >= 0)
                _currentPage = value;
        }
    }

    public int PageCount => _pageCount;

    public bool EnableZoom
    {
        get => _enableZoom;
        set => _enableZoom = value;
    }

    public bool EnableSwipe
    {
        get => _enableSwipe;
        set => _enableSwipe = value;
    }

    public bool EnableLinkNavigation
    {
        get => _enableLinkNavigation;
        set => _enableLinkNavigation = value;
    }

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

            if (_pageCount > 0)
            {
                Reload();
            }
        }
    }

    /// <inheritdoc />
    public float Zoom
    {
        // While a level is waiting to be applied, ours is the truth: the control is still
        // showing whatever it reset itself to.
        get => _zoomNeedsApply ? _zoom : ReadZoom();
        set
        {
            _zoom = Math.Clamp(value, _minZoom, _maxZoom);
            _zoomNeedsApply = !TryApplyZoom(_zoom);
        }
    }

    private float ReadZoom() => _pageCount > 0 ? _pdfView.Zoom : _zoom;

    /// <summary>
    /// Folds the level the control is currently showing — a pinch included — back into
    /// <see cref="_zoom"/>, so that whatever resets the control next can restore it.
    /// </summary>
    private void CaptureZoom()
    {
        if (!_zoomNeedsApply && _pageCount > 0)
            _zoom = Math.Clamp(_pdfView.Zoom, _minZoom, _maxZoom);
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
    /// Pushes <see cref="_zoom"/> back to the control once it can accept one. Posted so it
    /// runs after the layout pass that follows a load.
    /// </summary>
    private void SyncZoom()
    {
        if (!_zoomNeedsApply)
            return;

        _pdfView.Post(() =>
        {
            if (!_disposed && _zoomNeedsApply && TryApplyZoom(_zoom))
                _zoomNeedsApply = false;
        });
    }

    /// <summary>
    /// Applies a zoom level the way AhmerPdfViewer applies its own.
    ///
    /// PDFView.ZoomTo() is a bare field assignment: it neither re-clamps the scroll offsets
    /// nor re-renders, so the control keeps offsets that describe the *previous* zoom level.
    /// The next tap runs the library's link hit-test, which maps the touch through the new
    /// zoom and the stale offset, resolves a page the renderer never opened, and hands
    /// PdfiumCore a page index missing from its cache. PdfiumCore substitutes -1 for the
    /// missing native page pointer and passes it to pdfium unguarded, which dereferences it
    /// and takes the process down with SIGSEGV at 0xffffffff.
    ///
    /// ZoomCenteredTo() recomputes and clamps the offsets around a pivot (via MoveTo), and
    /// LoadPages() re-renders the visible parts at the new scale. That pair is exactly what
    /// ZoomWithAnimation drives on each animation frame and at its end, which is why
    /// animating the zoom sidesteps the crash.
    /// </summary>
    /// <returns><c>false</c> when the control is not ready to zoom yet.</returns>
    private bool TryApplyZoom(float zoom)
    {
        // MoveTo() no-ops without a loaded document, and the pivot needs real bounds.
        // Either way the offsets would stay stale, which is the state we're avoiding.
        if (_pageCount == 0 || _pdfView.Width <= 0 || _pdfView.Height <= 0)
            return false;

        if (Math.Abs(_pdfView.Zoom - zoom) > float.Epsilon)
        {
            _pdfView.ZoomCenteredTo(zoom, new global::Android.Graphics.PointF(_pdfView.Width / 2f, _pdfView.Height / 2f));
            _pdfView.LoadPages();
            // Re-settles the page under a snapping display mode, as the animated path does.
            _pdfView.PerformPageSnap();
            // The viewport now covers a different set of pages.
            EnsureVisiblePagesOpen();
        }

        return true;
    }

    /// <summary>
    /// Opens every page a touch inside the current viewport could resolve to.
    ///
    /// AhmerPdfium opens pages lazily, on its background rendering thread — but the tap
    /// hit-test runs synchronously on the UI thread. DragPinchManager.checkLinkTapped
    /// resolves the page under the touch and asks PdfiumCore for that page's links, and
    /// PdfiumCore.pagePtr() answers -1 for any page missing from its cache, then hands that
    /// -1 to pdfium as an FPDF_PAGE with no guard. pdfium dereferences it and the process
    /// dies with SIGSEGV. Tapping a page the renderer has not reached yet — after a fling,
    /// or a zoom that brought new pages into view — is enough to hit it.
    ///
    /// Opening those pages up front means the pointer is always real by the time a tap can
    /// reach them. PdfFile.OpenPage records what it has opened and no-ops on repeat, so this
    /// costs one native page open per page actually visited.
    /// </summary>
    private void EnsureVisiblePagesOpen()
    {
        if (_disposed)
            return;

        var pdfFile = _pdfView.PdfFile;
        if (pdfFile == null)
            return;

        int pageCount = pdfFile.PagesCount;
        if (pageCount <= 0)
            return;

        float zoom = _pdfView.Zoom;
        bool vertical = _pdfView.SwipeVertical;
        float viewportStart = vertical ? -_pdfView.CurrentYOffset : -_pdfView.CurrentXOffset;
        float viewportEnd = viewportStart + (vertical ? _pdfView.Height : _pdfView.Width);

        // One page of slack either side: a page can scroll into view before the current page
        // changes and brings us back here.
        int first = Math.Max(pdfFile.GetPageAtOffset(viewportStart, zoom) - 1, 0);
        int last = Math.Min(pdfFile.GetPageAtOffset(viewportEnd, zoom) + 1, pageCount - 1);

        for (int page = first; page <= last; page++)
        {
            // PdfFile.OpenPage takes the same lock PdfiumCore holds for the whole of a native
            // page render, and it takes it even when the page is already open. Remembering
            // what we've asked for keeps this off that lock once a page has been covered —
            // which matters because this runs on the UI thread while a fling is in flight.
            if (!_openedPages.Add(page))
                continue;

            try
            {
                pdfFile.OpenPage(page);
            }
            catch (Exception ex)
            {
                // A page that will not open cannot be rendered either — leave it to the
                // library's own error path to report rather than failing the whole sweep.
                // It stays in the set: the library also records the failure and won't retry.
                System.Diagnostics.Debug.WriteLine($"MauiNativePdfView: could not open page {page}: {ex.Message}");
            }
        }
    }

    public float MinZoom
    {
        get => _minZoom;
        set
        {
            _minZoom = value;
            // Push to the native control, not just the backing field: the field alone only
            // clamps the Zoom property, leaving pinch gestures bound by AhmerPdfViewer's
            // own defaults.
            _pdfView.MinZoom = value;
            ReclampZoom();
        }
    }

    public float MaxZoom
    {
        get => _maxZoom;
        set
        {
            _maxZoom = value;
            _pdfView.MaxZoom = value;
            ReclampZoom();
        }
    }

    public int PageSpacing
    {
        get => _pageSpacing;
        set
        {
            if (_pageSpacing != value)
            {
                _pageSpacing = value;
                if (_pageCount > 0) // Document is loaded
                    Reload();
            }
        }
    }

    public Abstractions.FitPolicy FitPolicy
    {
        get => _fitPolicy;
        set
        {
            if (_fitPolicy != value)
            {
                _fitPolicy = value;
                if (_pageCount > 0) // Document is loaded
                    Reload();
            }
        }
    }

    public Abstractions.PdfDisplayMode DisplayMode
    {
        get => _displayMode;
        set
        {
            if (_displayMode != value)
            {
                _displayMode = value;
                if (_pageCount > 0) // Document is loaded
                    Reload();
            }
        }
    }

    public PdfScrollOrientation ScrollOrientation
    {
        get => _scrollOrientation;
        set
        {
            if (_scrollOrientation != value)
            {
                _scrollOrientation = value;
                if (_pageCount > 0) // Document is loaded
                    Reload();
            }
        }
    }

    public int DefaultPage
    {
        get => _defaultPage;
        set => _defaultPage = value;
    }

    public bool EnableAntialiasing
    {
        get => _enableAntialiasing;
        set
        {
            if (_enableAntialiasing != value)
            {
                _enableAntialiasing = value;
                if (_pageCount > 0) // Document is loaded
                    Reload();
            }
        }
    }

    public bool UseBestQuality
    {
        get => _useBestQuality;
        set
        {
            if (_useBestQuality != value)
            {
                _useBestQuality = value;
                if (_pageCount > 0) // Document is loaded
                    Reload();
            }
        }
    }

    public Color? BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            _backgroundColor = value;
            // Always apply, including for null: guarding on non-null left a previously
            // assigned colour in place, so clearing BackgroundColor had no effect.
            var androidColor = value != null
                ? global::Android.Graphics.Color.Argb(
                    (int)(value.Alpha * 255),
                    (int)(value.Red * 255),
                    (int)(value.Green * 255),
                    (int)(value.Blue * 255))
                : global::Android.Graphics.Color.Transparent;
            _pdfView.SetBackgroundColor(androidColor);
            _pdfView.Invalidate();
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
                if (_pageCount > 0) // Document is loaded
                    Reload();
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
            if (_pageCount > 0)
                ApplyPageAlignment();
        }
    }

    /// <summary>
    /// AhmerPdfium clamps the scroll offset to keep a short page centered, and MAUI
    /// overrides any LayoutParameters change with its own Layout() call, so neither
    /// MoveTo nor LayoutParameters.Height work as alignment hooks.
    ///
    /// Instead we use <see cref="global::Android.Views.View.TranslationY"/> — a pure
    /// visual transform that shifts the entire native view (and its drawn page) up
    /// by half the slack. The library still centers the page within its own viewport,
    /// but the translated view places that centered page flush with the top of the
    /// MAUI control's allocated area. The corresponding empty area moves below the
    /// MAUI control's bounds, where the parent's background shows through.
    /// </summary>
    private void ApplyPageAlignment()
    {
        if (_disposed)
            return;

        _pdfView.Post(ApplyPageAlignmentOnUiThread);
    }

    private void ApplyPageAlignmentOnUiThread()
    {
        if (_disposed)
            return;

        if (_pageAlignment != PageAlignment.Top || _pageCount == 0)
        {
            if (_pdfView.TranslationY != 0f)
                _pdfView.TranslationY = 0f;
            return;
        }

        int pageIndex = _currentPage >= 0 && _currentPage < _pageCount ? _currentPage : 0;
        var pageSize = _pdfView.GetPageSize(pageIndex);
        if (pageSize == null || pageSize.Width <= 0 || pageSize.Height <= 0)
            return;

        int viewportWidth = _pdfView.Width;
        int viewportHeight = _pdfView.Height;
        if (viewportWidth <= 0 || viewportHeight <= 0)
            return;

        float scale = _fitPolicy switch
        {
            Abstractions.FitPolicy.Height => viewportHeight / pageSize.Height,
            Abstractions.FitPolicy.Both => Math.Min(viewportWidth / pageSize.Width, viewportHeight / pageSize.Height),
            _ => viewportWidth / pageSize.Width,
        };
        float renderedHeight = pageSize.Height * scale * _pdfView.Zoom;

        if (renderedHeight <= 0 || renderedHeight >= viewportHeight)
        {
            if (_pdfView.TranslationY != 0f)
                _pdfView.TranslationY = 0f;
            return;
        }

        float slack = viewportHeight - renderedHeight;
        float translateY = -slack / 2f;
        if (Math.Abs(_pdfView.TranslationY - translateY) > 0.5f)
            _pdfView.TranslationY = translateY;
    }

    public event EventHandler<DocumentLoadedEventArgs>? DocumentLoaded;
    public event EventHandler<PageChangedEventArgs>? PageChanged;
    public event EventHandler<PdfErrorEventArgs>? Error;
    public event EventHandler<LinkTappedEventArgs>? LinkTapped;
    public event EventHandler<PdfTappedEventArgs>? Tapped;
    public event EventHandler<RenderedEventArgs>? Rendered;

    /// <summary>
    /// This event is not supported on Android with the current AhmerPdfium library.
    /// Annotation tap detection is only available on iOS.
    /// </summary>
    public event EventHandler<AnnotationTappedEventArgs>? AnnotationTapped;

    public void GoToPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < _pageCount)
        {
            _pdfView.JumpTo(pageIndex);
        }
    }

    public void Reload()
    {
        LoadDocument();
    }

    #endregion

    private void LoadDocument(bool preserveZoom = true)
    {
        if (_source == null)
            return;

        // Store current page to restore after reload
        int pageToRestore = _currentPage;

        // PDFView.recycle() runs as part of Load() and resets the native zoom to 1, so
        // settle on the level to replay afterwards — the same treatment the current page
        // gets — and mark it unapplied.
        if (preserveZoom)
            CaptureZoom();
        else
            _zoom = Math.Clamp(1.0f, _minZoom, _maxZoom);

        _zoomNeedsApply = true;

        // The PdfFile behind these — and so every native page it had open — is replaced.
        _openedPages.Clear();

        try
        {
            var configurator = _source switch
            {
                FilePdfSource fileSource => _pdfView.FromFile(new Java.IO.File(fileSource.FilePath)),
                UriPdfSource uriSource => _pdfView.FromUri(global::Android.Net.Uri.Parse(uriSource.Uri.ToString())),
                StreamPdfSource streamSource => _pdfView.FromStream(streamSource.Stream),
                BytesPdfSource bytesSource => _pdfView.FromBytes(bytesSource.Data),
                AssetPdfSource assetSource => _pdfView.FromAsset(assetSource.AssetName),
                _ => throw new NotSupportedException($"PDF source type {_source.GetType().Name} is not supported.")
            };

            ConfigureAndLoad(configurator, pageToRestore);
        }
        catch (Exception ex)
        {
            OnError(new PdfErrorEventArgs($"Failed to load PDF document: {ex.Message}", ex));
        }
    }

    private void ConfigureAndLoad(PDFView.Configurator configurator, int pageToRestore = -1)
    {
        // Determine page snap and fling based on display mode
        bool enablePageSnap = _displayMode == Abstractions.PdfDisplayMode.SinglePage;
        bool enablePageFling = _displayMode == Abstractions.PdfDisplayMode.SinglePage;

        // Set password if provided
        if (!string.IsNullOrEmpty(_source?.Password))
        {
            configurator.Password(_source.Password);
        }

        var nativeFitPolicy = _fitPolicy switch
        {
            Abstractions.FitPolicy.Height => Com.Ahmer.Pdfviewer.Util.FitPolicy.Height,
            Abstractions.FitPolicy.Both => Com.Ahmer.Pdfviewer.Util.FitPolicy.Both,
            _ => Com.Ahmer.Pdfviewer.Util.FitPolicy.Width,
        };

        configurator
            .EnableSwipe(_enableSwipe)
            .EnableDoubleTap(_enableZoom)
            .SwipeHorizontal(_scrollOrientation == PdfScrollOrientation.Horizontal)
            .DefaultPage(pageToRestore >= 0 ? pageToRestore : _defaultPage)
            .AutoSpacing(false)
            .Spacing(_pageSpacing)
            .PageSnap(enablePageSnap)
            .PageFling(enablePageFling)
            .NightMode(false)
            .FitEachPage(false)
            .PageFitPolicy(nativeFitPolicy)
            .EnableAntialiasing(_enableAntialiasing)
            .OnLoad(new LoadCompleteListener(this, pageToRestore))
            .OnPageChange(new PageChangeListener(this))
            .OnError(new ErrorListener(this))
            .OnTap(_enableTapGestures ? _tapListener ??= new TapListener(this) : null)
            .OnRender(new RenderListener(this));

        // Note: UseBestQuality sets rendering quality (ARGB_8888 vs RGB_565)
        // This is handled by the PDFView configuration automatically based on device capabilities

        if (_enableAnnotationRendering)
        {
            configurator.EnableAnnotationRendering(true);
        }

        if (_enableLinkNavigation)
        {
            configurator.LinkHandler(new LinkHandlerImpl(this));
        }

        configurator.Load();
    }

    private void OnDocumentLoaded(int pageCount)
    {
        _pageCount = pageCount;
        // Before ApplyPageAlignment: the alignment maths reads the native zoom.
        SyncZoom();
        EnsureVisiblePagesOpen();
        ApplyPageAlignment();
        DocumentLoaded?.Invoke(this, new DocumentLoadedEventArgs(pageCount));
    }

    private void OnDocumentLoadedWithPageRestore(int pageCount, int pageToRestore)
    {
        _pageCount = pageCount;

        // Restore the page if valid
        if (pageToRestore >= 0 && pageToRestore < pageCount)
        {
            _pdfView.JumpTo(pageToRestore);
        }

        SyncZoom();
        EnsureVisiblePagesOpen();
        ApplyPageAlignment();
        DocumentLoaded?.Invoke(this, new DocumentLoadedEventArgs(pageCount));
    }

    private void OnPageChanged(int pageIndex, int pageCount)
    {
        _currentPage = pageIndex;
        _pageCount = pageCount;
        EnsureVisiblePagesOpen();
        PageChanged?.Invoke(this, new PageChangedEventArgs(pageIndex, pageCount));
    }

    private void OnError(PdfErrorEventArgs args)
    {
        Error?.Invoke(this, args);
    }

    private void OnLinkTapped(LinkTappedEventArgs args)
    {
        LinkTapped?.Invoke(this, args);
    }

    private void OnTapped(int pageIndex, float x, float y)
    {
        Tapped?.Invoke(this, new PdfTappedEventArgs(pageIndex, x, y));
    }

    private void OnRendered(int pageCount)
    {
        // Re-apply once after the first render — the library may center the page
        // again as part of its post-render layout pass, and a Zoom set before the view
        // had bounds only becomes applicable once something has actually been drawn.
        SyncZoom();
        EnsureVisiblePagesOpen();
        ApplyPageAlignment();
        Rendered?.Invoke(this, new RenderedEventArgs(pageCount));
    }

    #region Helper Methods

    private PDFView.Configurator FromStream(Stream stream)
    {
        var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return _pdfView.FromBytes(memoryStream.ToArray());
    }

    #endregion

    #region Listener Implementations

    private class LoadCompleteListener : Java.Lang.Object, IOnLoadCompleteListener
    {
        private readonly WeakReference<PdfViewAndroid> _viewRef;
        private readonly int _pageToRestore;

        public LoadCompleteListener(PdfViewAndroid view, int pageToRestore = -1)
        {
            _viewRef = new WeakReference<PdfViewAndroid>(view);
            _pageToRestore = pageToRestore;
        }

        public void LoadComplete(int nbPages)
        {
            if (_viewRef.TryGetTarget(out var view))
            {
                if (_pageToRestore >= 0)
                {
                    view.OnDocumentLoadedWithPageRestore(nbPages, _pageToRestore);
                }
                else
                {
                    view.OnDocumentLoaded(nbPages);
                }
            }
        }
    }

    private class PageChangeListener : Java.Lang.Object, IOnPageChangeListener
    {
        private readonly WeakReference<PdfViewAndroid> _viewRef;

        public PageChangeListener(PdfViewAndroid view)
        {
            _viewRef = new WeakReference<PdfViewAndroid>(view);
        }

        public void OnPageChanged(int page, int pageCount)
        {
            if (_viewRef.TryGetTarget(out var view))
            {
                view.OnPageChanged(page, pageCount);
            }
        }
    }

    private class ErrorListener : Java.Lang.Object, IOnErrorListener
    {
        private readonly WeakReference<PdfViewAndroid> _viewRef;

        public ErrorListener(PdfViewAndroid view)
        {
            _viewRef = new WeakReference<PdfViewAndroid>(view);
        }

        public void OnError(Java.Lang.Throwable? t)
        {
            if (_viewRef.TryGetTarget(out var view))
            {
                var message = t?.Message ?? "Unknown error occurred";
                view.OnError(new PdfErrorEventArgs(message));
            }
        }
    }

    private class LinkHandlerImpl : Java.Lang.Object, ILinkHandler
    {
        private readonly WeakReference<PdfViewAndroid> _viewRef;

        public LinkHandlerImpl(PdfViewAndroid view)
        {
            _viewRef = new WeakReference<PdfViewAndroid>(view);
        }

        public void HandleLinkEvent(LinkTapEvent? linkTapEvent)
        {
            if (_viewRef.TryGetTarget(out var view) && linkTapEvent != null)
            {
                var link = linkTapEvent.Link;
                var args = new LinkTappedEventArgs(
                    link?.Uri,
                    null  // DestPageIdx not available in this version
                );

                view.OnLinkTapped(args);

                // If not handled by the user, use default behavior
                if (!args.Handled)
                {
                    new DefaultLinkHandler(view._pdfView).HandleLinkEvent(linkTapEvent);
                }
            }
        }
    }

    private class TapListener : Java.Lang.Object, IOnTapListener
    {
        private readonly WeakReference<PdfViewAndroid> _viewRef;

        public TapListener(PdfViewAndroid view)
        {
            _viewRef = new WeakReference<PdfViewAndroid>(view);
        }

        public bool OnTap(MotionEvent? e)
        {
            if (_viewRef.TryGetTarget(out var view) && e != null)
            {
                view.OnTapped(view.CurrentPage, e.GetX(), e.GetY());
            }

            return false; // allow PDFView to continue default handling (links, etc.)
        }
    }

    private class RenderListener : Java.Lang.Object, IOnRenderListener
    {
        private readonly WeakReference<PdfViewAndroid> _viewRef;

        public RenderListener(PdfViewAndroid view)
        {
            _viewRef = new WeakReference<PdfViewAndroid>(view);
        }

        public void OnInitiallyRendered(int nbPages)
        {
            if (_viewRef.TryGetTarget(out var view))
            {
                view.OnRendered(nbPages);
            }
        }
    }

    #endregion

    public void Dispose()
    {
        if (_disposed)
            return;

        // Set before the peer goes away: work already queued on the view's message loop,
        // and library callbacks still in flight, run after this and would otherwise touch
        // a disposed Java object and throw on the UI thread.
        _disposed = true;

        if (_tapListener != null)
        {
            _tapListener.Dispose();
            _tapListener = null;
        }

        _pdfView?.Dispose();
    }
}

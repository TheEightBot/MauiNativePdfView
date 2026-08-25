using MauiNativePdfView.Abstractions;

namespace MauiNativePdfView;

/// <summary>
/// Cross-platform PDF viewer control for .NET MAUI.
/// </summary>
public class PdfView : View
{
    /// <summary>
    /// Bindable property for the PDF source.
    /// </summary>
    public static readonly BindableProperty SourceProperty =
        BindableProperty.Create(
            nameof(Source),
            typeof(PdfSource),
            typeof(PdfView),
            null,
            propertyChanged: OnSourceChanged);

    /// <summary>
    /// Bindable property for enabling zoom gestures.
    /// </summary>
    public static readonly BindableProperty EnableZoomProperty =
        BindableProperty.Create(
            nameof(EnableZoom),
            typeof(bool),
            typeof(PdfView),
            true);

    /// <summary>
    /// Bindable property for enabling swipe gestures.
    /// </summary>
    public static readonly BindableProperty EnableSwipeProperty =
        BindableProperty.Create(
            nameof(EnableSwipe),
            typeof(bool),
            typeof(PdfView),
            true);

    /// <summary>
    /// Bindable property for enabling tap gesture interception (used for PdfTapped event).
    /// </summary>
    public static readonly BindableProperty EnableTapGesturesProperty =
        BindableProperty.Create(
            nameof(EnableTapGestures),
            typeof(bool),
            typeof(PdfView),
            false);

    /// <summary>
    /// Bindable property for enabling link navigation.
    /// </summary>
    public static readonly BindableProperty EnableLinkNavigationProperty =
        BindableProperty.Create(
            nameof(EnableLinkNavigation),
            typeof(bool),
            typeof(PdfView),
            true);

    /// <summary>
    /// Bindable property for zoom level.
    /// </summary>
    public static readonly BindableProperty ZoomProperty =
        BindableProperty.Create(
            nameof(Zoom),
            typeof(float),
            typeof(PdfView),
            1.0f);

    /// <summary>
    /// Bindable property for minimum zoom level.
    /// </summary>
    public static readonly BindableProperty MinZoomProperty =
        BindableProperty.Create(
            nameof(MinZoom),
            typeof(float),
            typeof(PdfView),
            1.0f);

    /// <summary>
    /// Bindable property for maximum zoom level.
    /// </summary>
    public static readonly BindableProperty MaxZoomProperty =
        BindableProperty.Create(
            nameof(MaxZoom),
            typeof(float),
            typeof(PdfView),
            3.0f);

    /// <summary>
    /// Bindable property for page spacing.
    /// </summary>
    public static readonly BindableProperty PageSpacingProperty =
        BindableProperty.Create(
            nameof(PageSpacing),
            typeof(int),
            typeof(PdfView),
            10);

    /// <summary>
    /// Bindable property for fit policy.
    /// </summary>
    public static readonly BindableProperty FitPolicyProperty =
        BindableProperty.Create(
            nameof(FitPolicy),
            typeof(FitPolicy),
            typeof(PdfView),
            FitPolicy.Width);

    /// <summary>
    /// Bindable property for display mode.
    /// </summary>
    public static readonly BindableProperty DisplayModeProperty =
        BindableProperty.Create(
            nameof(DisplayMode),
            typeof(PdfDisplayMode),
            typeof(PdfView),
            PdfDisplayMode.SinglePageContinuous);

    /// <summary>
    /// Bindable property for scroll orientation.
    /// </summary>
    public static readonly BindableProperty ScrollOrientationProperty =
        BindableProperty.Create(
            nameof(ScrollOrientation),
            typeof(PdfScrollOrientation),
            typeof(PdfView),
            PdfScrollOrientation.Vertical);

    /// <summary>
    /// Bindable property for default page.
    /// </summary>
    public static readonly BindableProperty DefaultPageProperty =
        BindableProperty.Create(
            nameof(DefaultPage),
            typeof(int),
            typeof(PdfView),
            0);

    /// <summary>
    /// Bindable property for enabling antialiasing.
    /// </summary>
    public static readonly BindableProperty EnableAntialiasingProperty =
        BindableProperty.Create(
            nameof(EnableAntialiasing),
            typeof(bool),
            typeof(PdfView),
            true);

    /// <summary>
    /// Bindable property for using best quality rendering.
    /// </summary>
    public static readonly BindableProperty UseBestQualityProperty =
        BindableProperty.Create(
            nameof(UseBestQuality),
            typeof(bool),
            typeof(PdfView),
            true);

    /// <summary>
    /// Bindable property for annotation rendering.
    /// </summary>
    public static readonly BindableProperty EnableAnnotationRenderingProperty =
        BindableProperty.Create(
            nameof(EnableAnnotationRendering),
            typeof(bool),
            typeof(PdfView),
            true);

    /// <summary>
    /// Bindable property for vertical page alignment when content is shorter than the viewport.
    /// </summary>
    public static readonly BindableProperty PageAlignmentProperty =
        BindableProperty.Create(
            nameof(PageAlignment),
            typeof(PageAlignment),
            typeof(PdfView),
            PageAlignment.Default);

    /// <summary>
    /// Gets or sets the PDF source to display.
    /// </summary>
    public PdfSource? Source
    {
        get => (PdfSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Gets or sets whether zoom gestures are enabled.
    /// </summary>
    public bool EnableZoom
    {
        get => (bool)GetValue(EnableZoomProperty);
        set => SetValue(EnableZoomProperty, value);
    }

    /// <summary>
    /// Gets or sets whether swipe gestures are enabled.
    /// </summary>
    public bool EnableSwipe
    {
        get => (bool)GetValue(EnableSwipeProperty);
        set => SetValue(EnableSwipeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether tap gestures should be intercepted to raise <see cref="PdfTapped"/>.
    /// Disable this to allow platform link handling without custom tap interception.
    /// </summary>
    public bool EnableTapGestures
    {
        get => (bool)GetValue(EnableTapGesturesProperty);
        set => SetValue(EnableTapGesturesProperty, value);
    }

    /// <summary>
    /// Gets or sets whether link navigation is enabled.
    /// </summary>
    public bool EnableLinkNavigation
    {
        get => (bool)GetValue(EnableLinkNavigationProperty);
        set => SetValue(EnableLinkNavigationProperty, value);
    }

    /// <summary>
    /// Gets or sets the current zoom level, as a multiple of the scale at which the document
    /// fits the view under the active <see cref="FitPolicy"/>. <c>1.0</c> is the fitted
    /// document and <c>2.0</c> is twice that size — it is not a percentage of the PDF's
    /// intrinsic size. Values are clamped to <see cref="MinZoom"/>/<see cref="MaxZoom"/>.
    /// </summary>
    public float Zoom
    {
        get => (float)GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum zoom level, as a multiple of the fit scale.
    /// <c>1.0</c> prevents zooming out past the fitted document.
    /// </summary>
    public float MinZoom
    {
        get => (float)GetValue(MinZoomProperty);
        set => SetValue(MinZoomProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum zoom level, as a multiple of the fit scale.
    /// <c>3.0</c> allows zooming in to three times the fitted document.
    /// </summary>
    public float MaxZoom
    {
        get => (float)GetValue(MaxZoomProperty);
        set => SetValue(MaxZoomProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between pages in pixels.
    /// </summary>
    public int PageSpacing
    {
        get => (int)GetValue(PageSpacingProperty);
        set => SetValue(PageSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets how pages should fit on screen.
    /// </summary>
    public FitPolicy FitPolicy
    {
        get => (FitPolicy)GetValue(FitPolicyProperty);
        set => SetValue(FitPolicyProperty, value);
    }

    /// <summary>
    /// Gets or sets the display mode for PDF pages (single page, continuous, two-up, etc.).
    /// </summary>
    public PdfDisplayMode DisplayMode
    {
        get => (PdfDisplayMode)GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the scroll direction for page navigation.
    /// </summary>
    public PdfScrollOrientation ScrollOrientation
    {
        get => (PdfScrollOrientation)GetValue(ScrollOrientationProperty);
        set => SetValue(ScrollOrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the default page to display when the document loads (0-based).
    /// </summary>
    public int DefaultPage
    {
        get => (int)GetValue(DefaultPageProperty);
        set => SetValue(DefaultPageProperty, value);
    }

    /// <summary>
    /// Gets or sets whether antialiasing is enabled (Android only, iOS always uses antialiasing).
    /// </summary>
    public bool EnableAntialiasing
    {
        get => (bool)GetValue(EnableAntialiasingProperty);
        set => SetValue(EnableAntialiasingProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to use best quality rendering (Android only).
    /// </summary>
    public bool UseBestQuality
    {
        get => (bool)GetValue(UseBestQualityProperty);
        set => SetValue(UseBestQualityProperty, value);
    }

    /// <summary>
    /// Gets or sets the background color of the PDF viewer.
    /// </summary>
    public new Color? BackgroundColor
    {
        get => (Color?)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    /// <summary>
    /// Gets or sets whether PDF annotations (forms, comments, highlights, etc.) should be rendered.
    /// On Android: Controls whether annotations are displayed.
    /// On iOS: Always enabled (PdfKit renders annotations by default).
    /// </summary>
    public bool EnableAnnotationRendering
    {
        get => (bool)GetValue(EnableAnnotationRenderingProperty);
        set => SetValue(EnableAnnotationRenderingProperty, value);
    }

    /// <summary>
    /// Gets or sets how the page is aligned vertically inside the viewport when the
    /// rendered content is shorter than the view (e.g. a single short page, or a
    /// landscape page in a portrait viewport with <see cref="FitPolicy.Width"/>).
    /// Defaults to <see cref="PageAlignment.Default"/>, which preserves each platform's
    /// native placement (vertically centered on both iOS and Android). Has no visible
    /// effect once the content fills or exceeds the viewport.
    /// </summary>
    public PageAlignment PageAlignment
    {
        get => (PageAlignment)GetValue(PageAlignmentProperty);
        set => SetValue(PageAlignmentProperty, value);
    }

    /// <summary>
    /// Bindable property for the current page. Two-way by default: assigning it navigates,
    /// and it is updated as the visible page changes.
    /// </summary>
    public static readonly BindableProperty CurrentPageProperty =
        BindableProperty.Create(
            nameof(CurrentPage),
            typeof(int),
            typeof(PdfView),
            0,
            defaultBindingMode: BindingMode.TwoWay);

    private static readonly BindablePropertyKey PageCountPropertyKey =
        BindableProperty.CreateReadOnly(
            nameof(PageCount),
            typeof(int),
            typeof(PdfView),
            0);

    /// <summary>
    /// Bindable property for the page count. Read-only: it describes the loaded document,
    /// so bind it as a source (<c>Mode=OneWayToSource</c>) rather than as a target.
    /// </summary>
    public static readonly BindableProperty PageCountProperty = PageCountPropertyKey.BindableProperty;

    /// <summary>
    /// Gets or sets the current page number (0-based).
    ///
    /// Assigning it navigates, exactly as <see cref="GoToPage"/> does; a page assigned
    /// before the document has loaded is honoured once it does, rather than dropped. The
    /// property is updated whenever the visible page changes — by swipe, by fling, or by a
    /// <see cref="GoToPage"/> call — so a <c>TwoWay</c> binding tracks the document both ways.
    ///
    /// A page outside the document is ignored, and the property reverts to the page actually
    /// being shown rather than keeping the rejected value — so a binding cannot be left
    /// describing a page that was never navigated to.
    /// </summary>
    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    /// <summary>
    /// Gets the total number of pages in the document.
    /// </summary>
    public int PageCount
    {
        get => (int)GetValue(PageCountProperty);
        private set => SetValue(PageCountPropertyKey, value);
    }

    /// <summary>
    /// Occurs when the document has finished loading.
    /// </summary>
    public event EventHandler<DocumentLoadedEventArgs>? DocumentLoaded;

    /// <summary>
    /// Occurs when the current page changes.
    /// </summary>
    public event EventHandler<PageChangedEventArgs>? PageChanged;

    /// <summary>
    /// Occurs when an error occurs.
    /// </summary>
    public event EventHandler<PdfErrorEventArgs>? Error;

    /// <summary>
    /// Occurs when a link is tapped.
    /// </summary>
    public event EventHandler<LinkTappedEventArgs>? LinkTapped;

    /// <summary>
    /// Occurs when the PDF is tapped (single tap).
    /// </summary>
    public event EventHandler<PdfTappedEventArgs>? Tapped;

    /// <summary>
    /// Occurs when the PDF has finished rendering for the first time.
    /// </summary>
    public event EventHandler<RenderedEventArgs>? Rendered;

    /// <summary>
    /// Occurs when an annotation is tapped in the PDF.
    /// Platform availability: iOS only. Android does not support annotation tap detection with the current library.
    /// </summary>
    public event EventHandler<AnnotationTappedEventArgs>? AnnotationTapped;

    /// <summary>
    /// Navigates to the specified page.
    /// </summary>
    public void GoToPage(int pageIndex)
    {
        Handler?.Invoke(nameof(IPdfView.GoToPage), pageIndex);
    }

    /// <summary>
    /// Reloads the current document.
    /// </summary>
    public void Reload()
    {
        Handler?.Invoke(nameof(IPdfView.Reload));
    }

    internal void RaiseDocumentLoaded(DocumentLoadedEventArgs args)
    {
        PageCount = args.PageCount;
        DocumentLoaded?.Invoke(this, args);
    }

    internal void RaisePageChanged(PageChangedEventArgs args)
    {
        CurrentPage = args.PageIndex;
        PageCount = args.PageCount;
        PageChanged?.Invoke(this, args);
    }

    internal void RaiseError(PdfErrorEventArgs args)
    {
        Error?.Invoke(this, args);
    }

    internal void RaiseLinkTapped(LinkTappedEventArgs args)
    {
        LinkTapped?.Invoke(this, args);
    }

    internal void RaiseTapped(PdfTappedEventArgs args)
    {
        Tapped?.Invoke(this, args);
    }

    internal void RaiseRendered(RenderedEventArgs args)
    {
        Rendered?.Invoke(this, args);
    }

    internal void RaiseAnnotationTapped(AnnotationTappedEventArgs args)
    {
        AnnotationTapped?.Invoke(this, args);
    }

    private static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdfView view && view.Handler != null)
        {
            view.Handler.UpdateValue(nameof(Source));
        }
    }
}

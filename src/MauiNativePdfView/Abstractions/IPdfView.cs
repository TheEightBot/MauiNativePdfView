namespace MauiNativePdfView.Abstractions;

/// <summary>
/// Cross-platform interface for PDF viewer functionality.
/// </summary>
public interface IPdfView
{
    /// <summary>
    /// Gets or sets the PDF source to display.
    /// </summary>
    PdfSource? Source { get; set; }

    /// <summary>
    /// Gets or sets the current page number (0-based index).
    ///
    /// A set navigates when the document is loaded, and is otherwise remembered and applied
    /// once it loads — assigning a page before the document is ready is honoured rather than
    /// dropped, matching how <see cref="Zoom"/> behaves.
    ///
    /// A page outside the document is ignored: <see cref="CurrentPage"/> keeps reporting the
    /// page actually being shown, which is what lets the handler revert the virtual view
    /// rather than leave it holding a rejected value.
    /// </summary>
    int CurrentPage { get; set; }

    /// <summary>
    /// Gets the total number of pages in the document.
    /// </summary>
    int PageCount { get; }

    /// <summary>
    /// Gets or sets whether zoom gestures are enabled (pinch, double-tap).
    /// </summary>
    bool EnableZoom { get; set; }

    /// <summary>
    /// Gets or sets whether swipe gestures are enabled for page navigation.
    /// </summary>
    bool EnableSwipe { get; set; }

    /// <summary>
    /// Gets or sets whether links in the PDF are clickable.
    /// </summary>
    bool EnableLinkNavigation { get; set; }

    /// <summary>
    /// Gets or sets the current zoom level, as a multiple of the scale at which the document
    /// fits the view under the active <see cref="FitPolicy"/> (1.0 = fitted).
    ///
    /// Platform implementations follow one contract, so the property behaves the same
    /// everywhere:
    /// <list type="number">
    ///   <item>A set is clamped to <see cref="MinZoom"/>/<see cref="MaxZoom"/> and stored.</item>
    ///   <item>The stored level is pushed to the native control as soon as it can accept one.
    ///     Assigning Zoom before the document is loaded or the view is measured is therefore
    ///     honoured rather than dropped.</item>
    ///   <item>A get returns what the control is actually showing — so a pinch is reflected —
    ///     falling back to the stored level while the control has nothing to report.</item>
    ///   <item>The level survives events that reset the native control from underneath the
    ///     caller: any property change that forces a reload, an explicit <see cref="Reload"/>,
    ///     and a change in the fitted scale (rotation, resize). Whatever the user was looking
    ///     at, pinch included, is restored afterwards. Assigning a new <see cref="Source"/> is
    ///     the deliberate exception — a different document starts fitted at 1.0.</item>
    ///   <item>Changing <see cref="MinZoom"/> or <see cref="MaxZoom"/> re-clamps the current
    ///     level into the new bounds.</item>
    /// </list>
    /// </summary>
    float Zoom { get; set; }

    /// <summary>
    /// Gets or sets the minimum zoom level, as a multiple of the fit scale.
    /// Changing it re-clamps <see cref="Zoom"/>.
    /// </summary>
    float MinZoom { get; set; }

    /// <summary>
    /// Gets or sets the maximum zoom level, as a multiple of the fit scale.
    /// Changing it re-clamps <see cref="Zoom"/>.
    /// </summary>
    float MaxZoom { get; set; }

    /// <summary>
    /// Gets or sets the spacing between pages (in pixels).
    /// </summary>
    int PageSpacing { get; set; }

    /// <summary>
    /// Gets or sets how pages should fit on screen initially.
    /// </summary>
    FitPolicy FitPolicy { get; set; }

    /// <summary>
    /// Gets or sets the display mode for PDF pages (single page, continuous, two-up, etc.).
    /// </summary>
    PdfDisplayMode DisplayMode { get; set; }

    /// <summary>
    /// Gets or sets the scroll direction for page navigation.
    /// </summary>
    PdfScrollOrientation ScrollOrientation { get; set; }

    /// <summary>
    /// Gets or sets the default page to display when the document loads (0-based index).
    /// </summary>
    int DefaultPage { get; set; }

    /// <summary>
    /// Gets or sets whether antialiasing is enabled for rendering (Android only, iOS always uses antialiasing).
    /// </summary>
    bool EnableAntialiasing { get; set; }

    /// <summary>
    /// Gets or sets whether to use best quality rendering (ARGB_8888 vs RGB_565, Android only).
    /// </summary>
    bool UseBestQuality { get; set; }

    /// <summary>
    /// Gets or sets the background color of the PDF viewer.
    /// </summary>
    Color? BackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets whether PDF annotations (forms, comments, highlights, etc.) should be rendered.
    /// On Android: Controls whether annotations are displayed.
    /// On iOS: Always enabled (PdfKit renders annotations by default).
    /// </summary>
    bool EnableAnnotationRendering { get; set; }

    /// <summary>
    /// Gets or sets how the page is aligned vertically inside the viewport when the
    /// rendered content is shorter than the view. Defaults to <see cref="PageAlignment.Default"/>,
    /// which preserves each platform's native placement (vertically centered on both
    /// iOS and Android). Has no visible effect once the content fills or exceeds the
    /// viewport (e.g. multi-page documents in continuous scroll).
    /// </summary>
    PageAlignment PageAlignment { get; set; }

    /// <summary>
    /// Occurs when the document has finished loading.
    /// </summary>
    event EventHandler<DocumentLoadedEventArgs>? DocumentLoaded;

    /// <summary>
    /// Occurs when the current page changes.
    /// </summary>
    event EventHandler<PageChangedEventArgs>? PageChanged;

    /// <summary>
    /// Occurs when the zoom level being shown changes, including changes the user drives
    /// with a pinch or a double-tap. <see cref="Zoom"/> already reflects the new level when
    /// this is raised.
    /// </summary>
    event EventHandler<ZoomChangedEventArgs>? ZoomChanged;

    /// <summary>
    /// Occurs when an error occurs during loading or rendering.
    /// </summary>
    event EventHandler<PdfErrorEventArgs>? Error;

    /// <summary>
    /// Occurs when a link is tapped in the PDF.
    /// </summary>
    event EventHandler<LinkTappedEventArgs>? LinkTapped;

    /// <summary>
    /// Occurs when the PDF is tapped (single tap).
    /// </summary>
    event EventHandler<PdfTappedEventArgs>? Tapped;

    /// <summary>
    /// Occurs when the PDF has finished rendering for the first time.
    /// </summary>
    event EventHandler<RenderedEventArgs>? Rendered;

    /// <summary>
    /// Occurs when an annotation is tapped in the PDF.
    /// Platform availability: iOS only. Android does not support annotation tap detection with the current library.
    /// </summary>
    event EventHandler<AnnotationTappedEventArgs>? AnnotationTapped;

    /// <summary>
    /// Navigates to the specified page.
    /// </summary>
    /// <param name="pageIndex">The 0-based page index.</param>
    void GoToPage(int pageIndex);

    /// <summary>
    /// Reloads the current document.
    /// </summary>
    void Reload();
}

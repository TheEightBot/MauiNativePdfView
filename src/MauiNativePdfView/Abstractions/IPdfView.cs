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
    /// Gets the current page number (0-based index).
    /// </summary>
    int CurrentPage { get; }

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
    /// Gets or sets the current zoom level (1.0 = 100%).
    /// </summary>
    float Zoom { get; set; }

    /// <summary>
    /// Gets or sets the minimum zoom level.
    /// </summary>
    float MinZoom { get; set; }

    /// <summary>
    /// Gets or sets the maximum zoom level.
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
    /// Occurs when the document has finished loading.
    /// </summary>
    event EventHandler<DocumentLoadedEventArgs>? DocumentLoaded;

    /// <summary>
    /// Occurs when the current page changes.
    /// </summary>
    event EventHandler<PageChangedEventArgs>? PageChanged;

    /// <summary>
    /// Occurs when an error occurs during loading or rendering.
    /// </summary>
    event EventHandler<PdfErrorEventArgs>? Error;

    /// <summary>
    /// Occurs when a link is tapped in the PDF.
    /// </summary>
    event EventHandler<LinkTappedEventArgs>? LinkTapped;

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

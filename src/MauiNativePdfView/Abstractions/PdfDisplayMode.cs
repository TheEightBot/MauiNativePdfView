namespace MauiNativePdfView.Abstractions;

/// <summary>
/// Defines how PDF pages are displayed and paginated.
/// </summary>
public enum PdfDisplayMode
{
    /// <summary>
    /// Displays a single page at a time with page snapping.
    /// User swipes move one full page at a time.
    /// </summary>
    SinglePage,

    /// <summary>
    /// Displays pages in a continuously scrolling single column.
    /// Default mode - allows smooth scrolling through all pages.
    /// </summary>
    SinglePageContinuous,

    /// <summary>
    /// Displays two pages side by side.
    /// Useful for viewing spreads or comparing pages.
    /// </summary>
    TwoUp,

    /// <summary>
    /// Displays pages in a continuously scrolling double column.
    /// Shows two pages side by side with continuous scrolling.
    /// </summary>
    TwoUpContinuous
}

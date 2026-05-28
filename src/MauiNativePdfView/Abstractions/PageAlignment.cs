namespace MauiNativePdfView.Abstractions;

/// <summary>
/// Controls how a page is aligned within the viewport when the rendered page
/// is shorter than the available view height (e.g. a single short page, or a
/// landscape page in a portrait viewport with <see cref="FitPolicy.Width"/>).
/// Has no visible effect when the content already fills or exceeds the viewport.
/// </summary>
public enum PageAlignment
{
    /// <summary>
    /// Use the platform's default placement (PdfKit and AhmerPdfium both center
    /// a short page vertically).
    /// </summary>
    Default,

    /// <summary>
    /// Pin the page to the top of the viewport.
    /// </summary>
    Top,

    /// <summary>
    /// Center the page vertically in the viewport. Matches <see cref="Default"/>
    /// today; reserved so the API stays meaningful if platform defaults change.
    /// </summary>
    Center
}

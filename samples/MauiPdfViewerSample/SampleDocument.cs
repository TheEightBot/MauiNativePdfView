namespace MauiPdfViewerSample;

/// <summary>
/// Describes a bundled sample PDF. Add new entries to <see cref="PdfTestPage.Samples"/>
/// to surface more files in the sample picker — no XAML changes required.
/// </summary>
public sealed record SampleDocument(string Name, string FileName, string Description);

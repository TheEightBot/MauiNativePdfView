using MauiNativePdfView.Abstractions;

namespace MauiPdfViewerSample;

public partial class PdfTestPage : ContentPage
{
    private int _backgroundColorIndex = 0;
    private readonly Color[] _backgroundColors = new[]
    {
        Colors.White,
        Colors.LightGray,
        Colors.LightBlue,
        Colors.Beige
    };

    private int _displayModeIndex = 0;
    private readonly PdfDisplayMode[] _displayModes = new[]
    {
        PdfDisplayMode.SinglePageContinuous,
        PdfDisplayMode.SinglePage
    };
    private readonly string[] _displayModeNames = new[]
    {
        "Single Page Continuous",
        "Single Page"
    };

    public PdfTestPage()
    {
        InitializeComponent();
    }

    private void OnLoadFileClicked(object? sender, EventArgs e)
    {
        try
        {
            StatusLabel.Text = "Loading PDF...";

            // Load from raw asset
            PdfViewer.Source = PdfSource.FromAsset("sample.pdf");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error: {ex.Message}";
            DisplayAlert("Error", $"Failed to load PDF: {ex.Message}", "OK");
        }
    }

    private void OnLoadAnnotationFileClicked(object? sender, EventArgs e)
    {
        try
        {
            StatusLabel.Text = "Loading PDF with annotations...";

            // Load PDF with annotations from raw asset
            PdfViewer.Source = PdfSource.FromAsset("sample_with_annotations.pdf");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error: {ex.Message}";
            DisplayAlert("Error", $"Failed to load PDF: {ex.Message}", "OK");
        }
    }

    private void OnPrevPageClicked(object? sender, EventArgs e)
    {
        if (PdfViewer.CurrentPage > 0)
        {
            PdfViewer.GoToPage(PdfViewer.CurrentPage - 1);
        }
    }

    private void OnNextPageClicked(object? sender, EventArgs e)
    {
        if (PdfViewer.CurrentPage < PdfViewer.PageCount - 1)
        {
            PdfViewer.GoToPage(PdfViewer.CurrentPage + 1);
        }
    }

    private void OnDocumentLoaded(object? sender, DocumentLoadedEventArgs e)
    {
        StatusLabel.Text = $"Document loaded successfully!";
        PageInfoLabel.Text = $"Page 1 of {e.PageCount}";
        UpdateButtonStates();
    }

    private void OnPageChanged(object? sender, PageChangedEventArgs e)
    {
        PageInfoLabel.Text = $"Page {e.PageIndex + 1} of {e.PageCount}";
        UpdateButtonStates();
    }

    private void OnError(object? sender, PdfErrorEventArgs e)
    {
        StatusLabel.Text = $"Error: {e.Message}";
        DisplayAlert("PDF Error", e.Message, "OK");
    }

    private void UpdateButtonStates()
    {
        PrevPageButton.IsEnabled = PdfViewer.CurrentPage > 0;
        NextPageButton.IsEnabled = PdfViewer.CurrentPage < PdfViewer.PageCount - 1;
    }

    // New Phase 4 event handlers
    private void OnPdfTapped(object? sender, PdfTappedEventArgs e)
    {
        StatusLabel.Text = $"Tapped on page {e.PageIndex + 1} at ({e.X:F0}, {e.Y:F0})";
    }

    private void OnPdfRendered(object? sender, RenderedEventArgs e)
    {
        StatusLabel.Text += $" (Rendered {e.PageCount} pages)";
    }

    private void OnToggleOrientationClicked(object? sender, EventArgs e)
    {
        if (PdfViewer.ScrollOrientation == PdfScrollOrientation.Vertical)
        {
            PdfViewer.ScrollOrientation = PdfScrollOrientation.Horizontal;
            ToggleOrientationButton.Text = "Toggle (Horizontal)";
            StatusLabel.Text = "Scroll orientation: Horizontal";
        }
        else
        {
            PdfViewer.ScrollOrientation = PdfScrollOrientation.Vertical;
            ToggleOrientationButton.Text = "Toggle (Vertical)";
            StatusLabel.Text = "Scroll orientation: Vertical";
        }
    }

    private void OnToggleBackgroundClicked(object? sender, EventArgs e)
    {
        _backgroundColorIndex = (_backgroundColorIndex + 1) % _backgroundColors.Length;
        PdfViewer.BackgroundColor = _backgroundColors[_backgroundColorIndex];
        StatusLabel.Text = $"Background color: {_backgroundColors[_backgroundColorIndex]}";
    }

    // New Phase 5 event handler
    private void OnToggleDisplayModeClicked(object? sender, EventArgs e)
    {
        _displayModeIndex = (_displayModeIndex + 1) % _displayModes.Length;
        PdfViewer.DisplayMode = _displayModes[_displayModeIndex];
        ToggleDisplayModeButton.Text = _displayModeNames[_displayModeIndex];
        StatusLabel.Text = $"Display mode: {_displayModeNames[_displayModeIndex]}";
    }

    // New Phase 7 event handler
    private void OnToggleAnnotationsClicked(object? sender, EventArgs e)
    {
        PdfViewer.EnableAnnotationRendering = !PdfViewer.EnableAnnotationRendering;
        ToggleAnnotationsButton.Text = PdfViewer.EnableAnnotationRendering ? "Enabled" : "Disabled";
        StatusLabel.Text = $"Annotation rendering: {(PdfViewer.EnableAnnotationRendering ? "Enabled" : "Disabled")}";
    }

    private void OnAnnotationTapped(object? sender, AnnotationTappedEventArgs e)
    {
        var contents = string.IsNullOrEmpty(e.Contents) ? "(no content)" : e.Contents;
        AnnotationInfoLabel.Text = $"Annotation tapped on page {e.PageIndex + 1}: Type={e.AnnotationType}, Contents={contents}";
        StatusLabel.Text = $"Annotation tapped: {e.AnnotationType}";
    }
}

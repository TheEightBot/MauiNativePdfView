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
}

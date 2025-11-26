using MauiNativePdfView.Abstractions;
using Microsoft.Maui.ApplicationModel;

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
        LinkStatusLabel.Text = "Links ready";
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
        LinkStatusLabel.Text = "Link status unavailable";
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

    private void OnToggleTapGesturesClicked(object? sender, EventArgs e)
    {
        PdfViewer.EnableTapGestures = !PdfViewer.EnableTapGestures;
        ToggleTapGesturesButton.Text = PdfViewer.EnableTapGestures ? "Tap events enabled" : "Tap events disabled";
        StatusLabel.Text = PdfViewer.EnableTapGestures
            ? "Tap gestures routed to sample"
            : "Tap gestures forwarded to links";
    }

    private void OnAnnotationTapped(object? sender, AnnotationTappedEventArgs e)
    {
        var contents = string.IsNullOrEmpty(e.Contents) ? "(no content)" : e.Contents;
        AnnotationInfoLabel.Text = $"Annotation tapped on page {e.PageIndex + 1}: Type={e.AnnotationType}, Contents={contents}";
        StatusLabel.Text = $"Annotation tapped: {e.AnnotationType}";
    }

    private async void OnLinkTapped(object? sender, LinkTappedEventArgs e)
    {
        e.Handled = true;

        var linkDescription = !string.IsNullOrEmpty(e.Uri)
            ? e.Uri
            : e.DestinationPage.HasValue
                ? $"Page {e.DestinationPage.Value + 1}"
                : "document link";

        LinkStatusLabel.Text = $"Intercepted: {linkDescription}";
        StatusLabel.Text = "Link tapped – choose an action";

        var actions = new List<string>();
        if (!string.IsNullOrEmpty(e.Uri))
        {
            actions.Add("Open externally");
        }

        if (e.DestinationPage.HasValue)
        {
            actions.Add("Go to destination page");
        }

        var choice = await DisplayActionSheet("Handle link", "Dismiss", null, actions.ToArray());

        switch (choice)
        {
            case "Open externally" when !string.IsNullOrEmpty(e.Uri):
                if (Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri))
                {
                    try
                    {
                        await Launcher.OpenAsync(uri);
                        StatusLabel.Text = "Link opened in browser";
                        LinkStatusLabel.Text = $"Launched: {uri.Host}";
                    }
                    catch (Exception ex)
                    {
                        StatusLabel.Text = "Unable to launch link";
                        await DisplayAlert("Link", $"Failed to open {uri}: {ex.Message}", "OK");
                        LinkStatusLabel.Text = "Launch failed";
                    }
                }
                else
                {
                    StatusLabel.Text = "Invalid link";
                    await DisplayAlert("Link", "The tapped link is not a valid URI.", "OK");
                    LinkStatusLabel.Text = "Invalid URI";
                }
                break;
            case "Go to destination page" when e.DestinationPage.HasValue:
                PdfViewer.GoToPage(e.DestinationPage.Value);
                e.Handled = true;
                StatusLabel.Text = $"Navigated to page {e.DestinationPage.Value + 1}";
                LinkStatusLabel.Text = "Internal link handled";
                break;
            default:
                e.Handled = true;
                StatusLabel.Text = "Link dismissed";
                LinkStatusLabel.Text = "Links idle";
                break;
        }
    }
}

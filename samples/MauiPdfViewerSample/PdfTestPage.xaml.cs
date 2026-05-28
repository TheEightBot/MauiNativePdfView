using MauiNativePdfView.Abstractions;
using Microsoft.Maui.ApplicationModel;

namespace MauiPdfViewerSample;

public partial class PdfTestPage : ContentPage
{
    // ── Sample library ────────────────────────────────────────────────────────
    // To add a new sample, drop a PDF into Resources/Raw/ and append an entry here.
    public IReadOnlyList<SampleDocument> Samples { get; } = new SampleDocument[]
    {
        new("Sample",     "sample.pdf",                  "Multi-page document"),
        new("Annotated",  "sample_with_annotations.pdf", "With annotations"),
        new("Landscape",  "landscape_test.pdf",          "Landscape alignment test"),
    };

    // ── Toggle state ─────────────────────────────────────────────────────────
    private int _backgroundColorIndex = 0;
    private readonly Color[] _backgroundColors =
    {
        Colors.White,
        Colors.LightGray,
        Colors.LightBlue,
        Colors.Beige,
    };

    private int _fitPolicyIndex = 1; // matches FitPolicy="Height" XAML default
    private readonly FitPolicy[] _fitPolicies =
    {
        FitPolicy.Width,
        FitPolicy.Height,
        FitPolicy.Both,
    };
    private readonly string[] _fitPolicyNames = { "Width", "Height", "Both" };

    private int _displayModeIndex = 0;
    private readonly PdfDisplayMode[] _displayModes =
    {
        PdfDisplayMode.SinglePageContinuous,
        PdfDisplayMode.SinglePage,
    };
    private readonly string[] _displayModeNames = { "Cont. Scroll", "Single Page" };

    private int _alignmentIndex = 0;
    private readonly PageAlignment[] _alignments =
    {
        PageAlignment.Default,
        PageAlignment.Top,
        PageAlignment.Center,
    };
    private readonly string[] _alignmentNames = { "Default", "Top", "Center" };

    // ── Init ─────────────────────────────────────────────────────────────────
    public PdfTestPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadSample(Samples[0]);
    }

    // ── Sample loading ────────────────────────────────────────────────────────
    private void LoadSample(SampleDocument sample)
    {
        try
        {
            StatusLabel.Text = $"Loading {sample.Name}…";
            PdfViewer.Source = PdfSource.FromAsset(sample.FileName);
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error: {ex.Message}";
            DisplayAlert("Error", $"Failed to load {sample.Name}: {ex.Message}", "OK");
        }
    }

    private void OnSampleSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is SampleDocument sample)
        {
            LoadSample(sample);
            // Clear so the same card can be tapped again
            SamplesView.SelectedItem = null;
        }
    }

    // ── Navigation ────────────────────────────────────────────────────────────
    private void OnPrevPageClicked(object? sender, EventArgs e)
    {
        if (PdfViewer.CurrentPage > 0)
            PdfViewer.GoToPage(PdfViewer.CurrentPage - 1);
    }

    private void OnNextPageClicked(object? sender, EventArgs e)
    {
        if (PdfViewer.CurrentPage < PdfViewer.PageCount - 1)
            PdfViewer.GoToPage(PdfViewer.CurrentPage + 1);
    }

    private void UpdateButtonStates()
    {
        PrevPageButton.IsEnabled = PdfViewer.CurrentPage > 0;
        NextPageButton.IsEnabled = PdfViewer.CurrentPage < PdfViewer.PageCount - 1;
    }

    // ── PDF events ────────────────────────────────────────────────────────────
    private void OnDocumentLoaded(object? sender, DocumentLoadedEventArgs e)
    {
        StatusLabel.Text = "Document loaded";
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

    private void OnPdfTapped(object? sender, PdfTappedEventArgs e)
    {
        StatusLabel.Text = $"Tapped page {e.PageIndex + 1} at ({e.X:F0}, {e.Y:F0})";
    }

    private void OnPdfRendered(object? sender, RenderedEventArgs e)
    {
        StatusLabel.Text += $" — {e.PageCount} pages rendered";
    }

    private void OnAnnotationTapped(object? sender, AnnotationTappedEventArgs e)
    {
        var contents = string.IsNullOrEmpty(e.Contents) ? "(no content)" : e.Contents;
        AnnotationInfoLabel.Text = $"Page {e.PageIndex + 1}: {e.AnnotationType} — {contents}";
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

        StatusLabel.Text = $"Link intercepted: {linkDescription}";

        var actions = new List<string>();
        if (!string.IsNullOrEmpty(e.Uri))
            actions.Add("Open externally");
        if (e.DestinationPage.HasValue)
            actions.Add("Go to destination page");

        var choice = await DisplayActionSheet("Handle link", "Dismiss", null, actions.ToArray());

        switch (choice)
        {
            case "Open externally" when !string.IsNullOrEmpty(e.Uri):
                if (Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri))
                {
                    try
                    {
                        await Launcher.OpenAsync(uri);
                        StatusLabel.Text = $"Launched: {uri.Host}";
                    }
                    catch (Exception ex)
                    {
                        StatusLabel.Text = "Unable to launch link";
                        await DisplayAlert("Link", $"Failed to open {uri}: {ex.Message}", "OK");
                    }
                }
                else
                {
                    StatusLabel.Text = "Invalid link URI";
                    await DisplayAlert("Link", "The tapped link is not a valid URI.", "OK");
                }
                break;

            case "Go to destination page" when e.DestinationPage.HasValue:
                PdfViewer.GoToPage(e.DestinationPage.Value);
                e.Handled = true;
                StatusLabel.Text = $"Navigated to page {e.DestinationPage.Value + 1}";
                break;

            default:
                e.Handled = true;
                StatusLabel.Text = "Link dismissed";
                break;
        }
    }

    // ── Settings overlay ──────────────────────────────────────────────────────
    private void OnToggleSettingsClicked(object? sender, EventArgs e) =>
        SettingsOverlay.IsVisible = true;

    private void OnSettingsBackdropTapped(object? sender, TappedEventArgs e) =>
        SettingsOverlay.IsVisible = false;

    private void OnSettingsCloseClicked(object? sender, EventArgs e) =>
        SettingsOverlay.IsVisible = false;

    // ── Control toggles ───────────────────────────────────────────────────────
    private void OnToggleAlignmentClicked(object? sender, EventArgs e)
    {
        _alignmentIndex = (_alignmentIndex + 1) % _alignments.Length;
        PdfViewer.PageAlignment = _alignments[_alignmentIndex];
        ToggleAlignmentButton.Text = _alignmentNames[_alignmentIndex];
        StatusLabel.Text = $"Alignment: {_alignmentNames[_alignmentIndex]}";
    }

    private void OnToggleOrientationClicked(object? sender, EventArgs e)
    {
        if (PdfViewer.ScrollOrientation == PdfScrollOrientation.Vertical)
        {
            PdfViewer.ScrollOrientation = PdfScrollOrientation.Horizontal;
            ToggleOrientationButton.Text = "Horizontal";
            StatusLabel.Text = "Scroll orientation: Horizontal";
        }
        else
        {
            PdfViewer.ScrollOrientation = PdfScrollOrientation.Vertical;
            ToggleOrientationButton.Text = "Vertical";
            StatusLabel.Text = "Scroll orientation: Vertical";
        }
    }

    private void OnToggleFitPolicyClicked(object? sender, EventArgs e)
    {
        _fitPolicyIndex = (_fitPolicyIndex + 1) % _fitPolicies.Length;
        PdfViewer.FitPolicy = _fitPolicies[_fitPolicyIndex];
        ToggleFitPolicyButton.Text = _fitPolicyNames[_fitPolicyIndex];
        StatusLabel.Text = $"Fit policy: {_fitPolicyNames[_fitPolicyIndex]}";
    }

    private void OnToggleDisplayModeClicked(object? sender, EventArgs e)
    {
        _displayModeIndex = (_displayModeIndex + 1) % _displayModes.Length;
        PdfViewer.DisplayMode = _displayModes[_displayModeIndex];
        ToggleDisplayModeButton.Text = _displayModeNames[_displayModeIndex];
        StatusLabel.Text = $"Display mode: {_displayModeNames[_displayModeIndex]}";
    }

    private void OnToggleBackgroundClicked(object? sender, EventArgs e)
    {
        _backgroundColorIndex = (_backgroundColorIndex + 1) % _backgroundColors.Length;
        PdfViewer.BackgroundColor = _backgroundColors[_backgroundColorIndex];
        StatusLabel.Text = $"Background: {_backgroundColors[_backgroundColorIndex]}";
    }

    private void OnToggleAnnotationsClicked(object? sender, EventArgs e)
    {
        PdfViewer.EnableAnnotationRendering = !PdfViewer.EnableAnnotationRendering;
        ToggleAnnotationsButton.Text = PdfViewer.EnableAnnotationRendering ? "Enabled" : "Disabled";
        StatusLabel.Text = $"Annotations: {(PdfViewer.EnableAnnotationRendering ? "Enabled" : "Disabled")}";
    }

    private void OnToggleTapGesturesClicked(object? sender, EventArgs e)
    {
        PdfViewer.EnableTapGestures = !PdfViewer.EnableTapGestures;
        ToggleTapGesturesButton.Text = PdfViewer.EnableTapGestures ? "Events On" : "Events Off";
        StatusLabel.Text = PdfViewer.EnableTapGestures
            ? "Tap gestures routed to sample"
            : "Tap gestures forwarded to links";
    }
}

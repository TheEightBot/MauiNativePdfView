using MauiNativePdfView;
using MauiNativePdfView.Abstractions;

namespace MauiPdfViewerSample;

/// <summary>
/// Dedicated repro harness for layering / composition issues.
///
/// Primary target: GitHub issue #8 — content placed in the same Grid region as a
/// <c>PdfView</c> ("row 0") is invisible unless <c>BackgroundColor</c> is set, and
/// overlapping content flickers while the PDF scrolls.
///
/// This page is intentionally self-contained and toggle-driven so new layering
/// scenarios can be added over time without disturbing the main viewer page.
/// </summary>
public partial class LayoutReproPage : ContentPage
{
    private enum BackgroundMode { Unset, Transparent, Opaque }

    private BackgroundMode _backgroundMode = BackgroundMode.Unset;
    private bool _overlayLayout = true;   // true = Overlay (faithful #8 repro), false = Stacked (control case)
    private bool _headerOnTop;            // true = header ZIndex bumped (flicker workaround)

    public LayoutReproPage()
    {
        InitializeComponent();

        // Open in the exact failing state from issue #8: overlay layout, background unset.
        ApplyLayout();
        ApplyBackground();
        ApplyZIndex();

        PdfViewer.Source = PdfSource.FromAsset("sample.pdf");
    }

    // ── Layout: Overlay vs Stacked ────────────────────────────────────────────
    private void OnToggleLayout(object? sender, EventArgs e)
    {
        _overlayLayout = !_overlayLayout;
        ApplyLayout();
    }

    private void ApplyLayout()
    {
        if (_overlayLayout)
        {
            // Header and PdfView share a single cell — the faithful issue #8 repro.
            ReproHost.RowDefinitions = new RowDefinitionCollection(
                new RowDefinition { Height = GridLength.Star });

            Grid.SetRow(PdfViewer, 0);
            Grid.SetRow(HeaderBand, 0);
            HeaderBand.VerticalOptions = LayoutOptions.Start;
            LayoutButton.Text = "Layout: Overlay";
        }
        else
        {
            // Header in its own row above the PdfView — the control case that
            // should always render correctly regardless of background.
            ReproHost.RowDefinitions = new RowDefinitionCollection(
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star });

            Grid.SetRow(HeaderBand, 0);
            HeaderBand.VerticalOptions = LayoutOptions.Fill;
            Grid.SetRow(PdfViewer, 1);
            LayoutButton.Text = "Layout: Stacked";
        }

        UpdateStateLabel();
    }

    // ── Background: (unset) → Transparent → Opaque ─────────────────────────────
    private void OnCycleBackground(object? sender, EventArgs e)
    {
        _backgroundMode = _backgroundMode switch
        {
            BackgroundMode.Unset => BackgroundMode.Transparent,
            BackgroundMode.Transparent => BackgroundMode.Opaque,
            _ => BackgroundMode.Unset,
        };
        ApplyBackground();
    }

    private void ApplyBackground()
    {
        switch (_backgroundMode)
        {
            case BackgroundMode.Unset:
                // Closest thing to "never set in XAML": reset to the property default (null).
                PdfViewer.ClearValue(VisualElement.BackgroundColorProperty);
                BgButton.Text = "BG: (unset)";
                break;
            case BackgroundMode.Transparent:
                PdfViewer.BackgroundColor = Colors.Transparent;
                BgButton.Text = "BG: Transparent";
                break;
            case BackgroundMode.Opaque:
                PdfViewer.BackgroundColor = Colors.White;
                BgButton.Text = "BG: White";
                break;
        }

        UpdateStateLabel();
    }

    // ── Header ZIndex (flicker workaround toggle) ──────────────────────────────
    private void OnToggleZIndex(object? sender, EventArgs e)
    {
        _headerOnTop = !_headerOnTop;
        ApplyZIndex();
    }

    private void ApplyZIndex()
    {
        HeaderBand.ZIndex = _headerOnTop ? 10 : 0;
        ZIndexButton.Text = _headerOnTop ? "Header Z: 10" : "Header Z: 0";
        UpdateStateLabel();
    }

    // ── Status readout ─────────────────────────────────────────────────────────
    private void UpdateStateLabel()
    {
        var layout = _overlayLayout ? "Overlay" : "Stacked";
        var bg = _backgroundMode switch
        {
            BackgroundMode.Unset => "unset",
            BackgroundMode.Transparent => "Transparent",
            _ => "White (opaque)",
        };
        StateLabel.Text = $"Layout={layout}  •  Background={bg}  •  Header ZIndex={HeaderBand.ZIndex}";
    }

    // ── PdfView events ─────────────────────────────────────────────────────────
    private void OnDocumentLoaded(object? sender, DocumentLoadedEventArgs e)
    {
        HeaderSubLabel.Text = $"{e.PageCount} pages loaded — scroll to test flicker.";
    }

    private async void OnError(object? sender, PdfErrorEventArgs e)
    {
        await DisplayAlert("PDF Error", e.Message, "OK");
    }
}

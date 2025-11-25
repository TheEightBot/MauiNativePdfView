<div align="center">

# MauiNativePdfView

<img src="images/icon.svg" width="200" />

### A high-performance, cross-platform PDF viewer for .NET MAUI

[![NuGet](https://img.shields.io/nuget/v/MauiNativePdfView.svg)](https://www.nuget.org/packages/MauiNativePdfView/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MauiNativePdfView.svg)](https://www.nuget.org/packages/MauiNativePdfView/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/download)
[![MAUI](https://img.shields.io/badge/MAUI-Latest-green.svg)](https://github.com/dotnet/maui)

**Native PDF rendering** • **Zero web dependencies** • **Full feature parity**

[Features](#-features) • [Installation](#-installation) • [Quick Start](#-quick-start) • [Documentation](#-documentation) • [Examples](#-examples)

</div>

---

## 🎯 Overview

MauiNativePdfView brings native PDF viewing capabilities to your .NET MAUI applications by wrapping platform-native controls. Unlike WebView-based solutions, this library provides true native performance with full access to platform-specific PDF features.

### Why MauiNativePdfView?

- **🚀 Native Performance** - Uses PDFKit (iOS) and AhmerPdfium (Android) for optimal speed
- **💪 Feature Complete** - Comprehensive API with zoom, navigation, annotations, and events
- **🎨 Consistent API** - Write once, works on both iOS and Android
- **📦 Zero Web Dependencies** - No WebView, no JavaScript, pure native rendering
- **🔧 Highly Configurable** - Extensive properties for customization
- **📱 Production Ready** - Battle-tested with full annotation support

## ✨ Features

### Core Functionality

- ✅ **Multiple PDF Sources** - Load from files, URLs, streams, byte arrays, or embedded assets
- ✅ **Password Protection** - Full support for encrypted PDFs
- ✅ **Zoom & Gestures** - Pinch-to-zoom, double-tap zoom, with configurable min/max levels
- ✅ **Page Navigation** - Swipe between pages, programmatic navigation, page events
- ✅ **Link Handling** - Automatic detection and handling of internal/external links
- ✅ **Display Modes** - Single page or continuous scrolling
- ✅ **Scroll Orientation** - Vertical or horizontal page layout

### Advanced Features

- ✅ **Annotation Rendering** - Toggle PDF annotations on/off
- ✅ **Annotation Events** - Tap detection with annotation details (iOS)
- ✅ **Quality Control** - Antialiasing and rendering quality settings
- ✅ **Background Color** - Customizable viewer background
- ✅ **Page Spacing** - Adjustable spacing between pages
- ✅ **Event System** - Comprehensive events for document lifecycle

### Events

- `DocumentLoaded` - Fires when PDF is loaded with page count and metadata
- `PageChanged` - Current page and total page count updates
- `LinkTapped` - User taps on a link with URI and destination
- `Tapped` - General tap events with page coordinates
- `AnnotationTapped` - Annotation tap with type, content, and bounds (iOS)
- `Rendered` - Initial rendering complete
- `Error` - Error handling with detailed messages

## 📦 Installation

### NuGet Package Manager

```bash
dotnet add package MauiNativePdfView
```

### Package Manager Console

```powershell
Install-Package MauiNativePdfView
```

### Requirements

- **.NET 9.0** or later
- **iOS 12.2+** (PDFKit)
- **Android 7.0+** (API 24+)

## 🚀 Quick Start

### 1. Add Namespace

```xml
xmlns:pdf="clr-namespace:MauiNativePdfView;assembly=MauiNativePdfView"
```

### 2. Basic XAML

```xml
<pdf:PdfView x:Name="pdfViewer"
             EnableZoom="True"
             EnableSwipe="True"
             DocumentLoaded="OnDocumentLoaded"
             PageChanged="OnPageChanged" />
```

### 3. Load a PDF

```csharp
// From file
pdfViewer.Source = PdfSource.FromFile("/path/to/document.pdf");

// From URL
pdfViewer.Source = PdfSource.FromUri(new Uri("https://example.com/doc.pdf"));

// From embedded asset
pdfViewer.Source = PdfSource.FromAsset("sample.pdf");

// From stream
pdfViewer.Source = PdfSource.FromStream(myStream);

// From byte array
pdfViewer.Source = PdfSource.FromBytes(pdfBytes);

// With password
pdfViewer.Source = PdfSource.FromFile("/path/to/encrypted.pdf", "password");
```

### 4. Handle Events

```csharp
private void OnDocumentLoaded(object sender, DocumentLoadedEventArgs e)
{
    Console.WriteLine($"Loaded: {e.PageCount} pages");
    Console.WriteLine($"Title: {e.Title}");
    Console.WriteLine($"Author: {e.Author}");
}

private void OnPageChanged(object sender, PageChangedEventArgs e)
{
    Console.WriteLine($"Page {e.PageIndex + 1} of {e.PageCount}");
}
```

## 📖 Documentation

### PdfView Properties

| Property                    | Type                   | Default                | Description                    |
| --------------------------- | ---------------------- | ---------------------- | ------------------------------ |
| `Source`                    | `PdfSource`            | `null`                 | PDF source to display          |
| `EnableZoom`                | `bool`                 | `true`                 | Enable pinch-to-zoom           |
| `EnableSwipe`               | `bool`                 | `true`                 | Enable swipe gestures          |
| `EnableLinkNavigation`      | `bool`                 | `true`                 | Enable clickable links         |
| `Zoom`                      | `float`                | `1.0f`                 | Current zoom level             |
| `MinZoom`                   | `float`                | `1.0f`                 | Minimum zoom level             |
| `MaxZoom`                   | `float`                | `3.0f`                 | Maximum zoom level             |
| `PageSpacing`               | `int`                  | `10`                   | Spacing between pages (pixels) |
| `FitPolicy`                 | `FitPolicy`            | `Width`                | How pages fit on screen        |
| `DisplayMode`               | `PdfDisplayMode`       | `SinglePageContinuous` | Page display mode              |
| `ScrollOrientation`         | `PdfScrollOrientation` | `Vertical`             | Scroll direction               |
| `DefaultPage`               | `int`                  | `0`                    | Initial page (0-based)         |
| `EnableAntialiasing`        | `bool`                 | `true`                 | Antialiasing (Android only)    |
| `UseBestQuality`            | `bool`                 | `true`                 | Best quality rendering         |
| `BackgroundColor`           | `Color`                | `null`                 | Viewer background color        |
| `EnableAnnotationRendering` | `bool`                 | `true`                 | Show PDF annotations           |
| `CurrentPage`               | `int`                  | `0`                    | Current page (readonly)        |
| `PageCount`                 | `int`                  | `0`                    | Total pages (readonly)         |

### PdfSource Types

```csharp
// File path
var source = PdfSource.FromFile(string filePath, string? password = null);

// URI/URL
var source = PdfSource.FromUri(Uri uri, string? password = null);

// Stream
var source = PdfSource.FromStream(Stream stream, string? password = null);

// Byte array
var source = PdfSource.FromBytes(byte[] data, string? password = null);

// Asset/Resource
var source = PdfSource.FromAsset(string assetName, string? password = null);
```

### Enums

```csharp
// How pages fit on screen
public enum FitPolicy
{
    Width,    // Fit to width
    Height,   // Fit to height
    Both      // Fit both dimensions
}

// Page display mode
public enum PdfDisplayMode
{
    SinglePage,           // One page at a time
    SinglePageContinuous  // Continuous scrolling
}

// Scroll direction
public enum PdfScrollOrientation
{
    Vertical,    // Scroll up/down
    Horizontal   // Scroll left/right
}
```

### Methods

```csharp
// Navigate to specific page (0-based)
pdfViewer.GoToPage(int pageIndex);

// Reload current document
pdfViewer.Reload();
```

## 💡 Examples

### Complete PDF Viewer

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:pdf="clr-namespace:MauiNativePdfView;assembly=MauiNativePdfView"
             x:Class="MyApp.PdfPage">

    <Grid RowDefinitions="Auto,*,Auto">

        <!-- Toolbar -->
        <HorizontalStackLayout Grid.Row="0" Padding="10" Spacing="10">
            <Button Text="◀" Clicked="OnPreviousPage" />
            <Button Text="▶" Clicked="OnNextPage" />
            <Button Text="Zoom In" Clicked="OnZoomIn" />
            <Button Text="Zoom Out" Clicked="OnZoomOut" />
        </HorizontalStackLayout>

        <!-- PDF Viewer -->
        <pdf:PdfView x:Name="pdfViewer"
                     Grid.Row="1"
                     EnableZoom="True"
                     EnableSwipe="True"
                     EnableLinkNavigation="True"
                     EnableAnnotationRendering="True"
                     PageSpacing="10"
                     BackgroundColor="#F5F5F5"
                     DocumentLoaded="OnDocumentLoaded"
                     PageChanged="OnPageChanged"
                     LinkTapped="OnLinkTapped"
                     Error="OnError" />

        <!-- Status Bar -->
        <Label x:Name="statusLabel"
               Grid.Row="2"
               Padding="10"
               HorizontalOptions="Center" />
    </Grid>

</ContentPage>
```

```csharp
public partial class PdfPage : ContentPage
{
    public PdfPage()
    {
        InitializeComponent();
        pdfViewer.Source = PdfSource.FromAsset("sample.pdf");
    }

    private void OnDocumentLoaded(object sender, DocumentLoadedEventArgs e)
    {
        statusLabel.Text = $"Loaded: {e.PageCount} pages - {e.Title}";
    }

    private void OnPageChanged(object sender, PageChangedEventArgs e)
    {
        statusLabel.Text = $"Page {e.PageIndex + 1} of {e.PageCount}";
    }

    private void OnLinkTapped(object sender, LinkTappedEventArgs e)
    {
        if (e.Uri != null)
        {
            // Handle external link
            Launcher.OpenAsync(e.Uri);
            e.Handled = true;
        }
        else if (e.DestinationPage.HasValue)
        {
            // Internal link - handled automatically
        }
    }

    private void OnError(object sender, PdfErrorEventArgs e)
    {
        DisplayAlert("Error", e.Message, "OK");
    }

    private void OnPreviousPage(object sender, EventArgs e)
    {
        if (pdfViewer.CurrentPage > 0)
            pdfViewer.GoToPage(pdfViewer.CurrentPage - 1);
    }

    private void OnNextPage(object sender, EventArgs e)
    {
        if (pdfViewer.CurrentPage < pdfViewer.PageCount - 1)
            pdfViewer.GoToPage(pdfViewer.CurrentPage + 1);
    }

    private void OnZoomIn(object sender, EventArgs e)
    {
        pdfViewer.Zoom = Math.Min(pdfViewer.Zoom + 0.5f, pdfViewer.MaxZoom);
    }

    private void OnZoomOut(object sender, EventArgs e)
    {
        pdfViewer.Zoom = Math.Max(pdfViewer.Zoom - 0.5f, pdfViewer.MinZoom);
    }
}
```

### MVVM Pattern

```csharp
public class PdfViewModel : INotifyPropertyChanged
{
    private PdfSource _pdfSource;
    private int _currentPage;
    private int _pageCount;

    public PdfSource PdfSource
    {
        get => _pdfSource;
        set => SetProperty(ref _pdfSource, value);
    }

    public int CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public int PageCount
    {
        get => _pageCount;
        set => SetProperty(ref _pageCount, value);
    }

    public Command LoadPdfCommand { get; }
    public Command<int> GoToPageCommand { get; }

    public PdfViewModel()
    {
        LoadPdfCommand = new Command(LoadPdf);
        GoToPageCommand = new Command<int>(GoToPage);
    }

    private void LoadPdf()
    {
        PdfSource = PdfSource.FromAsset("document.pdf");
    }

    private void GoToPage(int pageIndex)
    {
        // Page navigation handled by binding
    }
}
```

```xml
<pdf:PdfView Source="{Binding PdfSource}"
             CurrentPage="{Binding CurrentPage}"
             PageCount="{Binding PageCount}" />
```

### Password-Protected PDFs

```csharp
try
{
    pdfViewer.Source = PdfSource.FromFile("encrypted.pdf", "mypassword");
}
catch (Exception ex)
{
    // Handle incorrect password
    await DisplayAlert("Error", "Invalid password", "OK");
}
```

### Annotation Handling (iOS)

```csharp
private void OnAnnotationTapped(object sender, AnnotationTappedEventArgs e)
{
    Console.WriteLine($"Annotation on page {e.PageIndex + 1}");
    Console.WriteLine($"Type: {e.AnnotationType}");
    Console.WriteLine($"Contents: {e.Contents}");
    Console.WriteLine($"Bounds: {e.Bounds}");

    // Prevent default behavior
    e.Handled = true;
}
```

## 🏗️ Architecture

```
┌─────────────────────────────────────┐
│     .NET MAUI Application           │
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│      MauiNativePdfView              │
│  ┌────────────────────────────────┐ │
│  │   PdfView (MAUI Control)       │ │
│  │   - Bindable Properties        │ │
│  │   - Event Handlers             │ │
│  │   - Platform Handlers          │ │
│  └────────────────────────────────┘ │
└─────────────┬───────────────────────┘
              │
      ┌───────┴────────┐
      │                │
┌─────▼──────┐   ┌────▼───────┐
│  Android   │   │    iOS     │
│  Handler   │   │  Handler   │
└─────┬──────┘   └────┬───────┘
      │               │
┌─────▼──────┐   ┌────▼───────┐
│AhmerPdfium │   │  PDFKit    │
│ (Native)   │   │ (Native)   │
└────────────┘   └────────────┘
```

## 🔧 Platform Details

### iOS (PDFKit)

- **Framework**: Apple's native PDFKit
- **Version**: iOS 12.2+
- **Features**: Full annotation support, smooth rendering
- **Size**: 0 KB (system framework)

### Android (AhmerPdfium)

- **Library**: [AhmerPdfium](https://github.com/AhmerAfzal1/AhmerPdfium) by Ahmer Afzal
- **Base**: Enhanced fork of [AndroidPdfViewer](https://github.com/barteksc/AndroidPdfViewer)
- **Version**: 2.0.1 (viewer) + 1.9.2 (pdfium)
- **Features**: 16KB page size support, reliable rendering
- **Size**: ~16MB (native libraries for all architectures)
- **Note**: Annotation tap events not supported by library

## 📊 Performance

- **Memory Efficient**: Native rendering engines handle memory management
- **Fast Loading**: Platform-optimized PDF parsing
- **Smooth Scrolling**: Hardware-accelerated rendering
- **Large Files**: Tested with 100+ page documents

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

### Building from Source

```bash
git clone https://github.com/yourusername/MauiNativePdfView.git
cd MauiNativePdfView
dotnet restore
dotnet build
```

### Running Tests

```bash
cd samples/MauiPdfViewerSample
dotnet build
```

## 📝 Changelog

See [FEATURE_ENHANCEMENT_PLAN.md](FEATURE_ENHANCEMENT_PLAN.md) for detailed implementation history and feature roadmap.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### Third-Party Licenses

- **AhmerPdfium**: Apache License 2.0
- **PDFKit**: Apple System Framework

## 🙏 Acknowledgments

- **[Ahmer Afzal](https://github.com/AhmerAfzal1)** - AhmerPdfium library maintainer
- **[barteksc](https://github.com/barteksc)** - Original AndroidPdfViewer
- **Apple** - PDFKit framework
- **.NET MAUI Team** - Excellent framework

## 💬 Support

- 📖 [Documentation](https://github.com/yourusername/MauiNativePdfView/wiki)
- 🐛 [Issue Tracker](https://github.com/yourusername/MauiNativePdfView/issues)
- 💬 [Discussions](https://github.com/yourusername/MauiNativePdfView/discussions)

## ⭐ Show Your Support

If you find this library helpful, please give it a star! ⭐

---

<div align="center">

**Made with ❤️ for the .NET MAUI community**

[⬆ Back to Top](#-mauinativepdfview)

</div>

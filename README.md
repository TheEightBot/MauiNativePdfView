<div align="center">

# MauiNativePdfView

<img src="images/icon.svg" width="200" />

### A high-performance, cross-platform PDF viewer for .NET MAUI

[![NuGet](https://img.shields.io/nuget/v/Eightbot.MauiNativePdfView.svg)](https://www.nuget.org/packages/MauiNativePdfView/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Eightbot.MauiNativePdfView.svg)](https://www.nuget.org/packages/MauiNativePdfView/)

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
- ✅ **Link Interception** - Intercept and handle link taps before navigation (both platforms)
- ✅ **Link Handling** - Automatic detection and handling of internal/external links
- ✅ **Display Modes** - Single page or continuous scrolling
- ✅ **Scroll Orientation** - Vertical or horizontal page layout

### Advanced Features

- ✅ **Annotation Rendering** - Toggle PDF annotations on/off
- ✅ **Annotation Events** - Tap detection with annotation details (iOS)
- ✅ **Tap Gesture Control** - Enable/disable custom tap interception
- ✅ **Quality Control** - Antialiasing and rendering quality settings
- ✅ **Background Color** - Customizable viewer background
- ✅ **Page Spacing** - Adjustable spacing between pages
- ✅ **Event System** - Comprehensive events for document lifecycle

### Events

- `DocumentLoaded` - Fires when PDF is loaded with page count and metadata
- `PageChanged` - Current page and total page count updates
- `LinkTapped` - Intercept link taps before navigation (set `e.Handled = true` to prevent)
- `Tapped` - General tap events with page coordinates (requires `EnableTapGestures = true`)
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
| `EnableTapGestures`         | `bool`                 | `false`                | Enable tap interception        |
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
            // Intercept and handle external link yourself
            DisplayAlert("Link Tapped", $"Opening: {e.Uri}", "OK");
            Launcher.OpenAsync(e.Uri);
            
            // Prevent default navigation
            e.Handled = true;
        }
        else if (e.DestinationPage.HasValue)
        {
            // Internal link - allow default navigation
            // Or set e.Handled = true to prevent it
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

### Link Interception

Both iOS and Android support intercepting link taps before navigation occurs. This allows you to handle links yourself or prevent navigation entirely.

```csharp
pdfViewer.LinkTapped += (sender, e) =>
{
    Console.WriteLine($"Link tapped: {e.Uri}");
    
    if (e.Uri?.Contains("example.com") == true)
    {
        // Custom handling for specific domain
        DisplayAlert("Info", "This link is not allowed", "OK");
        e.Handled = true; // Prevent navigation
    }
    else if (e.Uri != null)
    {
        // Log analytics before opening
        Analytics.TrackEvent("PDF_Link_Clicked", new Dictionary<string, string>
        {
            { "Uri", e.Uri }
        });
        
        // Allow default navigation (or handle manually)
        e.Handled = false;
    }
};
```

**Platform Implementation:**
- **iOS**: Uses `PdfViewDelegate.WillClickOnLink` to intercept before navigation
- **Android**: Uses `LinkHandler.HandleLinkEvent` to intercept before navigation

### Tap Gesture Handling

Enable custom tap detection with page coordinates:

```csharp
pdfViewer.EnableTapGestures = true;

pdfViewer.Tapped += (sender, e) =>
{
    Console.WriteLine($"Tapped page {e.PageIndex} at ({e.X}, {e.Y})");
    
    // Add your custom tap handling logic
    // For example: show a custom menu, add annotations, etc.
};
```

**Note**: When `EnableTapGestures = false` (default), the PDF viewer uses native platform tap handling which is optimized for link detection.

### Annotation Handling (iOS)

```csharp
private void OnAnnotationTapped(object sender, AnnotationTappedEventArgs e)
{
    Console.WriteLine($"Annotation on page {e.PageIndex + 1}");
    Console.WriteLine($"Type: {e.AnnotationType}");
    Console.WriteLine($"Contents: {e.Contents}");
    Console.WriteLine($"Bounds: {e.Bounds}");

    // Prevent default behavior if needed
    e.Handled = true;
}
```

**Note**: Annotation tap detection is only supported on iOS with PDFKit. Android's AhmerPdfium library does not expose annotation tap events.

## 🎯 Common Scenarios

### Restrict External Navigation

```csharp
pdfViewer.LinkTapped += (sender, e) =>
{
    if (e.Uri != null && e.Uri.StartsWith("http"))
    {
        DisplayAlert("Restricted", "External links are not allowed", "OK");
        e.Handled = true; // Block navigation
    }
};
```

### Track Link Clicks for Analytics

```csharp
pdfViewer.LinkTapped += (sender, e) =>
{
    // Log the link click
    Analytics.TrackEvent("PDF_Link_Clicked", new Dictionary<string, string>
    {
        { "Document", pdfViewer.Source?.ToString() ?? "Unknown" },
        { "Link", e.Uri ?? $"Page {e.DestinationPage}" },
        { "CurrentPage", pdfViewer.CurrentPage.ToString() }
    });
    
    // Allow normal navigation
    e.Handled = false;
};
```

### Custom Link Handling with Confirmation

```csharp
pdfViewer.LinkTapped += async (sender, e) =>
{
    if (e.Uri != null)
    {
        var result = await DisplayAlert(
            "Open Link?", 
            $"Do you want to open {e.Uri}?", 
            "Yes", 
            "No"
        );
        
        if (result)
        {
            await Launcher.OpenAsync(e.Uri);
        }
        
        e.Handled = true; // Prevent default navigation
    }
};
```

### Deep Link Handling

```csharp
pdfViewer.LinkTapped += async (sender, e) =>
{
    if (e.Uri?.StartsWith("myapp://") == true)
    {
        // Handle custom URL scheme
        await Shell.Current.GoToAsync(e.Uri.Replace("myapp://", ""));
        e.Handled = true;
    }
};
```

### Disable All Link Navigation

```csharp
// Simple approach
pdfViewer.EnableLinkNavigation = false;

// Or intercept all links
pdfViewer.LinkTapped += (sender, e) =>
{
    e.Handled = true; // Block all navigation
};
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
- **Features**: Full annotation support, smooth rendering, link interception via `PdfViewDelegate`
- **Link Handling**: Native `WillClickOnLink` delegate method
- **Size**: 0 KB (system framework)

### Android (AhmerPdfium)

- **Library**: [AhmerPdfium](https://github.com/AhmerAfzal1/AhmerPdfium) by Ahmer Afzal
- **Base**: Enhanced fork of [AndroidPdfViewer](https://github.com/barteksc/AndroidPdfViewer)
- **Version**: 2.0.1 (viewer) + 1.9.2 (pdfium)
- **Features**: 16KB page size support, reliable rendering, link interception via `LinkHandler`
- **Link Handling**: Custom `ILinkHandler` implementation
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
git clone https://github.com/TheEightBot/MauiNativePdfView.git
cd MauiNativePdfView
dotnet restore
dotnet build
```

### Running Tests

```bash
cd samples/MauiPdfViewerSample
dotnet build
```

## ❓ Troubleshooting

### Links Not Working on iOS

If links are not responding on iOS, ensure:
1. `EnableLinkNavigation = true` (default)
2. The PDF actually contains link annotations
3. You're not setting `e.Handled = true` for all links in the `LinkTapped` event

### Tapped Event Not Firing

The `Tapped` event requires:
```csharp
pdfViewer.EnableTapGestures = true;
```

**Note**: When `EnableTapGestures = true`, it may interfere with native link handling on some platforms. For link detection only, keep it `false` (default) and use the `LinkTapped` event.

### LinkTapped Event Handler Not Called

Ensure you're subscribing to the event:
```csharp
pdfViewer.LinkTapped += OnLinkTapped;
```

Or in XAML:
```xml
<pdf:PdfView LinkTapped="OnLinkTapped" />
```

### Android Annotation Events

Annotation tap events (`AnnotationTapped`) are **only supported on iOS**. The Android AhmerPdfium library does not expose annotation-level tap detection. Use the `Tapped` event as an alternative for Android.


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

- 🐛 [Issue Tracker](https://github.com/TheEightBot/MauiNativePdfView/issues)

## ⭐ Show Your Support

If you find this library helpful, please give it a star! ⭐

---

<div align="center">

**Made with ❤️ for the .NET MAUI community**

[⬆ Back to Top](#-mauinativepdfview)

</div>

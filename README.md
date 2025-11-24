# MauiNativePdfView

A cross-platform .NET MAUI PDF viewer library that wraps native PDF controls for iOS and Android.

## 🎯 Project Status

**Current Phase**: Phase 2 - Android Implementation  
**Version**: Pre-release (v1.0.0-alpha1)  
**Phase 1**: ✅ Complete ([PHASE1_COMPLETE.md](PHASE1_COMPLETE.md))

This project is currently under active development. See [WORKPLAN.md](WORKPLAN.md) for detailed implementation plan.

## 📋 Overview

MauiNativePdfView provides a unified API for displaying PDF documents in .NET MAUI applications using native platform controls:

- **iOS**: PDFKit framework (system native)
- **Android**: AhmerPdfium library (maintained fork with 16KB page size support)

## ✨ Planned Features

- ✅ Load PDFs from multiple sources (file, URL, stream, bytes, assets)
- ✅ Pinch-to-zoom functionality
- ✅ Double-tap zoom
- ✅ Clickable links (internal and external)
- ✅ Page navigation
- ✅ Consistent display across platforms
- ✅ Memory-efficient rendering
- ✅ Event-driven architecture

## 🏗️ Architecture

```
┌─────────────────────────────────────┐
│     .NET MAUI Application           │
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│      MauiNativePdfView (API)        │
│     - PdfView Control               │
│     - PdfSource                     │
│     - Events & Properties           │
└─────────────┬───────────────────────┘
              │
      ┌───────┴───────┐
      │               │
┌─────▼─────┐   ┌────▼──────┐
│  Android  │   │    iOS    │
│  Wrapper  │   │  Wrapper  │
└─────┬─────┘   └────┬──────┘
      │              │
┌─────▼─────┐   ┌────▼──────┐
│Android PDF│   │  PDFKit   │
│  Viewer   │   │(System)   │
└───────────┘   └───────────┘
```

## 📦 Project Structure

```
MauiNativePdfView/
├── src/
│   ├── MauiNativePdfView/                    # Main MAUI library
│   ├── MauiNativePdfView.Android.Binding/    # Android binding
│   └── MauiNativePdfView.iOS.Binding/        # iOS binding (if needed)
├── samples/
│   └── MauiPdfViewerSample/                  # Sample app
├── docs/                                      # Documentation
├── WORKPLAN.md                                # Detailed implementation plan
├── CHECKLIST.md                               # Quick progress tracker
└── README.md                                  # This file
```

## 🚀 Quick Start (Planned)

Once released, installation will be as simple as:

```bash
dotnet add package MauiNativePdfView
```

### Basic Usage (API Design)

```csharp
using MauiNativePdfView;

// In your XAML
<pdf:PdfView 
    x:Name="pdfViewer"
    Source="{Binding PdfSource}"
    EnableZoom="True"
    EnableLinkNavigation="True"
    DocumentLoaded="OnPdfLoaded"
    PageChanged="OnPageChanged" />

// In your code-behind
pdfViewer.Source = PdfSource.FromFile("path/to/document.pdf");
// or
pdfViewer.Source = PdfSource.FromUrl("https://example.com/document.pdf");
// or
pdfViewer.Source = PdfSource.FromStream(stream);
```

## 🔧 Technical Details

### Android Implementation
- **Library**: AhmerPdfium 2.0.1 + 1.9.2
- **Maven**: io.github.ahmerafzal1:ahmer-pdfviewer:2.0.1
- **Base**: Enhanced PdfiumAndroid with 16 KB page size support
- **Min SDK**: Android 7.0 (API 24)
- **Target SDK**: Android 15 (API 35)
- **License**: Apache 2.0
- **Size Impact**: ~16MB (native libraries)
- **Key Fix**: First-page rendering bug resolved

### iOS Implementation
- **Framework**: PDFKit (system framework)
- **Min Version**: iOS 11.0+
- **Size Impact**: None (system framework)
- **License**: Apple system framework

## 📚 Documentation

- [WORKPLAN.md](WORKPLAN.md) - Comprehensive implementation plan with all phases
- [CHECKLIST.md](CHECKLIST.md) - Quick reference checklist for tracking progress
- API Reference - Coming soon
- Platform Notes - Coming soon
- Contributing Guide - Coming soon

## 🗓️ Development Timeline

**Estimated**: 3-5 weeks (18-25 days)

1. **Phase 1**: Project Setup (2-3 days)
2. **Phase 2**: Android Implementation (4-5 days)
3. **Phase 3**: iOS Implementation (4-5 days)
4. **Phase 4**: MAUI Control (3-4 days)
5. **Phase 5**: Sample App (2-3 days)
6. **Phase 6**: Documentation (2-3 days)
7. **Phase 7**: Release (1-2 days)

## 🎯 Success Criteria

- [x] Comprehensive workplan created
- [x] Project structure and solution created
- [x] Android AAR binding configured and building
- [x] Sample app created and building
- [ ] PDF files load from all supported sources
- [ ] Pinch-to-zoom works smoothly on both platforms
- [ ] Links (internal and external) are functional
- [ ] Display is consistent across iOS and Android
- [ ] API is intuitive and well-documented
- [ ] Sample app demonstrates all features
- [ ] Package published to NuGet

## 🤝 Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) (coming soon) for details.

## 📄 License

This project will be licensed under [LICENSE TO BE DETERMINED].

The underlying libraries have the following licenses:
- AndroidPdfViewer: Apache License 2.0
- PDFKit: Apple System Framework

## 🙏 Acknowledgments

- [AhmerPdfium](https://github.com/AhmerAfzal1/AhmerPdfium) by Ahmer Afzal (maintained fork)
- [AndroidPdfViewer](https://github.com/DImuthuUpe/AndroidPdfViewer) by barteksc/DImuthuUpe (original)
- Apple's PDFKit framework
- .NET MAUI team

## 📞 Contact

Project maintained by [Your Name/Organization]

---

**Note**: This project is in active development. The API and features are subject to change.

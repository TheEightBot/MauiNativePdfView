# Android Binding Notes

## AhmerPdfium Binding Status

**Date**: November 24, 2025  
**Binding Project**: MauiNativePdfView.Android.Binding  
**Status**: ✅ Successfully bound with expected warnings

---

## AAR Files

### Downloaded from Maven Central

1. **ahmer-pdfium-1.9.2.aar** (11.0 MB)
   - URL: https://repo1.maven.org/maven2/io/github/ahmerafzal1/ahmer-pdfium/1.9.2/
   - Core PDF rendering engine

2. **ahmer-pdfviewer-2.0.1.aar** (134 KB)
   - URL: https://repo1.maven.org/maven2/io/github/ahmerafzal1/ahmer-pdfviewer/2.0.1/
   - UI component for PDF viewing

---

## Build Status

**Build Result**: ✅ Success (full solution builds correctly)  
**Warnings**: 5 (NuGet version resolution - harmless)

### NuGet Version Resolution

The build shows warnings about Xamarin.Kotlin.StdLib version resolution:

- **Warning**: NU1603 - Xamarin.Kotlin.StdLib 2.1.0 not found, resolved to 2.1.21 instead
- **Impact**: None - NuGet resolved to a newer compatible version
- **Resolution**: This is expected behavior, 2.1.21 is compatible with >= 2.1.0 constraint

### iOS Minimum Version

- Changed from iOS 11.0 to iOS 12.2 (minimum supported by .NET 9)
- PDFKit is available on iOS 11.0+, so this doesn't affect functionality

---

## Generated C# Namespace Structure

```
Com.Ahmer.Pdfium
├── FindFlags (enum)
├── PdfDocument
├── PdfiumCore
└── PdfTextPage

Com.Ahmer.Pdfviewer
├── AnimationManager
├── Configurator
├── DecodingAsyncTask
├── DragPinchManager
├── PagesLoader
├── PdfFile
├── PDFView (main entry point)
├── RenderingHandler
└── Link
    ├── DefaultLinkHandler
    └── LinkHandler (interface)
└── Scroll
    ├── DefaultScrollHandle
    └── ScrollHandle (interface)
└── Source
    ├── AssetSource
    ├── ByteArraySource
    ├── DocumentSource (interface)
    ├── FileSource
    ├── InputStreamSource
    └── UriSource
└── Util
    ├── FitPolicy (enum)
    ├── ScrollDir (enum)
    ├── SnapEdge (enum)
    └── State (enum)
```

---

## Key Classes Available

### Core PDF Rendering (Com.Ahmer.Pdfium)

- **PdfiumCore**: Main rendering engine
  - `NewDocument()` - Load PDF
  - `RenderPageBitmap()` - Render to bitmap
  - `GetPageWidthPoint()`, `GetPageHeightPoint()` - Page dimensions

- **PdfDocument**: Document metadata
  - `MetaData` - Title, author, etc.
  - `Bookmarks` - Table of contents

### UI Component (Com.Ahmer.Pdfviewer)

- **PDFView**: Main Android View
  - `FromFile()`, `FromUri()`, `FromBytes()`, `FromStream()`, `FromAsset()` - Load methods
  - `EnableSwipe()`, `EnableDoubletap()` - Gesture configuration
  - `OnLoad()`, `OnPageChange()`, `OnError()` - Event handlers
  - `Load()` - Final method to load and display

- **Configurator**: Fluent API builder
  - Chained configuration methods
  - Returns PDFView after `Load()`

---

## Dependencies

### Required NuGet Packages

```xml
<PackageReference Include="Xamarin.Kotlin.StdLib" Version="2.1.0" />
```

This resolves Kotlin interop warnings and provides necessary Kotlin types.

---

## ProGuard Rules

When building release APKs, add to `proguard-rules.pro`:

```proguard
# AhmerPdfium
-keep class com.ahmer.pdfium.** { *; }
-keep class com.ahmer.pdfviewer.** { *; }

# Kotlin
-keep class kotlin.** { *; }
-dontwarn kotlin.**
```

---

## Known Issues

### 1. Kotlin Companion Objects
**Issue**: Companion objects generate duplicate nested type warnings  
**Impact**: Low - C# doesn't use companion objects  
**Workaround**: None needed, warnings are expected

### 2. EnumEntries Type Missing
**Issue**: Kotlin 1.9+ EnumEntries type not found  
**Impact**: Low - enums still work correctly  
**Resolution**: Xamarin.Kotlin.StdLib package added

### 3. Dispose() Method Hiding
**Issue**: Generated Dispose() hides Object.Dispose()  
**Impact**: Low - standard pattern  
**Workaround**: None needed, both methods work

---

## Testing Checklist

- [x] AAR files downloaded successfully
- [x] Binding project builds without errors
- [x] C# types generated correctly
- [x] PDFView class is accessible
- [x] Configurator fluent API available
- [ ] Runtime testing (Phase 2)
- [ ] PDF loading verification (Phase 2)
- [ ] Zoom functionality (Phase 2)
- [ ] Link handling (Phase 2)

---

## Next Steps (Phase 2)

1. Create wrapper classes around `PDFView`
2. Implement platform handler for MAUI
3. Test actual PDF loading and rendering
4. Verify all gesture interactions work
5. Test memory management and disposal

---

## API Examples (From Binding)

### Basic Usage Pattern (Kotlin → C#)

**Kotlin (Original)**:
```kotlin
val pdfView: PDFView = findViewById(R.id.pdfView)
pdfView.fromFile(file)
    .pages(0, 2, 1)
    .enableSwipe(true)
    .load()
```

**C# (Bound)**:
```csharp
using Com.Ahmer.Pdfviewer;

var pdfView = FindViewById<PDFView>(Resource.Id.pdfView);
pdfView.FromFile(file)
    .Pages(0, 2, 1)
    .EnableSwipe(true)
    .Load();
```

---

## Resources

- **AhmerPdfium Repository**: https://github.com/AhmerAfzal1/AhmerPdfium
- **Maven Central**: https://central.sonatype.com/artifact/io.github.ahmerafzal1
- **.NET Android Binding Docs**: https://learn.microsoft.com/dotnet/android/binding-libs/
- **Xamarin Kotlin StdLib**: https://www.nuget.org/packages/Xamarin.Kotlin.StdLib

---

**Last Updated**: November 24, 2025  
**Review**: Will be updated during Phase 2 implementation

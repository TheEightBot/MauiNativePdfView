# MauiNativePdfView - Comprehensive Workplan

## Project Overview

A .NET MAUI PDF viewer library that wraps native PDF controls for iOS (PDFKit) and Android (AndroidPdfViewer). This library will provide a consistent, cross-platform API for displaying PDFs with advanced features including zoom, link handling, and multiple source support.

## Research Summary

### iOS Implementation

- **Native Framework**: PDFKit (built-in, iOS 11.0+)
- **Key Features**:
  - Native PDF rendering with PDFView
  - Built-in zoom and gesture support
  - Link handling via PDFAnnotation
  - Document loading from files, URLs, and data
  - Thumbnail views available
  - Full Apple ecosystem support

### Android Implementation

- **Selected Library**: AhmerPdfium (maintained fork of barteksc/AndroidPdfViewer)
- **Repository**: https://github.com/AhmerAfzal1/AhmerPdfium
- **Versions**:
  - PdfViewer: v2.0.1 (`io.github.ahmerafzal1:ahmer-pdfviewer:2.0.1`)
  - Pdfium Core: v1.9.2 (`io.github.ahmerafzal1:ahmer-pdfium:1.9.2`)
- **Statistics**: 24 stars (newer fork), Apache 2.0 license, actively maintained
- **Key Features**:
  - Based on enhanced PdfiumAndroid with 16 KB page size support
  - Pinch-to-zoom, double-tap zoom
  - Link handling with DefaultLinkHandler
  - Multiple document sources (Uri, File, bytes, Stream, Asset)
  - Page fit policies (WIDTH, HEIGHT, BOTH)
  - Swipe navigation, page snapping with improved behavior
  - Annotation rendering
  - Night mode support
  - **Fixed first-page rendering bug** from original library
  - Google Play compliant for 16 KB page sizes (required Nov 1, 2025)
  - Published to Maven Central

**Why AhmerPdfium**:

1. **Critical bug fixes** - Resolves first-page rendering issues in original library
2. **16 KB page size support** - Essential for Android 15+ and Google Play requirements
3. **Active maintenance** - Recent updates specifically for modern Android
4. **Maven Central** - Professional distribution, easier than JitPack
5. **Kotlin-based** - Modern, maintainable codebase
6. Comprehensive feature set matching all requirements
7. Apache 2.0 license compatible with commercial use
8. Built on proven AndroidPdfViewer foundation
9. Support for all required document sources
10. API 24+ minimum provides access to modern Android features

## Project Structure

```
MauiNativePdfView/
├── src/
│   ├── MauiNativePdfView/                      # Main MAUI library project
│   │   ├── Controls/                           # Cross-platform controls
│   │   ├── Platforms/
│   │   │   ├── Android/                        # Android-specific implementation
│   │   │   └── iOS/                            # iOS-specific implementation
│   │   └── Interfaces/                         # Shared interfaces
│   ├── MauiNativePdfView.Android.Binding/      # Android binding project
│   │   └── Jars/                               # AAR file location
│   └── MauiNativePdfView.iOS.Binding/          # iOS binding project (if needed)
├── samples/
│   └── MauiPdfViewerSample/                    # Sample application
└── docs/                                        # Documentation

```

## Detailed Workplan & Checklist

### Phase 1: Project Setup & Infrastructure

**Branch**: `feature/project-setup`

```markdown
- [ ] Create solution structure

  - [ ] Create blank solution file
  - [ ] Set up directory structure (src, samples, docs)
  - [ ] Create .gitignore for .NET MAUI projects
  - [ ] Create README.md with project description
  - [ ] Create LICENSE file (choose appropriate license)

  - [ ] Create Android binding project
  - [ ] Create MauiNativePdfView.Android.Binding project using `dotnet new android-bindinglib`
  - [ ] Download AhmerPdfViewer 2.0.1 AAR from Maven Central
  - [ ] Download AhmerPdfium 1.9.2 AAR from Maven Central
  - [ ] Add AAR files to binding project with AndroidLibrary build action
  - [ ] Configure Java dependency verification
  - [ ] Add metadata transforms if needed for API conflicts
  - [ ] Build and verify binding generation
  - [ ] Document any binding issues in BINDING_NOTES.md- [ ] Create main MAUI library project
  - [ ] Create MauiNativePdfView project using `dotnet new mauilib`
  - [ ] Configure multi-targeting (iOS, Android)
  - [ ] Set up platform-specific folders
  - [ ] Add project references
  - [ ] Configure package metadata (NuGet)

- [ ] Initial git commit and branch
  - [ ] Stage all initial files
  - [ ] Commit with message "Initial project setup"
  - [ ] Push to remote repository (if applicable)
```

**Checkpoint**: Review project structure and build configuration

---

### Phase 2: Android Native Wrapper Implementation

**Branch**: `feature/android-implementation`

```markdown
- [ ] Design shared abstraction layer

  - [ ] Define IPdfView interface with core methods
  - [ ] Define IPdfDocument interface
  - [ ] Define enums for zoom modes, fit policies
  - [ ] Define event args classes (PageChanged, LoadComplete, Error)
  - [ ] Define PdfSource class (File, Url, Stream, Bytes, Asset)
  - [ ] Document interface contracts

- [ ] Implement Android platform wrapper

  - [ ] Create AndroidPdfViewHandler class
  - [ ] Implement PDF loading from different sources
    - [ ] Load from file path
    - [ ] Load from URL (with download handling)
    - [ ] Load from stream
    - [ ] Load from byte array
    - [ ] Load from asset
  - [ ] Implement zoom functionality
    - [ ] Configure min/mid/max zoom levels
    - [ ] Enable pinch-to-zoom
    - [ ] Enable double-tap zoom
  - [ ] Implement link handling
    - [ ] Configure DefaultLinkHandler
    - [ ] Add custom link handler support
    - [ ] Test internal PDF links
    - [ ] Test external URL links
  - [ ] Configure display options
    - [ ] Set page fit policy (WIDTH by default)
    - [ ] Enable swipe navigation
    - [ ] Configure spacing between pages
  - [ ] Wire up event handlers
    - [ ] OnLoad event
    - [ ] OnPageChange event
    - [ ] OnError event
    - [ ] OnTap event
  - [ ] Handle lifecycle events
    - [ ] Memory management
    - [ ] Dispose pattern
    - [ ] Configuration changes

- [ ] Testing and validation
  - [ ] Test with local PDF files
  - [ ] Test with URL-based PDFs
  - [ ] Test with stream input
  - [ ] Test zoom interactions
  - [ ] Test link navigation
  - [ ] Test page navigation
  - [ ] Test error scenarios
  - [ ] Verify memory usage
```

**Checkpoint**: Android implementation review and testing

---

### Phase 3: iOS Native Wrapper Implementation

**Branch**: `feature/ios-implementation`

```markdown
- [ ] Create iOS binding project structure

  - [ ] Verify PDFKit availability (system framework)
  - [ ] Document PDFKit API usage patterns
  - [ ] Create binding helper classes if needed

- [ ] Implement iOS platform wrapper

  - [ ] Create iOSPdfViewHandler class
  - [ ] Configure PDFView component
  - [ ] Implement PDF loading from different sources
    - [ ] Load from file path (NSUrl)
    - [ ] Load from URL with async download
    - [ ] Load from NSData (bytes)
    - [ ] Load from stream
    - [ ] Load from asset bundle
  - [ ] Implement zoom functionality
    - [ ] Configure auto-scaling
    - [ ] Enable pinch-to-zoom gestures
    - [ ] Set min/max scale factors
  - [ ] Implement link handling
    - [ ] Handle PDFAnnotation link types
    - [ ] Process internal destination links
    - [ ] Process external URL links
    - [ ] Add custom URL handling
  - [ ] Configure display options
    - [ ] Set display mode (SinglePageContinuous)
    - [ ] Configure page breaking
    - [ ] Set display direction
  - [ ] Wire up event handlers
    - [ ] Document loaded notification
    - [ ] Page changed notification
    - [ ] Error handling
  - [ ] Handle lifecycle events
    - [ ] Memory management
    - [ ] Dispose pattern
    - [ ] Orientation changes

- [ ] Testing and validation
  - [ ] Test with local PDF files
  - [ ] Test with URL-based PDFs
  - [ ] Test with NSData input
  - [ ] Test zoom interactions
  - [ ] Test link navigation
  - [ ] Test page navigation
  - [ ] Test error scenarios
  - [ ] Test on different iOS versions (11+)
```

**Checkpoint**: iOS implementation review and testing

---

### Phase 4: MAUI Control & Cross-Platform API

**Branch**: `feature/maui-control`

```markdown
- [ ] Design MAUI control

  - [ ] Create PdfView class (inheriting from View)
  - [ ] Define bindable properties
    - [ ] Source (PdfSource)
    - [ ] CurrentPage (int)
    - [ ] PageCount (int, read-only)
    - [ ] ZoomScale (double)
    - [ ] MinZoomScale (double)
    - [ ] MaxZoomScale (double)
    - [ ] EnableZoom (bool)
    - [ ] EnableLinkNavigation (bool)
    - [ ] FitMode (enum)
  - [ ] Define events
    - [ ] DocumentLoaded
    - [ ] PageChanged
    - [ ] LoadError
    - [ ] LinkClicked
  - [ ] Define commands
    - [ ] LoadDocumentCommand
    - [ ] GoToPageCommand
    - [ ] ZoomToScaleCommand

- [ ] Implement MAUI handler

  - [ ] Create PdfViewHandler class
  - [ ] Implement CreatePlatformView for Android
  - [ ] Implement CreatePlatformView for iOS
  - [ ] Map properties to native controls
    - [ ] Map Source property
    - [ ] Map zoom properties
    - [ ] Map display properties
    - [ ] Map event handlers
  - [ ] Implement property mappers
  - [ ] Implement command mappers
  - [ ] Handle platform-specific differences gracefully

- [ ] Implement helper classes

  - [ ] PdfSource class with factory methods
    - [ ] FromFile(string path)
    - [ ] FromUrl(string url)
    - [ ] FromStream(Stream stream)
    - [ ] FromBytes(byte[] data)
    - [ ] FromResource(string resourceName)
  - [ ] PdfLoadErrorEventArgs
  - [ ] PdfPageChangedEventArgs
  - [ ] PdfLinkClickedEventArgs

- [ ] API consistency verification
  - [ ] Ensure Android and iOS behave identically
  - [ ] Document platform-specific limitations
  - [ ] Add platform-specific extension methods if needed
  - [ ] Create abstraction tests
```

**Checkpoint**: Cross-platform API review

---

### Phase 5: Sample Application Development

**Branch**: `feature/sample-app`

```markdown
- [ ] Create sample MAUI application
  - [ ] Create new MAUI app project
  - [ ] Reference MauiNativePdfView library
  - [ ] Design sample UI
    - [ ] PDF viewer page
    - [ ] Navigation controls
    - [ ] Zoom controls
    - [ ] Source selection
  - [ ] Add sample PDF files to resources
- [ ] Implement feature demonstrations

  - [ ] Demo: Load PDF from local file
    - [ ] File picker integration
    - [ ] Display selected PDF
  - [ ] Demo: Load PDF from URL
    - [ ] URL input field
    - [ ] Loading indicator
    - [ ] Error handling
  - [ ] Demo: Load PDF from embedded resource
    - [ ] Bundle sample PDFs
    - [ ] Resource loading
  - [ ] Demo: Zoom controls
    - [ ] Zoom in/out buttons
    - [ ] Reset zoom
    - [ ] Display current zoom level
  - [ ] Demo: Page navigation
    - [ ] Previous/next page buttons
    - [ ] Go to page number input
    - [ ] Display current page / total pages
  - [ ] Demo: Link interaction
    - [ ] PDF with internal links
    - [ ] PDF with external links
    - [ ] Link click event logging

- [ ] Add sample PDF files

  - [ ] Simple text PDF
  - [ ] PDF with images
  - [ ] PDF with internal links
  - [ ] PDF with external links
  - [ ] Multi-page document
  - [ ] Large PDF for performance testing

- [ ] Create documentation in sample
  - [ ] README for sample app
  - [ ] Code comments explaining usage
  - [ ] Screenshots of features
```

**Checkpoint**: Sample app review and user feedback

---

### Phase 6: Documentation & Polish

**Branch**: `feature/documentation`

```markdown
- [ ] Create comprehensive documentation
  - [ ] README.md with quick start guide
  - [ ] INSTALLATION.md with setup instructions
  - [ ] API_REFERENCE.md with complete API docs
  - [ ] PLATFORM_NOTES.md with platform-specific details
  - [ ] CHANGELOG.md for version history
  - [ ] CONTRIBUTING.md for contributors
- [ ] Add XML documentation
  - [ ] Document all public classes
  - [ ] Document all public methods
  - [ ] Document all public properties
  - [ ] Document all events
  - [ ] Add code examples in documentation
- [ ] Create usage examples
  - [ ] Basic usage example
  - [ ] Advanced configuration example
  - [ ] Error handling example
  - [ ] Custom link handling example
  - [ ] Memory management example
- [ ] Performance optimization
  - [ ] Profile memory usage
  - [ ] Optimize large PDF loading
  - [ ] Implement progressive loading if needed
  - [ ] Add memory cleanup best practices
- [ ] Add unit tests (if applicable)
  - [ ] Test PdfSource factory methods
  - [ ] Test property mapping
  - [ ] Test event handling
- [ ] Code quality improvements
  - [ ] Add code analyzers
  - [ ] Fix any warnings
  - [ ] Ensure consistent code style
  - [ ] Add EditorConfig
```

**Checkpoint**: Documentation review

---

### Phase 7: NuGet Package & Release

**Branch**: `release/v1.0.0`

```markdown
- [ ] Prepare for NuGet packaging
  - [ ] Configure package metadata
    - [ ] Package ID
    - [ ] Version number (1.0.0)
    - [ ] Authors and description
    - [ ] License information
    - [ ] Project URL and repository
    - [ ] Tags for discoverability
    - [ ] Icon and readme for NuGet
  - [ ] Include XML documentation
  - [ ] Configure multi-targeting properly
  - [ ] Add dependencies to package spec
- [ ] Build and test package
  - [ ] Build release configuration
  - [ ] Create NuGet package
  - [ ] Test package installation locally
  - [ ] Verify package contents
  - [ ] Test in clean sample project
- [ ] Create release artifacts
  - [ ] Tag release in git (v1.0.0)
  - [ ] Create GitHub release (if applicable)
  - [ ] Include release notes
  - [ ] Attach compiled binaries
- [ ] Publish package
  - [ ] Publish to NuGet.org (or private feed)
  - [ ] Verify package listing
  - [ ] Test package installation from NuGet
- [ ] Post-release tasks
  - [ ] Announce release
  - [ ] Update documentation with installation instructions
  - [ ] Monitor for issues
  - [ ] Plan next iteration
```

**Checkpoint**: Final release review

---

## Branching Strategy

### Main Branches

- `main` - Production-ready code
- `develop` - Integration branch for features

### Feature Branches

- `feature/project-setup` - Initial setup
- `feature/android-implementation` - Android wrapper
- `feature/ios-implementation` - iOS wrapper
- `feature/maui-control` - MAUI control layer
- `feature/sample-app` - Sample application
- `feature/documentation` - Documentation
- `release/v1.0.0` - Release preparation

### Branch Workflow

1. Create feature branch from `develop`
2. Implement and test feature
3. Commit regularly with descriptive messages
4. Get approval/feedback before merging
5. Merge to `develop` via pull request
6. Delete feature branch after merge

---

## Commit Message Convention

Format: `<type>(<scope>): <subject>`

Types:

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes
- `refactor`: Code refactoring
- `test`: Test additions/changes
- `chore`: Build process or auxiliary tool changes

Examples:

- `feat(android): implement PDF loading from URL`
- `fix(ios): resolve memory leak in PDFView handler`
- `docs(readme): add installation instructions`
- `refactor(core): simplify PdfSource factory methods`

---

## Key Dependencies

### .NET MAUI

- Target: .NET 8+ or .NET 9
- MAUI Workload: Latest stable

### Android

- AhmerPdfViewer: 2.0.1 (`io.github.ahmerafzal1:ahmer-pdfviewer:2.0.1`)
- AhmerPdfium: 1.9.2 (`io.github.ahmerafzal1:ahmer-pdfium:1.9.2`)
- Minimum Android SDK: API 24 (Android 7.0)
- Target Android SDK: API 35 (Android 15)
- Build Tools: AGP 8.13.0+
- NDK: r28+

### iOS

- PDFKit: System framework (iOS 11.0+)
- Minimum iOS: 11.0
- Target iOS: 17.0+

---

## Success Criteria

- ✅ PDF files load from all supported sources (file, URL, stream, bytes, assets)
- ✅ Pinch-to-zoom works smoothly on both platforms
- ✅ Links (internal and external) are clickable and functional
- ✅ Display is consistent across iOS and Android
- ✅ Memory usage is reasonable for large PDFs
- ✅ API is intuitive and well-documented
- ✅ Sample app demonstrates all features
- ✅ Package is published to NuGet
- ✅ No critical bugs in core functionality

---

## Timeline Estimate

- **Phase 1**: 2-3 days
- **Phase 2**: 4-5 days
- **Phase 3**: 4-5 days
- **Phase 4**: 3-4 days
- **Phase 5**: 2-3 days
- **Phase 6**: 2-3 days
- **Phase 7**: 1-2 days

**Total**: Approximately 18-25 days (3-5 weeks)

---

## Notes & Considerations

### Android Considerations

- AhmerPdfium uses enhanced PdfiumAndroid which adds ~16MB to app size (native libraries for multiple architectures)
- Consider APK splitting for production apps to reduce size
- 16 KB page size support is required for Google Play as of Nov 1, 2025 (fully supported in AhmerPdfium)
- ProGuard rules required: `-keep class com.ahmer.pdfium.** { *; }`
- Cannot load PDFs directly from URLs; must download first
- API 24+ minimum (Android 7.0+) - 99%+ of devices as of 2025
- First-page rendering bug from original library is fixed
- Search functionality is implemented (highlighting in progress)

### iOS Considerations

- PDFKit is a system framework (no size overhead)
- Available on iOS 11.0+, macOS 10.4+, Mac Catalyst 11.0+
- Excellent performance with native rendering
- Built-in gesture recognizers
- Full annotation support available

### Cross-Platform Considerations

- Abstract away platform differences in public API
- Document any unavoidable platform limitations
- Ensure consistent event firing between platforms
- Handle errors consistently
- Consider using MAUI's IPlatformApplication for initialization

### Performance Considerations

- Large PDFs (>50MB) may require special handling
- Consider lazy loading of pages
- Implement proper disposal to prevent memory leaks
- Monitor memory usage during testing

### Maintenance Considerations

- AhmerPdfium binding will need updates when library updates
- Monitor AhmerAfzal1/AhmerPdfium repository for updates
- Keep up with .NET MAUI updates
- Monitor for Android/iOS SDK changes
- Plan for breaking changes in major versions
- Library is actively maintained with recent commits

---

## Risk Mitigation

| Risk                                | Impact | Mitigation                                                                  |
| ----------------------------------- | ------ | --------------------------------------------------------------------------- |
| AndroidPdfViewer binding complexity | High   | Use Native Library Interop pattern, create thin wrapper API                 |
| Platform API differences            | Medium | Design abstraction layer carefully, document limitations                    |
| Large file memory issues            | Medium | Implement streaming, lazy loading, proper disposal                          |
| Android binding maintenance         | Medium | Keep wrapper API minimal, document update process                           |
| NuGet package size                  | Low    | Use APK splitting guidance in docs, consider separate packages per platform |

---

## Future Enhancements (Post v1.0)

- Text selection and search
- Annotation support (highlight, notes)
- Form filling capabilities
- Thumbnail view
- Print functionality
- Bookmark support
- Password-protected PDF support
- Page rotation
- Multiple display modes
- Accessibility features
- Windows platform support (if requested)

---

_This workplan is a living document and will be updated as the project progresses._

---

## Phase 4: Core Feature Enhancements

**Branch**: `feature/core-enhancements`

### Goals
Achieve maximum feature parity between Android and iOS platforms by adding commonly supported features.

### Tasks

#### 4.1 Scroll Direction Control
- [ ] Create `ScrollOrientation` enum (Vertical, Horizontal)
- [ ] Add `ScrollOrientation` property to IPdfView interface
- [ ] Implement in Android PdfViewAndroid (swipeHorizontal configurator)
- [ ] Implement in iOS PdfViewiOS (DisplayDirection property)
- [ ] Add bindable property to PdfView MAUI control
- [ ] Update handler mappers for both platforms
- [ ] Test horizontal and vertical scrolling modes
- [ ] Update sample app with orientation toggle

#### 4.2 Default Page Support
- [ ] Add `DefaultPage` property to IPdfView interface
- [ ] Implement in Android (defaultPage configurator)
- [ ] Implement in iOS (goToPage after load)
- [ ] Add bindable property to PdfView control
- [ ] Update handler mappers
- [ ] Test default page on initial load
- [ ] Document in sample app

#### 4.3 Rendering Quality Control
- [ ] Add `EnableAntialiasing` property (Android only)
- [ ] Add `UseBestQuality` property (RGB_565 vs ARGB_8888, Android only)
- [ ] Implement in Android PdfViewAndroid
- [ ] Add platform-specific documentation
- [ ] Add bindable properties to PdfView
- [ ] Test visual quality differences
- [ ] Update sample app with quality toggles

#### 4.4 Background Color Customization
- [ ] Add `BackgroundColor` property to IPdfView
- [ ] Implement in Android (view background)
- [ ] Implement in iOS (view backgroundColor)
- [ ] Add bindable property to PdfView
- [ ] Update handler mappers
- [ ] Test with various colors
- [ ] Add color picker to sample app

#### 4.5 Additional Events
- [ ] Add `Tapped` event for single tap detection
- [ ] Add `Rendered` event for initial render complete
- [ ] Create TappedEventArgs class
- [ ] Create RenderedEventArgs class
- [ ] Implement in Android (onTap, onRender listeners)
- [ ] Implement in iOS (tap gesture recognizer, notification)
- [ ] Update handler event forwarding
- [ ] Test event firing
- [ ] Document events in sample app

**Deliverables**:
- Enhanced IPdfView interface with 5+ new properties/events
- Cross-platform implementation of all core features
- Updated sample app demonstrating new capabilities
- Documentation for all new features

---

## Phase 5: Android-Specific Enhancements

**Branch**: `feature/android-enhancements`

### Goals
Add Android-specific features that leverage AhmerPdfium's advanced capabilities.

### Tasks

#### 5.1 ViewPager-Style Navigation
- [ ] Add `PageSnap` property (snap to page boundaries)
- [ ] Add `PageFling` property (single page per fling)
- [ ] Add `AutoSpacing` property (fit pages independently)
- [ ] Add `FitEachPage` property (relative scaling)
- [ ] Implement in Android PdfViewAndroid
- [ ] Add bindable properties (Android-only attributes)
- [ ] Test ViewPager behavior
- [ ] Document Android-specific features

#### 5.2 Mid-Level Zoom
- [ ] Add `MidZoom` property (three-level zoom)
- [ ] Implement in Android (setMidZoom)
- [ ] Test zoom level transitions
- [ ] Document zoom behavior differences

#### 5.3 Night Mode
- [ ] Add `NightMode` property
- [ ] Implement in Android (nightMode configurator)
- [ ] Test color inversion
- [ ] Add toggle to sample app
- [ ] Document as Android-only feature

#### 5.4 Advanced Events
- [ ] Add `PageScrolling` event with scroll offset
- [ ] Add `LongPressed` event
- [ ] Create PageScrollingEventArgs
- [ ] Create LongPressedEventArgs
- [ ] Implement listeners in Android
- [ ] Update handler event forwarding
- [ ] Test event scenarios

#### 5.5 Password-Protected PDFs
- [ ] Add `Password` property to PdfSource
- [ ] Implement in Android (password configurator)
- [ ] Test with encrypted PDFs
- [ ] Handle password errors
- [ ] Document password support

**Deliverables**:
- 10+ Android-specific enhancements
- ViewPager-style navigation options
- Night mode support
- Password-protected PDF handling
- Comprehensive documentation of platform differences

---

## Phase 6: iOS-Specific Enhancements

**Branch**: `feature/ios-enhancements`

### Goals
Leverage iOS PdfKit's unique capabilities for enhanced iOS experience.

### Tasks

#### 6.1 Extended Display Modes
- [ ] Create `DisplayMode` enum (SinglePage, SinglePageContinuous, TwoUp, TwoUpContinuous)
- [ ] Add `DisplayMode` property to IPdfView
- [ ] Implement in iOS PdfViewiOS
- [ ] Update FitPolicy to DisplayMode mapping
- [ ] Test all display modes
- [ ] Document iOS display capabilities

#### 6.2 Enhanced Document Properties
- [ ] Add `Creator` property
- [ ] Add `Keywords` property  
- [ ] Add `CreationDate` property
- [ ] Add `ModificationDate` property
- [ ] Implement in iOS (document attributes)
- [ ] Expose through DocumentLoadedEventArgs
- [ ] Test metadata extraction
- [ ] Update sample app to display properties

#### 6.3 Page Labels
- [ ] Add `GetPageLabel(int)` method
- [ ] Implement in iOS (page label support)
- [ ] Return custom page numbering
- [ ] Test with documents using page labels
- [ ] Document page label support

#### 6.4 Thumbnail Support
- [ ] Research PDFThumbnailView integration
- [ ] Design thumbnail API
- [ ] Add thumbnail generation methods
- [ ] Implement in iOS
- [ ] Create sample thumbnail viewer
- [ ] Document thumbnail capabilities

**Deliverables**:
- Extended iOS display modes
- Rich document metadata
- Page label support
- Thumbnail generation API
- Documentation of iOS-specific features

---

## Phase 7: Annotation Support

**Branch**: `feature/annotations`

### Goals
Enable PDF annotation rendering and interaction.

### Tasks

#### 7.1 Annotation Rendering
- [ ] Add `EnableAnnotationRendering` property
- [ ] Implement in Android (enableAnnotationRendering configurator)
- [ ] Implement in iOS (default behavior, control visibility)
- [ ] Test with annotated PDFs
- [ ] Document annotation support

#### 7.2 Annotation Events
- [ ] Add `AnnotationTapped` event
- [ ] Create AnnotationTappedEventArgs
- [ ] Implement in both platforms
- [ ] Get annotation type and content
- [ ] Test annotation interaction
- [ ] Document annotation events

**Deliverables**:
- Annotation rendering support
- Annotation interaction events
- Cross-platform annotation handling
- Sample annotated PDF demonstrations

---

## Phase 8: Advanced Features (Future)

**Branch**: `feature/advanced`

### Goals
Implement advanced PDF capabilities for power users.

### Potential Features
- Text selection and copying
- PDF search functionality
- Outline/bookmark navigation
- Custom drawing overlays
- Form field support
- PDF creation/editing
- Accessibility enhancements

**Note**: These features require significant research and may be implemented based on user demand and feasibility analysis.

---

## Updated Timeline

| Phase | Duration | Status |
|-------|----------|--------|
| Phase 1: Project Setup | 1-2 days | ✅ Complete |
| Phase 2: Android Implementation | 2-3 days | ✅ Complete |
| Phase 3: iOS Implementation | 2-3 days | ✅ Complete |
| Phase 4: Core Enhancements | 2-3 days | ⏳ Planned |
| Phase 5: Android Enhancements | 1-2 days | ⏳ Planned |
| Phase 6: iOS Enhancements | 1-2 days | ⏳ Planned |
| Phase 7: Annotation Support | 1 day | ⏳ Planned |
| Phase 8: Documentation & Polish | 2-3 days | ⏳ Planned |
| Phase 9: NuGet Package & Release | 1-2 days | ⏳ Planned |

**Total Estimated Time**: 13-21 days for Phases 1-9
**Completed**: Phases 1-3 (6-8 days)
**Remaining**: Phases 4-9 (7-13 days)


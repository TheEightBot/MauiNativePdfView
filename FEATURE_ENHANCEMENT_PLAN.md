# Feature Enhancement Plan - MauiNativePdfView

## Philosophy

**Goal: Maximum Feature Parity Across Platforms**

We aim to provide a consistent API across iOS and Android wherever reasonably possible. When a feature exists natively on only one platform, we evaluate whether it can be reasonably implemented or simulated on the other platform:

- ✅ **Implement on both**: If we can reasonably add the feature to both platforms (even if one requires custom implementation)
- ⚠️ **Platform-specific with graceful degradation**: If implementation is truly platform-specific, provide a no-op or graceful fallback
- ❌ **Skip**: If a feature cannot be reasonably implemented cross-platform and would provide poor user experience

## Feature Comparison Analysis

### Current Implementation Status

| Feature                        | Android (AhmerPdfium) | iOS (PdfKit)  | Current Implementation | Status                    |
| ------------------------------ | --------------------- | ------------- | ---------------------- | ------------------------- |
| **Loading Sources**            |                       |               |                        |                           |
| File path                      | ✅                    | ✅            | ✅                     | Complete                  |
| URI/URL                        | ✅                    | ✅            | ✅                     | Complete                  |
| Stream                         | ✅                    | ✅            | ✅                     | Complete                  |
| Byte array                     | ✅                    | ✅            | ✅                     | Complete                  |
| Asset/Resource                 | ✅                    | ✅            | ✅                     | Complete                  |
| **Display Configuration**      |                       |               |                        |                           |
| Horizontal scrolling           | ✅                    | ✅            | ✅                     | ✅ Phase 4 Complete       |
| Vertical scrolling             | ✅                    | ✅            | ✅                     | Complete                  |
| Page spacing                   | ✅                    | ✅            | ✅                     | Complete                  |
| Auto spacing                   | ✅                    | ❌            | ❌                     | ❌ Skip (ViewPager-only)  |
| Fit policy (Width/Height/Both) | ✅                    | ✅            | ✅                     | Complete                  |
| Fit each page                  | ✅                    | ❌            | ❌                     | ❌ Skip (ViewPager-only)  |
| Page snap                      | ✅                    | ❌            | ❌                     | ❌ Skip (ViewPager-only)  |
| Page fling                     | ✅                    | ❌            | ❌                     | ❌ Skip (ViewPager-only)  |
| **Zoom & Gestures**            |                       |               |                        |                           |
| Pinch zoom                     | ✅                    | ✅            | ✅                     | Complete                  |
| Double tap zoom                | ✅                    | ✅            | ✅                     | Complete                  |
| Min/Max zoom                   | ✅                    | ✅            | ✅                     | Complete                  |
| Mid zoom level                 | ✅                    | ❌            | ❌                     | ⚠️ Android-only            |
| Enable/disable zoom            | ✅                    | ✅            | ✅                     | Complete                  |
| Enable/disable swipe           | ✅                    | ✅            | ✅                     | Complete                  |
| Long press                     | ✅                    | ❌            | ❌                     | ✅ Add with gesture       |
| **Visual Enhancements**        |                       |               |                        |                           |
| Night mode / Dark mode         | ✅                    | ❌            | ❌                     | ✅ Add with Core Graphics |
| Antialiasing                   | ✅                    | ✅ (default)  | ✅                     | ✅ Phase 4 Complete       |
| Best quality (ARGB_8888)       | ✅                    | ✅ (default)  | ✅                     | ✅ Phase 4 Complete       |
| Background color               | ✅                    | ✅            | ✅                     | ✅ Phase 4 Complete       |
| **Annotations & Rendering**    |                       |               |                        |                           |
| Annotation rendering           | ✅                    | ✅ (default)  | ❌                     | ✅ To Add (Phase 7)       |
| Password protection            | ✅                    | ✅            | ❌                     | ✅ To Add (Phase 5)       |
| Custom drawing (onDraw)        | ✅                    | ✅            | ❌                     | **Advanced Feature**      |
| Custom drawing all pages       | ✅                    | ✅            | ❌                     | **Advanced Feature**      |
| **Navigation & Events**        |                       |               |                        |                           |
| Current page                   | ✅                    | ✅            | ✅                     | Complete                  |
| Total pages                    | ✅                    | ✅            | ✅                     | Complete                  |
| Go to page                     | ✅                    | ✅            | ✅                     | Complete                  |
| Default page                   | ✅                    | ✅            | ✅                     | ✅ Phase 4 Complete       |
| Page filter/order              | ✅                    | ❌            | ❌                     | ❌ Skip (complex)         |
| OnLoad callback                | ✅                    | ✅            | ✅                     | Complete                  |
| OnPageChange                   | ✅                    | ✅            | ✅                     | Complete                  |
| OnPageScroll                   | ✅                    | ❌            | ❌                     | ✅ Add with scroll events |
| OnTap                          | ✅                    | ✅            | ✅                     | ✅ Phase 4 Complete       |
| OnLongPress                    | ✅                    | ❌            | ❌                     | ✅ Add with gesture       |
| OnRender                       | ✅                    | ✅            | ✅                     | ✅ Phase 4 Complete       |
| **Links & Interaction**        |                       |               |                        |                           |
| Link navigation                | ✅                    | ✅            | ✅                     | Complete                  |
| Custom link handler            | ✅                    | ❌            | ❌                     | ⚠️ Android-only            |
| LinkTapped event               | ✅                    | ✅            | ✅                     | Complete                  |
| **UI Components**              |                       |               |                        |                           |
| Scroll handle                  | ✅                    | ✅ (built-in) | ❌                     | ❌ Skip (platform UX)     |
| Custom scroll handle           | ✅                    | ❌            | ❌                     | ❌ Skip (advanced)        |
| Thumbnail view                 | ❌                    | ✅            | ❌                     | ❌ Skip (iOS-specific)    |
| **Document Info**              |                       |               |                        |                           |
| Page count                     | ✅                    | ✅            | ✅                     | Complete                  |
| Title                          | ✅                    | ✅            | ✅                     | Complete                  |
| Author                         | ✅                    | ✅            | ✅                     | Complete                  |
| Subject                        | ✅                    | ✅            | ✅                     | Complete                  |
| Creator                        | ❌                    | ✅            | ❌                     | ✅ Add with PdfDocument   |
| Keywords                       | ❌                    | ✅            | ❌                     | ✅ Add with PdfDocument   |
| Creation/Modification dates    | ❌                    | ✅            | ❌                     | ✅ Add with PdfDocument   |
| **Advanced Features**          |                       |               |                        |                           |
| Text selection                 | ❌                    | ✅            | ❌                     | ❌ Skip (complex/iOS)     |
| Text search                    | ❌                    | ✅            | ❌                     | ❌ Skip (complex/iOS)     |
| PDF outline/bookmarks          | ❌                    | ✅            | ❌                     | ❌ Skip (complex/iOS)     |

## Priority Enhancements

### Phase 4: Core Feature Parity (High Priority) ✅ COMPLETE

**Branch:** `feature/core-enhancements` (merged to main)

**Status:** ✅ Fully implemented and tested on both platforms

#### 4.1 Scroll Direction Control ✅
- ✅ Created `PdfScrollOrientation` enum (Vertical, Horizontal)
- ✅ Android: `swipeHorizontal` configurator
- ✅ iOS: `DisplayDirection` property
- ✅ MAUI control with bindable property

#### 4.2 Default Page ✅
- ✅ `DefaultPage` property for initial page on load
- ✅ Android: `defaultPage` configurator
- ✅ iOS: `goToPage` after document loads

#### 4.3 Rendering Quality ✅
- ✅ `EnableAntialiasing` property (Android-specific, iOS always on)
- ✅ `UseBestQuality` property (Android ARGB_8888, iOS always on)

#### 4.4 Background Color ✅
- ✅ `BackgroundColor` property with full Color support
- ✅ Implemented on both platforms

#### 4.5 Additional Events ✅
- ✅ `PdfTappedEventArgs` and `Tapped` event
- ✅ `RenderedEventArgs` and `Rendered` event
- ✅ Implemented on both platforms

### Phase 5: Cross-Platform Event & Interaction Enhancements (High Priority)

**Branch:** `feature/events-interactions`

**Goal:** Add events and interaction features that work consistently across both platforms

#### 5.1 Long Press Support
- Add `PdfLongPressedEventArgs` with page index and coordinates
- Add `LongPressed` event
- **Android:** Use `onLongPress` listener
- **iOS:** Add `UILongPressGestureRecognizer`
- Both platforms: Consistent behavior and event args

#### 5.2 Page Scrolling Event
- Add `PageScrollingEventArgs` with scroll offset and direction
- Add `PageScrolling` event
- **Android:** Use `onPageScroll` listener with native offset
- **iOS:** Implement using `UIScrollViewDelegate` or observation
- Both platforms: Report scroll position consistently

#### 5.3 Password-Protected PDFs
- Add `Password` property to `PdfSource`
- Support encrypted PDFs on both platforms
- **Android:** Use `password()` configurator
- **iOS:** Use `PdfDocument(url, password)` constructor
- Handle incorrect password errors gracefully

### Phase 6: Visual Enhancement Parity (Medium Priority)

**Branch:** `feature/visual-enhancements`

**Goal:** Achieve visual feature parity where reasonably possible

#### 6.1 Night Mode / Dark Mode
- Add `EnableNightMode` property
- Inverts PDF colors for dark reading
- **Android:** Use built-in `nightMode()` configurator
- **iOS:** Implement using Core Graphics filters/blend modes
  - Apply `CIColorInvert` filter or similar
  - May have performance impact - document limitations

#### 6.2 Display Mode Enhancements (iOS)
- Expand `PdfDisplayMode` enum beyond current FitPolicy
- Add: `SinglePage`, `SinglePageContinuous`, `TwoUp`, `TwoUpContinuous`
- **iOS:** Map directly to `PdfDisplayMode` enum
- **Android:** Map to closest equivalent behavior with configurator
  - SinglePage: pageSnap=true, pageFling=true
  - Continuous: existing behavior
  - TwoUp: Not directly supported, document limitation

### Phase 7: Enhanced Document Metadata (Low Priority)

**Branch:** `feature/metadata`

**Goal:** Expose richer document metadata across platforms

#### 7.1 Extended Document Properties
- Add properties to `DocumentLoadedEventArgs`:
  - `Creator` (string)
  - `Keywords` (string)
  - `CreationDate` (DateTime?)
  - `ModificationDate` (DateTime?)
- **iOS:** Read from `PdfDocument.DocumentAttributes` dictionary
- **Android:** Attempt to read from `PdfDocument` metadata if available
  - Use PdfiumCore API if exposed
  - Otherwise return null (graceful degradation)

### Phase 8: Annotation Support (Low Priority)

**Branch:** `feature/annotations`

**Goal:** Enable PDF annotation rendering and basic interaction

#### 8.1 Annotation Rendering
- Add `EnableAnnotationRendering` property
- Render PDF annotations, forms, comments
- **Android:** Use `enableAnnotationRendering()` configurator
- **iOS:** Enabled by default, add toggle if possible

#### 8.2 Annotation Events
- Add `AnnotationTappedEventArgs` with annotation details
- Add `AnnotationTapped` event
- Both platforms: Report annotation type and content

### Features We're Skipping (Out of Scope)

**ViewPager-Style Android Features:**
- ❌ `PageSnap`, `PageFling`, `AutoSpacing`, `FitEachPage`
- **Reason:** These are tightly coupled to Android's ViewPager pattern and cannot be reasonably replicated on iOS without significant custom implementation that would diverge from native iOS UX patterns

**Platform-Specific Advanced Features:**
- ❌ `MidZoom` (Android three-level zoom) - iOS has smooth continuous zoom
- ❌ Custom link handlers (Android) - Would require complex iOS implementation
- ❌ Scroll handles (both) - Platform-specific UI components
- ❌ Thumbnails (iOS PDFThumbnailView) - Would require significant custom Android UI
- ❌ Page filtering/ordering (Android) - Complex feature with limited use case
- ❌ Text selection, search, bookmarks (iOS) - Advanced features requiring extensive work

## Implementation Strategy

### Immediate Next Steps (Phase 4)

1. **Create Feature Branch**

   ```bash
   git checkout -b feature/core-enhancements
   ```

2. **Update Abstractions**

   - Add new properties/events to `IPdfView`
   - Create new enums (ScrollOrientation, DisplayQuality, etc.)
   - Add new event args classes

3. **Implement Android**

   - Update PdfViewAndroid with new Configurator options
   - Wire up new events
   - Test each feature

4. **Implement iOS**

   - Update PdfViewiOS with PdfKit equivalents
   - Handle platform differences gracefully
   - Test each feature

5. **Update MAUI Control**

   - Add bindable properties
   - Update handler mappers
   - Document new features

6. **Update Sample App**
   - Add UI controls for new features
   - Demonstrate each capability
   - Create test scenarios

### Commit Strategy

Each feature should be a separate commit:

- `feat: add scroll orientation support (horizontal/vertical)`
- `feat: add default page property`
- `feat: add antialiasing and quality options`
- `feat: add background color property`
- `feat: add tapped and rendered events`

### Testing Requirements

For each new feature:

1. Test on Android device/emulator
2. Test on iOS device/simulator
3. Verify XAML binding works
4. Check event firing
5. Test edge cases

### Documentation Requirements

For each phase:

1. Update README with new features
2. Add XML documentation to all public APIs
3. Update sample app with examples
4. Create migration guide if needed

## Success Criteria

### Phase 4 Complete ✅

- ✅ Scroll orientation working both platforms
- ✅ Default page loads correctly
- ✅ Quality/antialiasing configurable
- ✅ Background color customizable
- ✅ Tapped and Rendered events firing
- ✅ All features documented
- ✅ Sample app demonstrates all features

### Overall Success Criteria:

- ✅ Maximum feature parity between platforms (no platform-only features unless truly impossible to implement)
- ✅ Clean, consistent API across iOS and Android
- ✅ Well-documented with XML comments
- ✅ Performant on both platforms
- ✅ No breaking changes to existing code
- ✅ Graceful degradation where platform limitations exist
- ✅ Ready for NuGet publication

## Updated Timeline Estimate

| Phase | Focus | Duration | Status |
|-------|-------|----------|--------|
| Phase 4 | Core Enhancements | 2-3 days | ✅ Complete |
| Phase 5 | Events & Interactions | 1-2 days | 📋 Planned |
| Phase 6 | Visual Enhancements | 1-2 days | 📋 Planned |
| Phase 7 | Document Metadata | 0.5-1 day | 📋 Planned |
| Phase 8 | Annotations | 1 day | 📋 Planned |

**Total remaining**: ~4-6 days for Phases 5-8
**Completed**: Phase 4 (2-3 days)
**Project total**: ~6-9 days

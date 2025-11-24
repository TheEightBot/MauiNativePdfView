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
| Page snap / Single page mode   | ✅                    | ✅            | ✅                     | ✅ Phase 5.1 Complete     |
| Page fling                     | ✅                    | ❌            | ❌                     | ❌ Skip (ViewPager-only)  |
| **Zoom & Gestures**            |                       |               |                        |                           |
| Pinch zoom                     | ✅                    | ✅            | ✅                     | Complete                  |
| Double tap zoom                | ✅                    | ✅            | ✅                     | Complete                  |
| Min/Max zoom                   | ✅                    | ✅            | ✅                     | Complete                  |
| Mid zoom level                 | ✅                    | ❌            | ❌                     | ⚠️ Android-only            |
| Enable/disable zoom            | ✅                    | ✅            | ✅                     | Complete                  |
| Enable/disable swipe           | ✅                    | ✅            | ✅                     | Complete                  |
| Long press                     | ✅                    | ❌            | ❌                     | ❌ Skip (not critical)    |
| **Visual Enhancements**        |                       |               |                        |                           |
| Night mode / Dark mode         | ✅                    | ❌            | ❌                     | ❌ Removed (iOS incompatible) |
| Antialiasing                   | ✅                    | ✅ (default)  | ✅                     | ✅ Phase 4 Complete       |
| Best quality (ARGB_8888)       | ✅                    | ✅ (default)  | ✅                     | ✅ Phase 4 Complete       |
| Background color               | ✅                    | ✅            | ✅                     | ✅ Phase 4 Complete       |
| **Annotations & Rendering**    |                       |               |                        |                           |
| Annotation rendering           | ✅                    | ✅ (default)  | ❌                     | ✅ To Add (Phase 7)       |
| Password protection            | ✅                    | ✅            | ✅                     | ✅ Phase 5.2 Complete     |
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
| OnPageScroll                   | ✅                    | ❌            | ❌                     | ❌ Skip (not critical)    |
| OnTap                          | ✅                    | ✅            | ✅                     | ✅ Phase 4 Complete       |
| OnLongPress                    | ✅                    | ❌            | ❌                     | ❌ Skip (not critical)    |
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

### Phase 5: Display Mode & Event Enhancements (High Priority)

**Branch:** `feature/display-events`

**Goal:** Add display modes and interaction features that work consistently across both platforms

**Status:** Partially complete - Display Mode merged, remaining items pending

#### 5.1 Single Page Display Mode (Page Snap) ✅ **COMPLETE**
- ✅ Added `PdfDisplayMode` enum: `SinglePage`, `SinglePageContinuous`
- ✅ **iOS:** Native support via `PdfDisplayMode` enum (maps directly)
- ✅ **Android:** Implemented with `pageSnap(true)`, `pageFling(true)` configurators
- ✅ **Fixed:** Android properties now trigger reload when changed after document loads
- ✅ **Fixed:** Android now preserves current page when reloading after property changes
- ✅ Removed TwoUp modes (not well-supported on Android)
- ✅ Merged to main (commit bb5d68d)

#### 5.2 Password-Protected PDFs ✅ **COMPLETE**
- ✅ Added `Password` property to `PdfSource` base class
- ✅ Support for encrypted PDFs on both platforms
- ✅ **Android:** Use `.password()` configurator
- ✅ **iOS:** Use `PdfDocument.Unlock()` method with `IsLocked` check
- ✅ Graceful error handling for incorrect/missing passwords
- ✅ Merged to main (commit ee2eed9)

### Phase 6: Enhanced Document Metadata (Low Priority)

**Branch:** `feature/metadata`

**Goal:** Expose richer document metadata across platforms

#### 6.1 Extended Document Properties
- Add properties to `DocumentLoadedEventArgs`:
  - `Creator` (string)
  - `Keywords` (string)
  - `CreationDate` (DateTime?)
  - `ModificationDate` (DateTime?)
- **iOS:** Read from `PdfDocument.DocumentAttributes` dictionary
- **Android:** Attempt to read from `PdfDocument` metadata if available
  - Use PdfiumCore API if exposed
  - Otherwise return null (graceful degradation)

### Phase 7: Annotation Support (Low Priority)

**Branch:** `feature/annotations`

**Goal:** Enable PDF annotation rendering and basic interaction

#### 7.1 Annotation Rendering
- Add `EnableAnnotationRendering` property
- Render PDF annotations, forms, comments
- **Android:** Use `enableAnnotationRendering()` configurator
- **iOS:** Enabled by default, add toggle if possible

#### 7.2 Annotation Events
- Add `AnnotationTappedEventArgs` with annotation details
- Add `AnnotationTapped` event
- Both platforms: Report annotation type and content

### Features We're Skipping (Out of Scope)

**ViewPager-Style Android Features:**
- ❌ `PageFling`, `AutoSpacing`
- **Reason:** These are tightly coupled to Android's ViewPager pattern and cannot be reasonably replicated on iOS without significant custom implementation that would diverge from native iOS UX patterns

**Fit Each Page Independently:**
- ❌ `FitEachPage` - Per-page zoom levels
- **Reason:** iOS doesn't support per-page zoom natively. Workaround would require:
  - Listening to `PageChanged` events
  - Calculating and setting `ScaleFactor` for each page manually
  - Fighting with user zoom gestures
  - Extra complexity and poor UX
  - Global `AutoScales` property already works well for most use cases
- **Research conclusion:** Skip - complexity doesn't justify marginal value

**Night Mode / Dark Mode:**
- ❌ Removed after extensive implementation attempts
- **Android:** Has native `nightMode()` configurator that works perfectly
- **iOS:** No native support in PdfKit
- **Attempted iOS implementations (all failed):**
  1. Direct CALayer filters on PdfView - no visual effect
  2. Per-page overlay with `IPdfPageOverlayViewProvider` - app crashes
  3. Transparent overlay UIView with filter - no visual effect
  4. Layer.Filters array approach - no visual effect
  5. Container view pattern with filters - no visual effect
- **Root cause:** PdfKit's PdfView renders directly to graphics context, bypassing the CALayer compositing pipeline where filters would be applied
- **Conclusion:** Cross-platform night mode not feasible without major architectural changes. Removed from project scope.
- **Alternative:** Users can use system-wide dark mode or accessibility features

**Event & Gesture Features:**
- ❌ Long press support - Not critical for core PDF viewing functionality
- ❌ Page scrolling events - Not critical, PageChanged event is sufficient for most use cases

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
| Phase 5.1 | Display Mode | 0.5 days | ✅ Complete |
| Phase 5.2 | Password Support | 0.5 days | ✅ Complete |
| Phase 6 | Document Metadata | 0.5-1 day | 📋 Planned |
| Phase 7 | Annotations | 1 day | 📋 Planned |

**Total remaining**: ~1.5-2 days for remaining phases
**Note:** Night Mode feature removed from scope after extensive research and implementation attempts
**Completed**: Phase 4 (2-3 days)
**Project total**: ~5.5-8 days

## Research Summary

### ✅ Page Snap / Single Page Mode - **COMPLETE**
- iOS has **native support** via `PdfDisplayMode.SinglePage`
- Android implemented with `pageSnap(true)` + `pageFling(true)`
- Direct property mapping, clean implementation
- ✅ **Status:** Implemented and merged

### ⚠️ Fit Each Page Independently - **SKIP**
- iOS lacks native per-page zoom support
- Workarounds are complex and provide poor UX
- Conflicts with user zoom gestures
- Global `AutoScales` is sufficient for most use cases
- **Status:** Skipped this feature

### ❌ Night Mode / Dark Mode - **REMOVED FROM SCOPE**
- **Android:** Has native `nightMode()` - works perfectly
- **iOS:** No native PdfKit support, all custom implementations failed
- **Attempted approaches:**
  1. CALayer.CompositingFilter on PdfView
  2. Per-page overlay with IPdfPageOverlayViewProvider (caused crashes)
  3. Transparent overlay UIView with filter
  4. Layer.Filters array
  5. Container view pattern with parent-level filters
- **Root issue:** PdfKit's PdfView renders directly to graphics context, bypassing CALayer compositing
- **Result:** No visual effect achieved on iOS despite multiple implementation strategies
- **Decision:** Feature removed due to iOS incompatibility
- **Alternative:** Users can use iOS system-wide accessibility features or Dark Mode where applicable

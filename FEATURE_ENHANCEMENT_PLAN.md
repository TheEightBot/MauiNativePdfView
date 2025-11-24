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
| Page snap / Single page mode   | ✅                    | ✅            | ❌                     | ✅ Add (Phase 5)          |
| Page fling                     | ✅                    | ❌            | ❌                     | ❌ Skip (ViewPager-only)  |
| **Zoom & Gestures**            |                       |               |                        |                           |
| Pinch zoom                     | ✅                    | ✅            | ✅                     | Complete                  |
| Double tap zoom                | ✅                    | ✅            | ✅                     | Complete                  |
| Min/Max zoom                   | ✅                    | ✅            | ✅                     | Complete                  |
| Mid zoom level                 | ✅                    | ❌            | ❌                     | ⚠️ Android-only           |
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
| Custom link handler            | ✅                    | ❌            | ❌                     | ⚠️ Android-only           |
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

#### 5.1 Single Page Display Mode (Page Snap) ✅ **RESEARCHED - Ready to Implement**

- Add `PdfDisplayMode` enum: `SinglePage`, `SinglePageContinuous`, `TwoUp`, `TwoUpContinuous`
- **iOS:** Native support via `PdfDisplayMode` enum (maps directly)
- **Android:** Approximate with `pageSnap(true)`, `pageFling(true)` configurators
- **Implementation:** ~2-3 hours
- **Value:** High - common user expectation for single-page viewing

#### 5.2 Long Press Support

- Add `PdfLongPressedEventArgs` with page index and coordinates
- Add `LongPressed` event
- **Android:** Use `onLongPress` listener
- **iOS:** Add `UILongPressGestureRecognizer`
- Both platforms: Consistent behavior and event args

#### 5.3 Page Scrolling Event

- Add `PageScrollingEventArgs` with scroll offset and direction
- Add `PageScrolling` event
- **Android:** Use `onPageScroll` listener with native offset
- **iOS:** Implement using `UIScrollViewDelegate` or observation
- Both platforms: Report scroll position consistently

#### 5.4 Password-Protected PDFs

- Add `Password` property to `PdfSource`
- Support encrypted PDFs on both platforms
- **Android:** Use `password()` configurator
- **iOS:** Use `PdfDocument(url, password)` constructor
- Handle incorrect password errors gracefully

### Phase 6: Night Mode (Medium Priority)

**Branch:** `feature/night-mode`

**Goal:** Implement dark mode for better reading in low-light conditions

#### 6.1 Night Mode / Dark Mode ⚠️ **RESEARCHED - Proof-of-Concept Required**

- Add `EnableNightMode` property
- Inverts PDF colors for dark reading
- **Android:** Use built-in `nightMode()` configurator (native, performant)
- **iOS:** Custom implementation using Core Graphics `CIColorInvert` filter
  - **Implementation approaches:**
    1. Layer-based filtering (simplest, may impact performance)
    2. Per-page overlay with `PdfPageOverlayViewProvider` (complex, better performance)
    3. Blend mode on superview (quick implementation)
  - **Performance considerations:**
    - GPU-intensive operation
    - Battery drain on continuous use
    - May not be as crisp as Android native
    - Requires testing on actual devices (not just simulator)
- **Implementation:** ~4-6 hours (mostly iOS custom work)
- **Value:** Medium - requested feature with performance trade-offs
- **Requirements:**
  - Performance testing required on iOS devices
  - Documentation of limitations and battery impact
  - Consider optional/experimental flag initially
  - User warning about performance on iOS

**Research Notes:**

- iOS PdfKit does not have native night mode support
- Core Image filters (`CIColorInvert`) can invert colors but add processing overhead
- Alternative: Could use `CIColorControls` with reduced brightness/increased contrast
- May want to provide multiple "night mode styles" (invert, sepia, grayscale) to justify the effort

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

| Phase   | Focus                 | Duration  | Status                     |
| ------- | --------------------- | --------- | -------------------------- |
| Phase 4 | Core Enhancements     | 2-3 days  | ✅ Complete                |
| Phase 5 | Display Mode & Events | 1-2 days  | 🔬 Researched              |
| Phase 6 | Night Mode            | 0.5-1 day | 🔬 Researched (POC needed) |
| Phase 7 | Document Metadata     | 0.5-1 day | 📋 Planned                 |
| Phase 8 | Annotations           | 1 day     | 📋 Planned                 |

**Total remaining**: ~3.5-5 days for Phases 5-8
**Completed**: Phase 4 (2-3 days)
**Project total**: ~5.5-8 days

## Research Summary (Phase 5 & 6)

### ✅ Page Snap / Single Page Mode - **FEASIBLE & EASY**

- iOS has **native support** via `PdfDisplayMode.SinglePage`
- Android can approximate with `pageSnap(true)` + `pageFling(true)`
- Direct property mapping, no custom code needed
- **Recommendation:** Implement immediately

### ⚠️ Fit Each Page Independently - **SKIP**

- iOS lacks native per-page zoom support
- Workarounds are complex and provide poor UX
- Conflicts with user zoom gestures
- Global `AutoScales` is sufficient for most use cases
- **Recommendation:** Skip this feature

### ⚠️ Night Mode / Dark Mode - **FEASIBLE with Caveats**

- Android has native `nightMode()` - works perfectly
- iOS requires custom Core Image filter implementation
- Performance concerns on iOS (GPU overhead, battery drain)
- Need proof-of-concept to validate approach
- Should document limitations clearly
- **Recommendation:** Implement with performance warnings

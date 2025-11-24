# Feature Enhancement Plan - MauiNativePdfView

## Feature Comparison Analysis

### Current Implementation Status

| Feature | Android (AhmerPdfium) | iOS (PdfKit) | Current Implementation | Status |
|---------|----------------------|--------------|----------------------|--------|
| **Loading Sources** | | | | |
| File path | ✅ | ✅ | ✅ | Complete |
| URI/URL | ✅ | ✅ | ✅ | Complete |
| Stream | ✅ | ✅ | ✅ | Complete |
| Byte array | ✅ | ✅ | ✅ | Complete |
| Asset/Resource | ✅ | ✅ | ✅ | Complete |
| **Display Configuration** | | | | |
| Horizontal scrolling | ✅ | ✅ | ❌ | **To Add** |
| Vertical scrolling | ✅ | ✅ | ✅ | Complete |
| Page spacing | ✅ | ✅ | ✅ | Complete |
| Auto spacing | ✅ | ❌ | ❌ | **To Add (Android only)** |
| Fit policy (Width/Height/Both) | ✅ | ✅ | ✅ | Complete |
| Fit each page | ✅ | ❌ | ❌ | **To Add (Android only)** |
| Page snap | ✅ | ❌ | ❌ | **To Add (Android only)** |
| Page fling | ✅ | ❌ | ❌ | **To Add (Android only)** |
| **Zoom & Gestures** | | | | |
| Pinch zoom | ✅ | ✅ | ✅ | Complete |
| Double tap zoom | ✅ | ✅ | ✅ | Complete |
| Min/Max zoom | ✅ | ✅ | ✅ | Complete |
| Mid zoom level | ✅ | ❌ | ❌ | **To Add (Android only)** |
| Enable/disable zoom | ✅ | ✅ | ✅ | Complete |
| Enable/disable swipe | ✅ | ✅ | ✅ | Complete |
| Long press | ✅ | ❌ | ❌ | **To Add (Android only)** |
| **Visual Enhancements** | | | | |
| Night mode / Dark mode | ✅ | ❌ | ❌ | **To Add (Android only)** |
| Antialiasing | ✅ | ✅ (default) | ❌ | **To Add** |
| Best quality (ARGB_8888) | ✅ | ✅ (default) | ❌ | **To Add** |
| Background color | ✅ | ✅ | ❌ | **To Add** |
| **Annotations & Rendering** | | | | |
| Annotation rendering | ✅ | ✅ (default) | ❌ | **To Add** |
| Password protection | ✅ | ✅ | ❌ | **To Add** |
| Custom drawing (onDraw) | ✅ | ✅ | ❌ | **Advanced Feature** |
| Custom drawing all pages | ✅ | ✅ | ❌ | **Advanced Feature** |
| **Navigation & Events** | | | | |
| Current page | ✅ | ✅ | ✅ | Complete |
| Total pages | ✅ | ✅ | ✅ | Complete |
| Go to page | ✅ | ✅ | ✅ | Complete |
| Default page | ✅ | ✅ | ❌ | **To Add** |
| Page filter/order | ✅ | ❌ | ❌ | **To Add (Android only)** |
| OnLoad callback | ✅ | ✅ | ✅ | Complete |
| OnPageChange | ✅ | ✅ | ✅ | Complete |
| OnPageScroll | ✅ | ❌ | ❌ | **To Add (Android only)** |
| OnTap | ✅ | ✅ | ❌ | **To Add** |
| OnLongPress | ✅ | ❌ | ❌ | **To Add (Android only)** |
| OnRender | ✅ | ✅ | ❌ | **To Add** |
| **Links & Interaction** | | | | |
| Link navigation | ✅ | ✅ | ✅ | Complete |
| Custom link handler | ✅ | ❌ | ❌ | **To Add (Android only)** |
| LinkTapped event | ✅ | ✅ | ✅ | Complete |
| **UI Components** | | | | |
| Scroll handle | ✅ | ✅ (built-in) | ❌ | **To Add** |
| Custom scroll handle | ✅ | ❌ | ❌ | **Advanced Feature** |
| Thumbnail view | ❌ | ✅ | ❌ | **To Add (iOS only)** |
| **Document Info** | | | | |
| Page count | ✅ | ✅ | ✅ | Complete |
| Title | ✅ | ✅ | ✅ | Complete |
| Author | ✅ | ✅ | ✅ | Complete |
| Subject | ✅ | ✅ | ✅ | Complete |
| Creator | ❌ | ✅ | ❌ | **To Add (iOS only)** |
| Keywords | ❌ | ✅ | ❌ | **To Add (iOS only)** |
| **Advanced Features** | | | | |
| Text selection | ❌ | ✅ | ❌ | **Future** |
| Text search | ❌ | ✅ | ❌ | **Future** |
| PDF outline/bookmarks | ❌ | ✅ | ❌ | **Future** |

## Priority Enhancements

### Phase 4: Core Feature Parity (High Priority)

**Branch:** `feature/core-enhancements`

#### 4.1 Scroll Direction Control
- Add `ScrollOrientation` enum (Vertical, Horizontal)
- Implement in Android (swipeHorizontal)
- Implement in iOS (DisplayDirection)
- Update MAUI control with bindable property

#### 4.2 Default Page
- Add `DefaultPage` property
- Allow setting initial page on load
- Both platforms support this

#### 4.3 Antialiasing & Quality
- Add `EnableAntialiasing` property (Android)
- Add `UseBestQuality` property (Android - ARGB_8888 vs RGB_565)
- iOS always uses high quality

#### 4.4 Background Color
- Add `BackgroundColor` property
- Set page background color
- Both platforms support

#### 4.5 Additional Events
- Add `Tapped` event (single tap)
- Add `Rendered` event (first render complete)
- Platform-specific: `PageScrolling` (Android), `LongPressed` (Android)

### Phase 5: Android-Specific Enhancements (Medium Priority)

**Branch:** `feature/android-enhancements`

#### 5.1 ViewPager-Style Navigation
- Add `PageSnap` property - snap to page boundaries
- Add `PageFling` property - fling changes single page
- Add `AutoSpacing` property - fit each page independently
- Add `FitEachPage` property - scale pages relative to largest

#### 5.2 Mid-Level Zoom
- Add `MidZoom` property
- Three-level zoom: Min → Mid → Max → Min
- Android only (iOS has continuous zoom)

#### 5.3 Night Mode
- Add `NightMode` property
- Inverts colors for reading in dark
- Android only (iOS doesn't have built-in support)

#### 5.4 Advanced Events
- Add `PageScrolling` event with offset info
- Add `LongPressed` event

#### 5.5 Password Support
- Add `Password` property
- Support encrypted PDFs

### Phase 6: iOS-Specific Enhancements (Medium Priority)

**Branch:** `feature/ios-enhancements`

#### 6.1 Display Modes
- Expand `DisplayMode` options
- SinglePage, SinglePageContinuous, TwoUp, TwoUpContinuous
- Update FitPolicy mapping

#### 6.2 Thumbnail Support
- Add thumbnail generation API
- PDFThumbnailView integration (optional)

#### 6.3 Document Properties
- Add `Creator` property
- Add `Keywords` property
- Add `CreationDate` property
- Add `ModificationDate` property

#### 6.4 Page Labels
- Add `GetPageLabel(int)` method
- Support custom page numbering

### Phase 7: Annotation Support (Low Priority)

**Branch:** `feature/annotations`

#### 7.1 Annotation Rendering
- Add `EnableAnnotationRendering` property
- Render PDF annotations, forms, comments
- Both platforms support

#### 7.2 Annotation Events
- Add `AnnotationTapped` event
- Get annotation details

### Phase 8: Advanced Features (Future)

**Branch:** `feature/advanced`

#### 8.1 Text Selection (iOS Primary)
- Text selection support
- Copy text functionality
- Selection events

#### 8.2 Search (iOS Primary)
- Add `Search(string)` method
- Highlight search results
- Navigation between results

#### 8.3 Custom Drawing
- `OnDraw` callback for custom overlays
- `OnDrawAll` callback for per-page drawing

#### 8.4 Scroll Handle
- Custom scroll indicator
- Page number display
- Android has built-in, iOS needs custom

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

### Phase 4 Complete When:
- ✅ Scroll orientation working both platforms
- ✅ Default page loads correctly
- ✅ Quality/antialiasing configurable
- ✅ Background color customizable
- ✅ Tapped and Rendered events firing
- ✅ All features documented
- ✅ Sample app demonstrates all features

### Overall Success:
- Maximum feature parity between platforms
- Clean, consistent API
- Well-documented
- Performant
- No breaking changes to existing code
- Ready for NuGet publication

## Timeline Estimate

- Phase 4 (Core Enhancements): 2-3 days
- Phase 5 (Android-Specific): 1-2 days
- Phase 6 (iOS-Specific): 1-2 days
- Phase 7 (Annotations): 1 day
- Phase 8 (Advanced): 3-5 days (if implemented)

Total for Phases 4-7: ~5-8 days of development

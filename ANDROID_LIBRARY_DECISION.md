# Android PDF Library Decision: AhmerPdfium

## Executive Summary

**Selected Library**: [AhmerPdfium](https://github.com/AhmerAfzal1/AhmerPdfium)  
**Versions**: 
- PdfViewer: v2.0.1 (`io.github.ahmerafzal1:ahmer-pdfviewer:2.0.1`)
- Pdfium Core: v1.9.2 (`io.github.ahmerafzal1:ahmer-pdfium:1.9.2`)

**Decision Date**: November 24, 2025  
**Reason**: Maintained fork with critical bug fixes and Android 15 compliance

---

## Comparison: AhmerPdfium vs AndroidPdfViewer

| Feature | AndroidPdfViewer (Original) | AhmerPdfium (Fork) |
|---------|----------------------------|-------------------|
| **Repository** | [DImuthuUpe/AndroidPdfViewer](https://github.com/DImuthuUpe/AndroidPdfViewer) | [AhmerAfzal1/AhmerPdfium](https://github.com/AhmerAfzal1/AhmerPdfium) |
| **Stars** | 8,400+ | 24 (newer fork) |
| **Version** | 3.2.0-beta.1 | 2.0.1 (viewer) + 1.9.2 (core) |
| **Language** | Java | Kotlin |
| **Distribution** | JitPack | Maven Central ✅ |
| **Min Android** | API 21 (Android 5.0) | API 24 (Android 7.0) |
| **License** | Apache 2.0 | Apache 2.0 |
| **First Page Bug** | ❌ Known issue | ✅ Fixed |
| **16 KB Pages** | ✅ Supported | ✅ Supported + Enhanced |
| **Active Maintenance** | ⚠️ Limited | ✅ Active (2024-2025) |
| **Kotlin Conversion** | ❌ Java only | ✅ Modern Kotlin |
| **Search Feature** | ❌ No | ✅ Yes (highlight WIP) |

---

## Why AhmerPdfium Over AndroidPdfViewer?

### 1. Critical Bug Fixes ✅

**First-Page Rendering Bug**
- **Original Issue**: When the first page has a small height, it renders incompletely
- **Impact**: Incorrect offset calculations, partial rendering, especially with page snap or after zooming
- **AhmerPdfium Fix**: Completely resolved in v2.0.1

This alone justifies the switch, as first-page rendering issues would significantly impact user experience.

### 2. Enhanced 16 KB Page Size Support 🎯

**Google Play Requirement** (Mandatory as of November 1, 2025):
- All new apps and updates targeting Android 15+ must support 16 KB page sizes
- Critical for modern Android devices using larger memory pages

**AhmerPdfium Enhancement**:
- Not just supported, but enhanced with better native library alignment
- Includes post-build realignment tools
- Passes all 16 KB compatibility checks
- Better integration with Android 15+ features

### 3. Maven Central Distribution 📦

**Original (AndroidPdfViewer)**:
```gradle
repositories {
    maven { url 'https://jitpack.io' }
}
dependencies {
    implementation 'com.github.barteksc:android-pdf-viewer:3.2.0-beta.1'
}
```

**AhmerPdfium**:
```gradle
repositories {
    mavenCentral()  // Standard repository
}
dependencies {
    implementation 'io.github.ahmerafzal1:ahmer-pdfviewer:2.0.1'
    implementation 'io.github.ahmerafzal1:ahmer-pdfium:1.9.2'
}
```

**Benefits**:
- More reliable package distribution
- Better for enterprise/corporate environments
- Standard Maven Central trust and security
- Easier for .NET binding tools to consume

### 4. Modern Kotlin Implementation 🔧

- Cleaner, more maintainable codebase
- Better null safety
- Coroutine support potential
- Modern Android development practices
- Easier to contribute to if needed

### 5. Active Maintenance 🔄

**Recent Activity**:
- Latest commits in 2024-2025
- Active issue responses
- Search feature implementation (in progress)
- Community engagement

**Original Library**:
- Less frequent updates
- More reliance on community forks
- Beta status maintained for extended period

### 6. Additional Features 🌟

**Search Functionality**:
- Implemented in AhmerPdfium (highlighting in progress)
- Not available in original
- Future enhancement for our library

**Better API 24+ Integration**:
- Takes advantage of newer Android APIs
- 99%+ device coverage as of 2025
- Cleaner implementation without legacy workarounds

---

## Technical Implementation Details

### Package Structure

```
AhmerPdfium Project
├── ahmer-pdfium (Core)
│   ├── Version: 1.9.2
│   ├── Purpose: Native PDF rendering engine
│   └── ProGuard: -keep class com.ahmer.pdfium.** { *; }
│
└── ahmer-pdfviewer (UI Component)
    ├── Version: 2.0.1
    ├── Purpose: Android View wrapper
    └── Depends on: ahmer-pdfium:1.9.2
```

### Maven Coordinates

```xml
<!-- Pdfium Core -->
<dependency>
    <groupId>io.github.ahmerafzal1</groupId>
    <artifactId>ahmer-pdfium</artifactId>
    <version>1.9.2</version>
</dependency>

<!-- PdfViewer UI -->
<dependency>
    <groupId>io.github.ahmerafzal1</groupId>
    <artifactId>ahmer-pdfviewer</artifactId>
    <version>2.0.1</version>
</dependency>
```

### .NET Binding Strategy

For .NET MAUI binding, we'll need to:

1. **Download AAR Files**:
   - `ahmer-pdfium-1.9.2.aar`
   - `ahmer-pdfviewer-2.0.1.aar`

2. **Create Binding Project**:
   ```bash
   dotnet new android-bindinglib -n MauiNativePdfView.Android.Binding
   ```

3. **Add AAR Files**:
   ```xml
   <ItemGroup>
       <AndroidLibrary Include="Jars\ahmer-pdfium-1.9.2.aar" />
       <AndroidLibrary Include="Jars\ahmer-pdfviewer-2.0.1.aar" />
   </ItemGroup>
   ```

4. **Configure Metadata** (if needed):
   - Handle any naming conflicts
   - Map Kotlin types to C#
   - Fix visibility issues

---

## API Comparison

### Basic Usage (Nearly Identical)

**AndroidPdfViewer (Original)**:
```java
PDFView pdfView = findViewById(R.id.pdfView);
pdfView.fromFile(file)
    .pages(0, 2, 1, 3, 3, 3)
    .enableSwipe(true)
    .swipeHorizontal(false)
    .enableDoubletap(true)
    .defaultPage(0)
    .onLoad(onLoadCompleteListener)
    .load();
```

**AhmerPdfium**:
```kotlin
val pdfView: PDFView = findViewById(R.id.pdfView)
pdfView.fromFile(file)
    .pages(0, 2, 1, 3, 3, 3)
    .enableSwipe(true)
    .swipeHorizontal(false)
    .enableDoubletap(true)
    .defaultPage(0)
    .onLoad(onLoadCompleteListener)
    .load()
```

**Key Difference**: Kotlin syntax, but API is compatible!

### Package Names

- **Original**: `com.github.barteksc.pdfviewer.*`
- **AhmerPdfium**: `com.ahmer.pdfviewer.*`

This means our .NET binding will have different namespaces, but the API surface is nearly identical.

---

## Migration Considerations

### If We Started with AndroidPdfViewer

If we had already started with the original library, migration would be straightforward:

1. **Update Dependencies**: Change Maven coordinates
2. **Update Package Imports**: `com.github.barteksc` → `com.ahmer`
3. **Update ProGuard Rules**: `com.shockwave` → `com.ahmer.pdfium`
4. **Test Thoroughly**: Verify no behavior changes

### Starting Fresh (Our Case)

Since we're starting fresh, we avoid migration entirely and benefit from:
- Latest bug fixes from day one
- Better Android 15+ compliance
- Modern Kotlin codebase
- Maven Central distribution

---

## Risk Assessment

### Risks: AhmerPdfium

| Risk | Severity | Mitigation |
|------|----------|------------|
| Smaller community (24 stars vs 8.4k) | Low | Built on proven AndroidPdfViewer base |
| Newer fork (less battle-tested) | Low | Core code is from mature library |
| Two separate packages | Low | Standard practice for core + UI separation |
| Kotlin learning curve | Very Low | Similar API, documentation available |
| Maintainer bus factor | Medium | Can fork if needed, have full source |

### Benefits Far Outweigh Risks

- ✅ Critical bug fixes
- ✅ Android 15 compliance
- ✅ Maven Central reliability
- ✅ Modern codebase
- ✅ Active maintenance

---

## Long-Term Strategy

### Monitoring

1. **Watch GitHub Repository**: Star and watch AhmerAfzal1/AhmerPdfium
2. **Maven Central**: Monitor for new releases
3. **Issues Tracking**: Follow issue discussions
4. **Community**: Engage with maintainer if needed

### Backup Plan

If AhmerPdfium becomes unmaintained:
1. We have complete source code
2. Can create our own fork
3. Can fall back to original AndroidPdfViewer
4. Minimal code changes needed due to API compatibility

### Contribution

If we find issues or need features:
- AhmerPdfium accepts PRs
- Active maintainer engagement
- Smaller codebase easier to contribute to

---

## ProGuard Configuration

Required ProGuard rules for release builds:

```proguard
# AhmerPdfium
-keep class com.ahmer.pdfium.** { *; }

# If you see warnings about missing classes, add:
-dontwarn com.ahmer.pdfium.**
```

Note: This differs from original library rules (`com.shockwave.**`)

---

## Device Compatibility

### Minimum Requirements
- **Android Version**: 7.0 (Nougat) - API 24
- **Market Share**: ~99% of active devices (as of 2025)
- **Reason for API 24**: Better Android features, cleaner implementation

### Original Library
- **Android Version**: 5.0 (Lollipop) - API 21
- **Market Share**: ~99.5% of active devices

**Impact**: Negligible - the 0.5% difference represents extremely old devices unlikely to run modern .NET MAUI apps anyway.

---

## Conclusion

**AhmerPdfium is the correct choice** for this project because:

1. ✅ **Fixes critical bugs** that affect user experience
2. ✅ **Future-proof** with 16 KB page size support
3. ✅ **Professional distribution** via Maven Central
4. ✅ **Modern codebase** with Kotlin
5. ✅ **Actively maintained** with recent updates
6. ✅ **Compatible API** with original library
7. ✅ **Low risk** - built on proven foundation

The decision to use AhmerPdfium over AndroidPdfViewer is justified by tangible improvements and future-proofing, while maintaining the mature codebase and API familiarity.

---

**Decision Approved**: November 24, 2025  
**Review Date**: Upon Phase 2 completion (Android implementation)  
**Revision**: If critical issues discovered, reassess

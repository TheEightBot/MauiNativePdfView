using Android.Content;
using Com.Ahmer.Pdfviewer;
using MauiNativePdfView.Abstractions;
using Microsoft.Maui.Handlers;

namespace MauiNativePdfView.Platforms.Android;

/// <summary>
/// Android handler for PdfView control.
/// Maps the cross-platform PdfView to the native Android implementation.
/// </summary>
public partial class PdfViewHandler : ViewHandler<PdfView, PDFView>
{
    private PdfViewAndroid? _pdfViewWrapper;

    /// <summary>
    /// Property mapper for PdfView properties.
    /// </summary>
    public static IPropertyMapper<PdfView, PdfViewHandler> Mapper = new PropertyMapper<PdfView, PdfViewHandler>(ViewMapper)
    {
        [nameof(PdfView.Source)] = MapSource,
        [nameof(PdfView.EnableZoom)] = MapEnableZoom,
        [nameof(PdfView.EnableSwipe)] = MapEnableSwipe,
        [nameof(PdfView.EnableTapGestures)] = MapEnableTapGestures,
        [nameof(PdfView.EnableLinkNavigation)] = MapEnableLinkNavigation,
        [nameof(PdfView.Zoom)] = MapZoom,
        [nameof(PdfView.MinZoom)] = MapMinZoom,
        [nameof(PdfView.MaxZoom)] = MapMaxZoom,
        [nameof(PdfView.PageSpacing)] = MapPageSpacing,
        [nameof(PdfView.FitPolicy)] = MapFitPolicy,
        [nameof(PdfView.DisplayMode)] = MapDisplayMode,
        [nameof(PdfView.ScrollOrientation)] = MapScrollOrientation,
        [nameof(PdfView.DefaultPage)] = MapDefaultPage,
        [nameof(PdfView.EnableAntialiasing)] = MapEnableAntialiasing,
        [nameof(PdfView.UseBestQuality)] = MapUseBestQuality,
        [nameof(PdfView.BackgroundColor)] = MapBackgroundColor,
        [nameof(PdfView.EnableAnnotationRendering)] = MapEnableAnnotationRendering,
    };

    /// <summary>
    /// Command mapper for PdfView commands.
    /// </summary>
    public static CommandMapper<PdfView, PdfViewHandler> CommandMapper = new(ViewCommandMapper)
    {
        [nameof(IPdfView.GoToPage)] = MapGoToPage,
        [nameof(IPdfView.Reload)] = MapReload,
    };

    public PdfViewHandler() : base(Mapper, CommandMapper)
    {
    }

    public PdfViewHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override PDFView CreatePlatformView()
    {
        var context = Context ?? throw new InvalidOperationException("Context is null");
        _pdfViewWrapper = new PdfViewAndroid(context);

        // Wire up events to forward to virtual view
        _pdfViewWrapper.DocumentLoaded += OnDocumentLoaded;
        _pdfViewWrapper.PageChanged += OnPageChanged;
        _pdfViewWrapper.Error += OnError;
        _pdfViewWrapper.LinkTapped += OnLinkTapped;
        _pdfViewWrapper.Tapped += OnTapped;
        _pdfViewWrapper.Rendered += OnRendered;

        return _pdfViewWrapper.NativeView;
    }

    protected override void ConnectHandler(PDFView platformView)
    {
        base.ConnectHandler(platformView);

        // Apply initial values
        MapSource(this, VirtualView);
        MapEnableZoom(this, VirtualView);
        MapEnableSwipe(this, VirtualView);
        MapEnableTapGestures(this, VirtualView);
        MapEnableLinkNavigation(this, VirtualView);
        MapMinZoom(this, VirtualView);
        MapMaxZoom(this, VirtualView);
        MapPageSpacing(this, VirtualView);
        MapFitPolicy(this, VirtualView);
        MapDisplayMode(this, VirtualView);
        MapScrollOrientation(this, VirtualView);
        MapDefaultPage(this, VirtualView);
        MapEnableAntialiasing(this, VirtualView);
        MapUseBestQuality(this, VirtualView);
        MapBackgroundColor(this, VirtualView);
    }

    protected override void DisconnectHandler(PDFView platformView)
    {
        if (_pdfViewWrapper != null)
        {
            _pdfViewWrapper.DocumentLoaded -= OnDocumentLoaded;
            _pdfViewWrapper.PageChanged -= OnPageChanged;
            _pdfViewWrapper.Error -= OnError;
            _pdfViewWrapper.LinkTapped -= OnLinkTapped;
            _pdfViewWrapper.Tapped -= OnTapped;
            _pdfViewWrapper.Rendered -= OnRendered;
            _pdfViewWrapper.Dispose();
            _pdfViewWrapper = null;
        }

        base.DisconnectHandler(platformView);
    }

    #region Event Handlers

    private void OnDocumentLoaded(object? sender, DocumentLoadedEventArgs e)
    {
        VirtualView?.RaiseDocumentLoaded(e);
    }

    private void OnPageChanged(object? sender, PageChangedEventArgs e)
    {
        VirtualView?.RaisePageChanged(e);
    }

    private void OnError(object? sender, PdfErrorEventArgs e)
    {
        VirtualView?.RaiseError(e);
    }

    private void OnLinkTapped(object? sender, LinkTappedEventArgs e)
    {
        VirtualView?.RaiseLinkTapped(e);
    }

    private void OnTapped(object? sender, PdfTappedEventArgs e)
    {
        VirtualView?.RaiseTapped(e);
    }

    private void OnRendered(object? sender, RenderedEventArgs e)
    {
        VirtualView?.RaiseRendered(e);
    }

    #endregion

    #region Property Mappers

    private static void MapSource(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.Source = view.Source;
        }
    }

    private static void MapEnableZoom(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.EnableZoom = view.EnableZoom;
        }
    }

    private static void MapEnableSwipe(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.EnableSwipe = view.EnableSwipe;
        }
    }

    private static void MapEnableTapGestures(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.EnableTapGestures = view.EnableTapGestures;
        }
    }

    private static void MapEnableLinkNavigation(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.EnableLinkNavigation = view.EnableLinkNavigation;
        }
    }

    private static void MapZoom(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.Zoom = view.Zoom;
        }
    }

    private static void MapMinZoom(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.MinZoom = view.MinZoom;
        }
    }

    private static void MapMaxZoom(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.MaxZoom = view.MaxZoom;
        }
    }

    private static void MapPageSpacing(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.PageSpacing = view.PageSpacing;
        }
    }

    private static void MapFitPolicy(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.FitPolicy = view.FitPolicy;
        }
    }

    private static void MapDisplayMode(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.DisplayMode = view.DisplayMode;
        }
    }

    private static void MapScrollOrientation(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.ScrollOrientation = view.ScrollOrientation;
        }
    }

    private static void MapDefaultPage(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.DefaultPage = view.DefaultPage;
        }
    }

    private static void MapEnableAntialiasing(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.EnableAntialiasing = view.EnableAntialiasing;
        }
    }

    private static void MapUseBestQuality(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.UseBestQuality = view.UseBestQuality;
        }
    }

    private static void MapBackgroundColor(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.BackgroundColor = view.BackgroundColor;
        }
    }

    private static void MapEnableAnnotationRendering(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.EnableAnnotationRendering = view.EnableAnnotationRendering;
        }
    }

    #endregion

    #region Command Mappers

    private static void MapGoToPage(PdfViewHandler handler, PdfView view, object? args)
    {
        if (handler._pdfViewWrapper != null && args is int pageIndex)
        {
            handler._pdfViewWrapper.GoToPage(pageIndex);
        }
    }

    private static void MapReload(PdfViewHandler handler, PdfView view, object? args)
    {
        handler._pdfViewWrapper?.Reload();
    }

    #endregion
}

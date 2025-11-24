using Microsoft.Maui.Handlers;
using MauiNativePdfView.Abstractions;
using PdfKit;

namespace MauiNativePdfView.Platforms.iOS;

/// <summary>
/// iOS handler for PdfView control using PdfKit.
/// </summary>
public partial class PdfViewHandler : ViewHandler<PdfView, PdfKit.PdfView>
{
    private PdfViewiOS? _pdfViewWrapper;

    public static IPropertyMapper<PdfView, PdfViewHandler> Mapper = new PropertyMapper<PdfView, PdfViewHandler>(ViewMapper)
    {
        [nameof(PdfView.Source)] = MapSource,
        [nameof(PdfView.EnableZoom)] = MapEnableZoom,
        [nameof(PdfView.EnableSwipe)] = MapEnableSwipe,
        [nameof(PdfView.EnableLinkNavigation)] = MapEnableLinkNavigation,
        [nameof(PdfView.Zoom)] = MapZoom,
        [nameof(PdfView.MinZoom)] = MapMinZoom,
        [nameof(PdfView.MaxZoom)] = MapMaxZoom,
        [nameof(PdfView.PageSpacing)] = MapPageSpacing,
        [nameof(PdfView.FitPolicy)] = MapFitPolicy
    };

    public static CommandMapper<PdfView, PdfViewHandler> CommandMapper = new(ViewCommandMapper)
    {
        [nameof(IPdfView.GoToPage)] = MapGoToPage,
        [nameof(IPdfView.Reload)] = MapReload
    };

    public PdfViewHandler() : base(Mapper, CommandMapper)
    {
    }

    public PdfViewHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override PdfKit.PdfView CreatePlatformView()
    {
        _pdfViewWrapper = new PdfViewiOS();

        // Subscribe to wrapper events and forward to MAUI control
        _pdfViewWrapper.DocumentLoaded += OnDocumentLoaded;
        _pdfViewWrapper.PageChanged += OnPageChanged;
        _pdfViewWrapper.Error += OnError;
        _pdfViewWrapper.LinkTapped += OnLinkTapped;

        return _pdfViewWrapper.NativeView;
    }

    protected override void ConnectHandler(PdfKit.PdfView platformView)
    {
        base.ConnectHandler(platformView);

        // Apply initial property values
        if (_pdfViewWrapper != null && VirtualView != null)
        {
            _pdfViewWrapper.Source = VirtualView.Source;
            _pdfViewWrapper.EnableZoom = VirtualView.EnableZoom;
            _pdfViewWrapper.EnableSwipe = VirtualView.EnableSwipe;
            _pdfViewWrapper.EnableLinkNavigation = VirtualView.EnableLinkNavigation;
            _pdfViewWrapper.Zoom = VirtualView.Zoom;
            _pdfViewWrapper.MinZoom = VirtualView.MinZoom;
            _pdfViewWrapper.MaxZoom = VirtualView.MaxZoom;
            _pdfViewWrapper.PageSpacing = VirtualView.PageSpacing;
            _pdfViewWrapper.FitPolicy = VirtualView.FitPolicy;
        }
    }

    protected override void DisconnectHandler(PdfKit.PdfView platformView)
    {
        if (_pdfViewWrapper != null)
        {
            _pdfViewWrapper.DocumentLoaded -= OnDocumentLoaded;
            _pdfViewWrapper.PageChanged -= OnPageChanged;
            _pdfViewWrapper.Error -= OnError;
            _pdfViewWrapper.LinkTapped -= OnLinkTapped;
            _pdfViewWrapper.Dispose();
            _pdfViewWrapper = null;
        }

        base.DisconnectHandler(platformView);
    }

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

    public static void MapSource(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.Source = view.Source;
        }
    }

    public static void MapEnableZoom(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.EnableZoom = view.EnableZoom;
        }
    }

    public static void MapEnableSwipe(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.EnableSwipe = view.EnableSwipe;
        }
    }

    public static void MapEnableLinkNavigation(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.EnableLinkNavigation = view.EnableLinkNavigation;
        }
    }

    public static void MapZoom(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.Zoom = view.Zoom;
        }
    }

    public static void MapMinZoom(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.MinZoom = view.MinZoom;
        }
    }

    public static void MapMaxZoom(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.MaxZoom = view.MaxZoom;
        }
    }

    public static void MapPageSpacing(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.PageSpacing = view.PageSpacing;
        }
    }

    public static void MapFitPolicy(PdfViewHandler handler, PdfView view)
    {
        if (handler._pdfViewWrapper != null)
        {
            handler._pdfViewWrapper.FitPolicy = view.FitPolicy;
        }
    }

    public static void MapGoToPage(PdfViewHandler handler, PdfView view, object? args)
    {
        if (handler._pdfViewWrapper != null && args is int pageIndex)
        {
            handler._pdfViewWrapper.GoToPage(pageIndex);
        }
    }

    public static void MapReload(PdfViewHandler handler, PdfView view, object? args)
    {
        handler._pdfViewWrapper?.Reload();
    }
}

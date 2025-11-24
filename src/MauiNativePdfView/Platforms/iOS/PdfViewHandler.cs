using Microsoft.Maui.Handlers;
using UIKit;

namespace MauiNativePdfView.Platforms.iOS;

/// <summary>
/// iOS handler for PdfView control.
/// TODO: Implement iOS PDFKit wrapper in Phase 3.
/// </summary>
public partial class PdfViewHandler : ViewHandler<PdfView, UIView>
{
    public static IPropertyMapper<PdfView, PdfViewHandler> Mapper = new PropertyMapper<PdfView, PdfViewHandler>(ViewMapper)
    {
        // TODO: Add property mappers
    };

    public static CommandMapper<PdfView, PdfViewHandler> CommandMapper = new(ViewCommandMapper)
    {
        // TODO: Add command mappers
    };

    public PdfViewHandler() : base(Mapper, CommandMapper)
    {
    }

    public PdfViewHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override UIView CreatePlatformView()
    {
        // TODO: Implement iOS PDFKit wrapper
        var label = new UILabel
        {
            Text = "iOS PDF viewer coming in Phase 3",
            TextAlignment = UITextAlignment.Center
        };
        return label;
    }
}

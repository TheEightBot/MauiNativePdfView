using Microsoft.Maui.Hosting;

namespace MauiNativePdfView;

/// <summary>
/// Extension methods for configuring MauiNativePdfView.
/// </summary>
public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Configures MauiNativePdfView with the application.
    /// </summary>
    public static MauiAppBuilder UseMauiNativePdfView(this MauiAppBuilder builder)
    {
        builder.ConfigureMauiHandlers(handlers =>
        {
#if ANDROID
            handlers.AddHandler<PdfView, Platforms.Android.PdfViewHandler>();
#elif IOS
            handlers.AddHandler<PdfView, Platforms.iOS.PdfViewHandler>();
#endif
        });

        return builder;
    }
}

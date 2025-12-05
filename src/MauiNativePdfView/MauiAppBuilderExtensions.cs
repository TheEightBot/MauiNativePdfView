using Microsoft.Maui.Hosting;

namespace MauiNativePdfView;

/// <summary>
/// Extension methods for configuring MauiNativePdfView.
/// </summary>
public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Ensures the MauiNativePdfView assembly is linked and available for XAML resolution.
    /// This method is called automatically by <see cref="UseMauiNativePdfView"/> but can be
    /// called explicitly if needed for custom namespace schema resolution.
    /// </summary>
    /// <remarks>
    /// This method ensures the linker preserves the assembly containing the custom XAML
    /// namespace types, preventing them from being trimmed during compilation.
    /// </remarks>
    public static void Init()
    {
        // This method ensures the assembly is referenced and not trimmed by the linker.
        // The actual initialization is handled by UseMauiNativePdfView.
    }

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

using System.ComponentModel;
using System.Globalization;

namespace MauiNativePdfView.Abstractions;

/// <summary>
/// TypeConverter for converting strings to PdfSource types in XAML.
/// </summary>
/// <remarks>
/// Supports the following string formats:
/// <list type="bullet">
/// <item><description>URLs (http:// or https://) → UriPdfSource</description></item>
/// <item><description>Asset paths (asset://) → AssetPdfSource</description></item>
/// <item><description>File URIs (file://) → FilePdfSource</description></item>
/// <item><description>Simple filenames (no path separators) → AssetPdfSource</description></item>
/// <item><description>Other strings → FilePdfSource</description></item>
/// </list>
/// </remarks>
public class PdfSourceTypeConverter : TypeConverter
{
    /// <summary>
    /// Returns whether this converter can convert from the specified type.
    /// </summary>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string) || sourceType == typeof(Uri) || base.CanConvertFrom(context, sourceType);

    /// <summary>
    /// Returns whether this converter can convert to the specified type.
    /// </summary>
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    /// <summary>
    /// Converts from a string or Uri to a PdfSource.
    /// </summary>
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is Uri uri)
        {
            return new UriPdfSource(uri);
        }

        if (value is string stringValue)
        {
            return ParseString(stringValue);
        }

        return base.ConvertFrom(context, culture, value);
    }

    /// <summary>
    /// Converts a PdfSource to a string representation.
    /// </summary>
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string))
        {
            return value switch
            {
                UriPdfSource uriSource => uriSource.Uri.ToString(),
                FilePdfSource fileSource => fileSource.FilePath,
                AssetPdfSource assetSource => $"asset://{assetSource.AssetName}",
                _ => null
            };
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    /// <summary>
    /// Converts a string to the appropriate PdfSource type based on the string format.
    /// </summary>
    private static PdfSource ParseString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Cannot convert empty or null string to {nameof(PdfSource)}");
        }

        var trimmedValue = value.Trim();

        // Check for HTTP/HTTPS URLs
        if (trimmedValue.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            trimmedValue.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return new UriPdfSource(new Uri(trimmedValue));
        }

        // Check for asset:// prefix (custom scheme for embedded assets)
        if (trimmedValue.StartsWith("asset://", StringComparison.OrdinalIgnoreCase))
        {
            var assetName = trimmedValue["asset://".Length..];
            return new AssetPdfSource(assetName);
        }

        // Check for file:// URI scheme
        if (trimmedValue.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(trimmedValue);
            return new FilePdfSource(uri.LocalPath);
        }

        // Default: treat as a file path or asset name
        // If it looks like a relative path without extension or with common asset extensions,
        // and doesn't contain path separators at the start, treat as asset
        if (!trimmedValue.StartsWith("/") && 
            !trimmedValue.StartsWith("\\") &&
            !trimmedValue.Contains(":/") &&
            !trimmedValue.Contains(":\\") &&
            (trimmedValue.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ||
             !trimmedValue.Contains(Path.DirectorySeparatorChar) &&
             !trimmedValue.Contains(Path.AltDirectorySeparatorChar)))
        {
            // Could be an asset name like "sample.pdf" or a simple filename
            // We'll treat simple names as assets, full paths as files
            if (!Path.IsPathRooted(trimmedValue) && 
                !trimmedValue.Contains(Path.DirectorySeparatorChar) &&
                !trimmedValue.Contains(Path.AltDirectorySeparatorChar))
            {
                return new AssetPdfSource(trimmedValue);
            }
        }

        // Treat as file path
        return new FilePdfSource(trimmedValue);
    }
}

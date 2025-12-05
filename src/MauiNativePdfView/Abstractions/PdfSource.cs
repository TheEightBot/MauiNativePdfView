using System.ComponentModel;

namespace MauiNativePdfView.Abstractions;

/// <summary>
/// Base class for PDF source types.
/// </summary>
/// <remarks>
/// Supports implicit conversion from strings and URIs for convenient XAML usage.
/// String conversion follows these rules:
/// <list type="bullet">
/// <item><description>URLs (http:// or https://) → UriPdfSource</description></item>
/// <item><description>Asset paths (asset://) → AssetPdfSource</description></item>
/// <item><description>File URIs (file://) → FilePdfSource</description></item>
/// <item><description>Simple filenames → AssetPdfSource</description></item>
/// <item><description>Full paths → FilePdfSource</description></item>
/// </list>
/// </remarks>
[TypeConverter(typeof(PdfSourceTypeConverter))]
public abstract class PdfSource
{
    /// <summary>
    /// Gets or sets the password for encrypted PDF documents.
    /// Leave null or empty for non-encrypted PDFs.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Implicitly converts a string to a PdfSource.
    /// </summary>
    /// <param name="source">The source string (URL, asset path, or file path).</param>
    public static implicit operator PdfSource?(string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
            return null;

        var trimmedValue = source.Trim();

        // HTTP/HTTPS URLs
        if (trimmedValue.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            trimmedValue.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return new UriPdfSource(new Uri(trimmedValue));
        }

        // Asset prefix
        if (trimmedValue.StartsWith("asset://", StringComparison.OrdinalIgnoreCase))
        {
            return new AssetPdfSource(trimmedValue["asset://".Length..]);
        }

        // File URI
        if (trimmedValue.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
        {
            return new FilePdfSource(new Uri(trimmedValue).LocalPath);
        }

        // Simple filename (no path separators) → treat as asset
        if (!Path.IsPathRooted(trimmedValue) &&
            !trimmedValue.Contains(Path.DirectorySeparatorChar) &&
            !trimmedValue.Contains(Path.AltDirectorySeparatorChar))
        {
            return new AssetPdfSource(trimmedValue);
        }

        // Default to file path
        return new FilePdfSource(trimmedValue);
    }

    /// <summary>
    /// Implicitly converts a Uri to a PdfSource.
    /// </summary>
    /// <param name="uri">The URI to convert.</param>
    public static implicit operator PdfSource?(Uri? uri)
    {
        if (uri == null)
            return null;

        return new UriPdfSource(uri);
    }

    /// <summary>
    /// Creates a PDF source from a file path.
    /// </summary>
    public static PdfSource FromFile(string filePath)
        => new FilePdfSource(filePath);

    /// <summary>
    /// Creates a PDF source from a URI.
    /// </summary>
    public static PdfSource FromUri(Uri uri)
        => new UriPdfSource(uri);

    /// <summary>
    /// Creates a PDF source from a stream.
    /// </summary>
    public static PdfSource FromStream(Stream stream)
        => new StreamPdfSource(stream);

    /// <summary>
    /// Creates a PDF source from a byte array.
    /// </summary>
    public static PdfSource FromBytes(byte[] data)
        => new BytesPdfSource(data);

    /// <summary>
    /// Creates a PDF source from an embedded resource.
    /// </summary>
    public static PdfSource FromAsset(string assetName)
        => new AssetPdfSource(assetName);
}

/// <summary>
/// PDF source from a file path.
/// </summary>
public sealed class FilePdfSource : PdfSource
{
    public string FilePath { get; }

    public FilePdfSource(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        FilePath = filePath;
    }
}

/// <summary>
/// PDF source from a URI (URL or local file URI).
/// </summary>
public sealed class UriPdfSource : PdfSource
{
    public Uri Uri { get; }

    public UriPdfSource(Uri uri)
    {
        Uri = uri ?? throw new ArgumentNullException(nameof(uri));
    }
}

/// <summary>
/// PDF source from a stream.
/// </summary>
public sealed class StreamPdfSource : PdfSource
{
    public Stream Stream { get; }

    public StreamPdfSource(Stream stream)
    {
        Stream = stream ?? throw new ArgumentNullException(nameof(stream));
    }
}

/// <summary>
/// PDF source from a byte array.
/// </summary>
public sealed class BytesPdfSource : PdfSource
{
    public byte[] Data { get; }

    public BytesPdfSource(byte[] data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        if (data.Length == 0)
            throw new ArgumentException("Data cannot be empty.", nameof(data));
    }
}

/// <summary>
/// PDF source from an embedded asset/resource.
/// </summary>
public sealed class AssetPdfSource : PdfSource
{
    public string AssetName { get; }

    public AssetPdfSource(string assetName)
    {
        if (string.IsNullOrWhiteSpace(assetName))
            throw new ArgumentException("Asset name cannot be null or empty.", nameof(assetName));

        AssetName = assetName;
    }
}

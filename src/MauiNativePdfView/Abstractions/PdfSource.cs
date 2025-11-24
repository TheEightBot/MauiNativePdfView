namespace MauiNativePdfView.Abstractions;

/// <summary>
/// Base class for PDF source types.
/// </summary>
public abstract class PdfSource
{
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

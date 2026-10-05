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
/// <remarks>
/// The view reads the stream again whenever it reloads the document — a setting change,
/// <see cref="IPdfView.Reload"/>, or on Android returning to a page another one covered.
/// A seekable stream is re-read from where it stood on the first load, so keep it open for
/// as long as the view shows it; a forward-only stream is buffered in memory on the first
/// load instead. The view never closes the stream: disposing it stays with the caller.
/// </remarks>
public sealed class StreamPdfSource : PdfSource
{
    private readonly object _gate = new();
    private long? _startPosition;
    private byte[]? _buffer;

    public Stream Stream { get; }

    public StreamPdfSource(Stream stream)
    {
        Stream = stream ?? throw new ArgumentNullException(nameof(stream));
    }

    /// <summary>
    /// Opens the document from its first byte, however many times the view needs to load it.
    /// </summary>
    /// <remarks>
    /// Each call returns a fresh stream the caller owns and may dispose without touching
    /// <see cref="Stream"/>. That matters on Android: AhmerPdfViewer closes whatever stream
    /// it is handed once it has read it, and reads it on a background thread — so handing it
    /// <see cref="Stream"/> itself would dispose the caller's stream on the first load, and a
    /// reload could seek it out from under a read still in flight.
    ///
    /// A seekable stream is shared rather than copied, so a large document isn't held in
    /// memory twice; only a forward-only stream, which can't be read a second time, is.
    /// </remarks>
    internal Stream OpenDocument()
    {
        lock (_gate)
        {
            if (_buffer != null)
                return new MemoryStream(_buffer, writable: false);

            if (_startPosition is { } start)
            {
                // A disposed stream reports CanSeek false. Say why the reload failed, rather
                // than let it surface as an ObjectDisposedException from the native loader.
                if (!Stream.CanSeek)
                    throw new InvalidOperationException(
                        "The stream behind this StreamPdfSource was closed after the document first loaded, " +
                        "so the document can't be reloaded. Keep the stream open for as long as the view shows it, " +
                        "or use PdfSource.FromBytes.");

                return new SharedStreamReader(Stream, start, _gate);
            }

            if (Stream.CanSeek)
            {
                _startPosition = Stream.Position;
                return new SharedStreamReader(Stream, _startPosition.Value, _gate);
            }

            using var copy = new MemoryStream();
            Stream.CopyTo(copy);
            _buffer = copy.ToArray();
            return new MemoryStream(_buffer, writable: false);
        }
    }

    /// <summary>
    /// A read-only view onto the shared source stream that keeps its own cursor, seeking the
    /// source to it under the source's lock on every read. Two loads in flight at once — a
    /// reload racing a cancelled decode that is still reading — each see the whole document.
    /// Disposing it leaves the source stream open.
    /// </summary>
    private sealed class SharedStreamReader : Stream
    {
        private readonly Stream _source;
        private readonly long _start;
        private readonly object _gate;
        private long _position;

        public SharedStreamReader(Stream source, long start, object gate)
        {
            _source = source;
            _start = start;
            _gate = gate;
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;

        public override long Length
        {
            get
            {
                lock (_gate)
                    return Math.Max(_source.Length - _start, 0);
            }
        }

        public override long Position
        {
            get => _position;
            set => _position = value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));
        }

        public override int Read(byte[] buffer, int offset, int count)
            => Read(buffer.AsSpan(offset, count));

        public override int Read(Span<byte> buffer)
        {
            lock (_gate)
            {
                _source.Position = _start + _position;
                int read = _source.Read(buffer);
                _position += read;
                return read;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            Position = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => Length + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin)),
            };
            return _position;
        }

        public override void Flush()
        {
        }

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
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

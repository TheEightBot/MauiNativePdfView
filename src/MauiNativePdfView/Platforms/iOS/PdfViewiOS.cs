using MauiNativePdfView.Abstractions;
using PDFKit;
using UIKit;
using Foundation;

namespace MauiNativePdfView.Platforms.iOS;

/// <summary>
/// iOS implementation of IPdfView using PDFKit's PDFView.
/// </summary>
public class PdfViewiOS : IPdfView, IDisposable
{
    private readonly PDFView _pdfView;
    private PdfSource? _source;
    private bool _disposed;
    private NSObject? _pageChangedObserver;

    public PdfViewiOS()
    {
        _pdfView = new PDFView
        {
            AutoScales = true,
            DisplayMode = PDFDisplayMode.SinglePageContinuous,
            DisplayDirection = PDFDisplayDirection.Vertical
        };

        // Subscribe to page change notifications
        _pageChangedObserver = NSNotificationCenter.DefaultCenter.AddObserver(
            PDFView.PageChangedNotification,
            OnPageChangedNotification,
            _pdfView);
    }

    /// <summary>
    /// Gets the native PDFView instance.
    /// </summary>
    public PDFView NativeView => _pdfView;

    public PdfSource? Source
    {
        get => _source;
        set
        {
            _source = value;
            LoadDocument();
        }
    }

    public int CurrentPage => _pdfView.Document != null && _pdfView.CurrentPage != null
        ? (int)_pdfView.Document.GetIndexForPage(_pdfView.CurrentPage)
        : 0;

    public int PageCount => _pdfView.Document?.PageCount ?? 0;

    public bool EnableZoom
    {
        get => _pdfView.MinScaleFactor < _pdfView.MaxScaleFactor;
        set
        {
            if (value)
            {
                _pdfView.MinScaleFactor = MinZoom;
                _pdfView.MaxScaleFactor = MaxZoom;
            }
            else
            {
                var currentScale = _pdfView.ScaleFactor;
                _pdfView.MinScaleFactor = currentScale;
                _pdfView.MaxScaleFactor = currentScale;
            }
        }
    }

    public bool EnableSwipe { get; set; } = true;

    public bool EnableLinkNavigation
    {
        get => _pdfView.EnableDataDetectors;
        set => _pdfView.EnableDataDetectors = value;
    }

    public float Zoom
    {
        get => (float)_pdfView.ScaleFactor;
        set => _pdfView.ScaleFactor = value;
    }

    public float MinZoom { get; set; } = 1.0f;

    public float MaxZoom { get; set; } = 3.0f;

    public int PageSpacing
    {
        get => (int)_pdfView.PageBreakMargins.Top;
        set
        {
            _pdfView.PageBreakMargins = new UIEdgeInsets(value, value, value, value);
        }
    }

    public FitPolicy FitPolicy
    {
        get => _pdfView.AutoScales ? FitPolicy.Width : FitPolicy.Both;
        set
        {
            switch (value)
            {
                case FitPolicy.Width:
                    _pdfView.AutoScales = true;
                    _pdfView.DisplayMode = PDFDisplayMode.SinglePageContinuous;
                    break;
                case FitPolicy.Height:
                    _pdfView.AutoScales = true;
                    _pdfView.DisplayMode = PDFDisplayMode.SinglePage;
                    break;
                case FitPolicy.Both:
                    _pdfView.AutoScales = false;
                    break;
            }
        }
    }

    public event EventHandler<DocumentLoadedEventArgs>? DocumentLoaded;
    public event EventHandler<PageChangedEventArgs>? PageChanged;
    public event EventHandler<PdfErrorEventArgs>? Error;
    public event EventHandler<LinkTappedEventArgs>? LinkTapped;

    public void GoToPage(int pageIndex)
    {
        if (_pdfView.Document == null)
            return;

        if (pageIndex < 0 || pageIndex >= PageCount)
            return;

        var page = _pdfView.Document.GetPage((nuint)pageIndex);
        if (page != null)
        {
            _pdfView.GoToPage(page);
        }
    }

    public void Reload()
    {
        LoadDocument();
    }

    private void LoadDocument()
    {
        if (_source == null)
        {
            _pdfView.Document = null;
            return;
        }

        try
        {
            PDFDocument? document = null;

            switch (_source)
            {
                case FilePdfSource fileSource:
                    var fileUrl = NSUrl.FromFilename(fileSource.FilePath);
                    document = new PDFDocument(fileUrl);
                    break;

                case UriPdfSource uriSource:
                    var url = new NSUrl(uriSource.Uri.AbsoluteUri);
                    document = new PDFDocument(url);
                    break;

                case StreamPdfSource streamSource:
                    using (var memoryStream = new MemoryStream())
                    {
                        streamSource.Stream.CopyTo(memoryStream);
                        var data = NSData.FromArray(memoryStream.ToArray());
                        document = new PDFDocument(data);
                    }
                    break;

                case BytesPdfSource bytesSource:
                    var bytesData = NSData.FromArray(bytesSource.Data);
                    document = new PDFDocument(bytesData);
                    break;

                case AssetPdfSource assetSource:
                    var assetPath = Path.Combine(NSBundle.MainBundle.BundlePath, assetSource.AssetName);
                    if (File.Exists(assetPath))
                    {
                        var assetUrl = NSUrl.FromFilename(assetPath);
                        document = new PDFDocument(assetUrl);
                    }
                    else
                    {
                        // Try Resources folder
                        var resourcePath = NSBundle.MainBundle.PathForResource(
                            Path.GetFileNameWithoutExtension(assetSource.AssetName),
                            Path.GetExtension(assetSource.AssetName));
                        
                        if (!string.IsNullOrEmpty(resourcePath))
                        {
                            var resourceUrl = NSUrl.FromFilename(resourcePath);
                            document = new PDFDocument(resourceUrl);
                        }
                    }
                    break;
            }

            if (document != null)
            {
                _pdfView.Document = document;

                // Get document metadata
                var pageCount = document.PageCount;
                var title = document.DocumentAttributes?[PDFDocumentAttribute.TitleAttribute]?.ToString();
                var author = document.DocumentAttributes?[PDFDocumentAttribute.AuthorAttribute]?.ToString();
                var subject = document.DocumentAttributes?[PDFDocumentAttribute.SubjectAttribute]?.ToString();

                DocumentLoaded?.Invoke(this, new DocumentLoadedEventArgs(
                    (int)pageCount,
                    title,
                    author,
                    subject));

                // Trigger initial page changed event
                PageChanged?.Invoke(this, new PageChangedEventArgs(0, (int)pageCount));
            }
            else
            {
                Error?.Invoke(this, new PdfErrorEventArgs("Failed to load PDF document", null));
            }
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new PdfErrorEventArgs($"Error loading PDF: {ex.Message}", ex));
        }
    }

    private void OnPageChangedNotification(NSNotification notification)
    {
        if (_pdfView.Document != null && _pdfView.CurrentPage != null)
        {
            var pageIndex = (int)_pdfView.Document.GetIndexForPage(_pdfView.CurrentPage);
            var pageCount = (int)_pdfView.Document.PageCount;

            PageChanged?.Invoke(this, new PageChangedEventArgs(pageIndex, pageCount));
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_pageChangedObserver != null)
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(_pageChangedObserver);
            _pageChangedObserver?.Dispose();
            _pageChangedObserver = null;
        }

        _pdfView?.Dispose();
    }
}

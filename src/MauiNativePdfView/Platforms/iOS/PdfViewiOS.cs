using MauiNativePdfView.Abstractions;
using PdfKit;
using UIKit;
using Foundation;
using CoreImage;

namespace MauiNativePdfView.Platforms.iOS;

/// <summary>
/// iOS implementation of IPdfView using PdfKit's PdfView.
/// </summary>
public class PdfViewiOS : IPdfView, IDisposable
{
    private readonly PdfKit.PdfView _pdfView;
    private PdfSource? _source;
    private bool _disposed;
    private NSObject? _pageChangedObserver;
    private UITapGestureRecognizer? _tapGestureRecognizer;
    private PdfScrollOrientation _scrollOrientation = PdfScrollOrientation.Vertical;
    private int _defaultPage = 0;
    private bool _documentLoaded = false;

    public PdfViewiOS()
    {
        _pdfView = new PdfKit.PdfView
        {
            AutoScales = true,
            DisplayMode = PdfKit.PdfDisplayMode.SinglePageContinuous,
            DisplayDirection = PdfDisplayDirection.Vertical
        };

        // Subscribe to page change notifications
        _pageChangedObserver = NSNotificationCenter.DefaultCenter.AddObserver(
            PdfKit.PdfView.PageChangedNotification,
            OnPageChangedNotification,
            _pdfView);

        // Add tap gesture recognizer
        _tapGestureRecognizer = new UITapGestureRecognizer(HandleTap);
        _pdfView.AddGestureRecognizer(_tapGestureRecognizer);
    }

    /// <summary>
    /// Gets the native PdfView instance.
    /// </summary>
    public PdfKit.PdfView NativeView => _pdfView;

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
        ? (int)_pdfView.Document.GetPageIndex(_pdfView.CurrentPage)
        : 0;

    public int PageCount => _pdfView.Document != null ? (int)_pdfView.Document.PageCount : 0;

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
                    _pdfView.DisplayMode = PdfKit.PdfDisplayMode.SinglePageContinuous;
                    break;
                case FitPolicy.Height:
                    _pdfView.AutoScales = true;
                    _pdfView.DisplayMode = PdfKit.PdfDisplayMode.SinglePage;
                    break;
                case FitPolicy.Both:
                    _pdfView.AutoScales = false;
                    break;
            }
        }
    }

    public Abstractions.PdfDisplayMode DisplayMode
    {
        get
        {
            return _pdfView.DisplayMode switch
            {
                PdfKit.PdfDisplayMode.SinglePage => Abstractions.PdfDisplayMode.SinglePage,
                PdfKit.PdfDisplayMode.SinglePageContinuous => Abstractions.PdfDisplayMode.SinglePageContinuous,
                PdfKit.PdfDisplayMode.TwoUp => Abstractions.PdfDisplayMode.TwoUp,
                PdfKit.PdfDisplayMode.TwoUpContinuous => Abstractions.PdfDisplayMode.TwoUpContinuous,
                _ => Abstractions.PdfDisplayMode.SinglePageContinuous
            };
        }
        set
        {
            _pdfView.DisplayMode = value switch
            {
                Abstractions.PdfDisplayMode.SinglePage => PdfKit.PdfDisplayMode.SinglePage,
                Abstractions.PdfDisplayMode.SinglePageContinuous => PdfKit.PdfDisplayMode.SinglePageContinuous,
                Abstractions.PdfDisplayMode.TwoUp => PdfKit.PdfDisplayMode.TwoUp,
                Abstractions.PdfDisplayMode.TwoUpContinuous => PdfKit.PdfDisplayMode.TwoUpContinuous,
                _ => PdfKit.PdfDisplayMode.SinglePageContinuous
            };
        }
    }

    public PdfScrollOrientation ScrollOrientation
    {
        get => _scrollOrientation;
        set
        {
            _scrollOrientation = value;
            _pdfView.DisplayDirection = value == PdfScrollOrientation.Horizontal
                ? PdfDisplayDirection.Horizontal
                : PdfDisplayDirection.Vertical;
        }
    }

    public int DefaultPage
    {
        get => _defaultPage;
        set => _defaultPage = value;
    }

    public bool EnableAntialiasing
    {
        get => true; // iOS always uses antialiasing
        set { } // No-op on iOS
    }

    public bool UseBestQuality
    {
        get => true; // iOS always uses best quality
        set { } // No-op on iOS
    }

    public Color? BackgroundColor
    {
        get => _pdfView.BackgroundColor != null
            ? Color.FromRgba(
                _pdfView.BackgroundColor.CGColor.Components[0],
                _pdfView.BackgroundColor.CGColor.Components[1],
                _pdfView.BackgroundColor.CGColor.Components[2],
                _pdfView.BackgroundColor.CGColor.Alpha)
            : null;
        set
        {
            if (value != null)
            {
                _pdfView.BackgroundColor = UIColor.FromRGBA(
                    (float)value.Red,
                    (float)value.Green,
                    (float)value.Blue,
                    (float)value.Alpha);
            }
        }
    }

    private bool _nightMode = false;

    public bool NightMode
    {
        get => _nightMode;
        set
        {
            if (_nightMode != value)
            {
                _nightMode = value;
                ApplyNightMode();
            }
        }
    }

    private void ApplyNightMode()
    {
        // POC: Apply Core Image color invert filter to invert PDF colors
        // This creates a dark mode effect by inverting the document colors
        // NOTE: This may impact performance, especially on older devices
        if (_nightMode)
        {
            // Create a CIColorInvert filter
            var filter = new CIColorInvert();
            
            // Apply the filter to the PDF view
            // This inverts all colors, making white backgrounds black and black text white
            _pdfView.Layer.Filters = new CIFilter[] { filter };
        }
        else
        {
            // Remove filters to restore normal rendering
            _pdfView.Layer.Filters = null;
        }
    }

    public event EventHandler<DocumentLoadedEventArgs>? DocumentLoaded;
    public event EventHandler<PageChangedEventArgs>? PageChanged;
    public event EventHandler<PdfErrorEventArgs>? Error;
    public event EventHandler<LinkTappedEventArgs>? LinkTapped;
    public event EventHandler<PdfTappedEventArgs>? Tapped;
    public event EventHandler<RenderedEventArgs>? Rendered;

    public void GoToPage(int pageIndex)
    {
        if (_pdfView.Document == null)
            return;

        if (pageIndex < 0 || pageIndex >= PageCount)
            return;

        var page = _pdfView.Document.GetPage((nint)pageIndex);
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
            PdfDocument? document = null;

            switch (_source)
            {
                case FilePdfSource fileSource:
                    var fileUrl = NSUrl.FromFilename(fileSource.FilePath);
                    document = new PdfDocument(fileUrl);
                    break;

                case UriPdfSource uriSource:
                    var url = new NSUrl(uriSource.Uri.AbsoluteUri);
                    document = new PdfDocument(url);
                    break;

                case StreamPdfSource streamSource:
                    using (var memoryStream = new MemoryStream())
                    {
                        streamSource.Stream.CopyTo(memoryStream);
                        var data = NSData.FromArray(memoryStream.ToArray());
                        document = new PdfDocument(data);
                    }
                    break;

                case BytesPdfSource bytesSource:
                    var bytesData = NSData.FromArray(bytesSource.Data);
                    document = new PdfDocument(bytesData);
                    break;

                case AssetPdfSource assetSource:
                    var assetPath = Path.Combine(NSBundle.MainBundle.BundlePath, assetSource.AssetName);
                    if (File.Exists(assetPath))
                    {
                        var assetUrl = NSUrl.FromFilename(assetPath);
                        document = new PdfDocument(assetUrl);
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
                            document = new PdfDocument(resourceUrl);
                        }
                    }
                    break;
            }

            if (document != null)
            {
                _pdfView.Document = document;

                // Get document metadata
                var pageCount = (int)document.PageCount;
                var title = document.DocumentAttributes?["Title"]?.ToString();
                var author = document.DocumentAttributes?["Author"]?.ToString();
                var subject = document.DocumentAttributes?["Subject"]?.ToString();

                DocumentLoaded?.Invoke(this, new DocumentLoadedEventArgs(
                    pageCount,
                    title,
                    author,
                    subject));

                // Navigate to default page if specified
                if (_defaultPage > 0 && _defaultPage < pageCount)
                {
                    var page = document.GetPage((nint)_defaultPage);
                    if (page != null)
                    {
                        _pdfView.GoToPage(page);
                    }
                }

                // Trigger initial page changed event
                var currentPageIndex = _defaultPage > 0 && _defaultPage < pageCount ? _defaultPage : 0;
                PageChanged?.Invoke(this, new PageChangedEventArgs(currentPageIndex, pageCount));

                // Fire rendered event after a short delay to ensure rendering is complete
                if (!_documentLoaded)
                {
                    _documentLoaded = true;
                    System.Threading.Tasks.Task.Delay(100).ContinueWith(_ =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            Rendered?.Invoke(this, new RenderedEventArgs(pageCount));
                        });
                    });
                }
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
            var pageIndex = (int)_pdfView.Document.GetPageIndex(_pdfView.CurrentPage);
            var pageCount = (int)_pdfView.Document.PageCount;

            PageChanged?.Invoke(this, new PageChangedEventArgs(pageIndex, pageCount));
        }
    }

    private void HandleTap(UITapGestureRecognizer recognizer)
    {
        var location = recognizer.LocationInView(_pdfView);
        var pageIndex = CurrentPage;
        
        // Convert location to page coordinates
        var page = _pdfView.CurrentPage;
        if (page != null)
        {
            var pagePoint = _pdfView.ConvertPointToPage(location, page);
            Tapped?.Invoke(this, new PdfTappedEventArgs(pageIndex, (float)pagePoint.X, (float)pagePoint.Y));
        }
        else
        {
            Tapped?.Invoke(this, new PdfTappedEventArgs(pageIndex, (float)location.X, (float)location.Y));
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

        if (_tapGestureRecognizer != null)
        {
            _pdfView.RemoveGestureRecognizer(_tapGestureRecognizer);
            _tapGestureRecognizer?.Dispose();
            _tapGestureRecognizer = null;
        }

        _pdfView?.Dispose();
    }
}

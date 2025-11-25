namespace MauiNativePdfView.Abstractions;

/// <summary>
/// Event arguments for document loaded event.
/// </summary>
public class DocumentLoadedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the total number of pages in the document.
    /// </summary>
    public int PageCount { get; }

    /// <summary>
    /// Gets the document title, if available.
    /// </summary>
    public string? Title { get; }

    /// <summary>
    /// Gets the document author, if available.
    /// </summary>
    public string? Author { get; }

    /// <summary>
    /// Gets the document subject, if available.
    /// </summary>
    public string? Subject { get; }

    public DocumentLoadedEventArgs(int pageCount, string? title = null, string? author = null, string? subject = null)
    {
        PageCount = pageCount;
        Title = title;
        Author = author;
        Subject = subject;
    }
}

/// <summary>
/// Event arguments for page changed event.
/// </summary>
public class PageChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the current page index (0-based).
    /// </summary>
    public int PageIndex { get; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int PageCount { get; }

    public PageChangedEventArgs(int pageIndex, int pageCount)
    {
        PageIndex = pageIndex;
        PageCount = pageCount;
    }
}

/// <summary>
/// Event arguments for PDF error event.
/// </summary>
public class PdfErrorEventArgs : EventArgs
{
    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the exception, if available.
    /// </summary>
    public Exception? Exception { get; }

    public PdfErrorEventArgs(string message, Exception? exception = null)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Exception = exception;
    }
}

/// <summary>
/// Event arguments for link tapped event.
/// </summary>
public class LinkTappedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the link URI.
    /// </summary>
    public string? Uri { get; }

    /// <summary>
    /// Gets the destination page index for internal links (0-based).
    /// </summary>
    public int? DestinationPage { get; }

    /// <summary>
    /// Gets or sets whether the link navigation should be handled by the system.
    /// Set to false to prevent default link handling.
    /// </summary>
    public bool Handled { get; set; }

    public LinkTappedEventArgs(string? uri, int? destinationPage = null)
    {
        Uri = uri;
        DestinationPage = destinationPage;
    }
}

/// <summary>
/// Event arguments for PDF tapped event.
/// </summary>
public class PdfTappedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the page index where the tap occurred (0-based).
    /// </summary>
    public int PageIndex { get; }

    /// <summary>
    /// Gets the X coordinate of the tap (relative to page).
    /// </summary>
    public float X { get; }

    /// <summary>
    /// Gets the Y coordinate of the tap (relative to page).
    /// </summary>
    public float Y { get; }

    public PdfTappedEventArgs(int pageIndex, float x, float y)
    {
        PageIndex = pageIndex;
        X = x;
        Y = y;
    }
}

/// <summary>
/// Event arguments for PDF rendered event.
/// </summary>
public class RenderedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the total number of pages that were rendered.
    /// </summary>
    public int PageCount { get; }

    public RenderedEventArgs(int pageCount)
    {
        PageCount = pageCount;
    }
}

/// <summary>
/// Event arguments for annotation tapped event.
/// </summary>
public class AnnotationTappedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the page index where the annotation is located (0-based).
    /// </summary>
    public int PageIndex { get; }

    /// <summary>
    /// Gets the annotation type (e.g., "Text", "Highlight", "Ink", "FreeText").
    /// May be null if the type cannot be determined.
    /// </summary>
    public string? AnnotationType { get; }

    /// <summary>
    /// Gets the text contents of the annotation.
    /// May be null or empty if the annotation has no text content.
    /// </summary>
    public string? Contents { get; }

    /// <summary>
    /// Gets the bounds rectangle of the annotation in page coordinates.
    /// Contains X, Y, Width, and Height of the annotation.
    /// </summary>
    public Rect Bounds { get; }

    /// <summary>
    /// Gets or sets whether the annotation tap has been handled.
    /// Set to true to prevent default annotation behavior.
    /// </summary>
    public bool Handled { get; set; }

    public AnnotationTappedEventArgs(int pageIndex, string? annotationType, string? contents, Rect bounds)
    {
        PageIndex = pageIndex;
        AnnotationType = annotationType;
        Contents = contents;
        Bounds = bounds;
    }
}

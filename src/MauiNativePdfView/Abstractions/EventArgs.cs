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

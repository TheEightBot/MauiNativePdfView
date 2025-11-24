namespace MauiNativePdfView.Abstractions;

/// <summary>
/// Defines how pages should fit on the screen initially.
/// </summary>
public enum FitPolicy
{
    /// <summary>
    /// Fit page width to screen width.
    /// </summary>
    Width,

    /// <summary>
    /// Fit page height to screen height.
    /// </summary>
    Height,

    /// <summary>
    /// Fit entire page to screen (best fit).
    /// </summary>
    Both
}

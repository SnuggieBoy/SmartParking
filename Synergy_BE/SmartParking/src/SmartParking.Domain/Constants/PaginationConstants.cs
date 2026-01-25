namespace SmartParking.Domain.Constants;

/// <summary>
/// Constants for pagination across all list endpoints.
/// Enforces max page size to prevent abuse.
/// </summary>
public static class PaginationConstants
{
    /// <summary>
    /// Default page number when not specified (first page).
    /// </summary>
    public const int DefaultPage = 1;

    /// <summary>
    /// Default page size when not specified.
    /// </summary>
    public const int DefaultPageSize = 10;

    /// <summary>
    /// Maximum allowed page size to prevent abuse.
    /// SECURITY: Prevents clients from requesting huge pages that could DoS the API.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Minimum page number (must be >= 1).
    /// </summary>
    public const int MinPage = 1;

    /// <summary>
    /// Minimum page size (must be >= 1).
    /// </summary>
    public const int MinPageSize = 1;
}

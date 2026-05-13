// Summary DTO for race listings - lightweight representation for API responses
namespace F1BettingApp.Application.DTOs;

using F1BettingApp.Domain.Enums;

/// <summary>
/// Lightweight DTO for race summary information in lists and search results
/// </summary>
public class RaceSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Circuit { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime RaceDate { get; set; }
    public RaceStatus Status { get; set; }
    public int Season { get; set; }
    public string Flag { get; set; } = string.Empty;
}

/// <summary>
/// Generic pagination result wrapper
/// </summary>
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }

    /// <summary>
    /// Gets the total number of pages (1-indexed)
    /// </summary>
    public int Count => TotalPages;
}

/// <summary>
/// Standardized error response format for API errors
/// </summary>
public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }

    /// <summary>
    /// Creates an error response from an exception
    /// </summary>
    public static ErrorResponse FromException(Exception ex) => new()
    {
        Error = "INTERNAL_ERROR",
        Message = "An unexpected error occurred",
        Details = ex.Message
    };
}
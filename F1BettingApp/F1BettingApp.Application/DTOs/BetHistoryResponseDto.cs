using System;
using System.Collections.Generic;
using F1BettingApp.Application.DTOs;

namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// DTO for paginated bet history response.
    /// </summary>
    public class BetHistoryResponseDto
    {
        /// <summary>
        /// The list of bets in this page.
        /// </summary>
        public List<BetHistoryDto> Bets { get; set; } = new();

        /// <summary>
        /// Total number of bets across all pages.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number (1-based).
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages available.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / (double)PageSize);

        /// <summary>
        /// Indicates if there are more pages to fetch.
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Indicates if there are previous pages.
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;
    }
}
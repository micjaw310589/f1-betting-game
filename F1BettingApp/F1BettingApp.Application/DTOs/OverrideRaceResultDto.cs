using System.Collections.Generic;

namespace F1BettingApp.Application.DTOs;

/// <summary>
/// DTO for overriding race results manually (admin only).
/// </summary>
public class OverrideRaceResultDto
{
    /// <summary>
    /// List of positions in order (1st, 2nd, 3rd, etc.) with driver IDs.
    /// </summary>
    public List<PositionEntryDto> Positions { get; set; } = new();

    /// <summary>
    /// Optional: Fastest lap driver ID.
    /// </summary>
    public int? FastestLapDriverId { get; set; }
}

/// <summary>
/// Represents a finishing position with its driver ID.
/// </summary>
public class PositionEntryDto
{
    /// <summary>
    /// Finishing position (1-based).
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Driver ID that finished in this position.
    /// </summary>
    public int DriverId { get; set; }
}

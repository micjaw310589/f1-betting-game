using System;
using System.Collections.Generic;

namespace F1BettingApp.Domain.Entities;

/// <summary>
/// Stores race results for finished races from the current season only.
/// </summary>
public class RaceResult
{
    public int Id { get; set; }
    public int RaceId { get; set; }
    public int Season { get; set; }
    public ICollection<RaceResultPosition> Positions { get; set; } = new List<RaceResultPosition>();
    public int? FastestLapDriverId { get; set; }
    public TimeSpan? FastLapTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Navigation property to the associated race.
    /// </summary>
    public Race Race { get; set; } = null!;

    /// <summary>
    /// Navigation property to the fastest lap driver.
    /// </summary>
    public Driver? FastestLapDriver { get; set; }
}

/// <summary>
/// A single finishing position in a race result.
/// </summary>
public class RaceResultPosition
{
    public int Id { get; set; }
    public int RaceResultId { get; set; }
    public int Position { get; set; }
    public int DriverId { get; set; }
    public int TeamId { get; set; }
    public int Points { get; set; }

    /// <summary>
    /// Navigation property to the associated race result.
    /// </summary>
    public RaceResult RaceResult { get; set; } = null!;

    /// <summary>
    /// Navigation property to the driver.
    /// </summary>
    public Driver Driver { get; set; } = null!;
}
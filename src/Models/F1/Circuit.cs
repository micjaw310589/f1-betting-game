using F1BettingGame.Models.Enums;
using F1BettingGame.Models.Results;

namespace F1BettingGame.Models.F1;

/// <summary>
/// Tor wyścigowy Formuły 1.
/// Odpowiada tabeli: circuits
/// </summary>
public class Circuit
{
    public short CircuitId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? FullName { get; set; }

    public string Country { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    /// <summary>Długość okrążenia w kilometrach.</summary>
    public decimal TrackLengthKm { get; set; }

    public short NumOfLaps { get; set; }

    /// <summary>Kierunek jazdy: Clockwise lub Counterclockwise.</summary>
    public CircuitDirection? Direction { get; set; }

    /// <summary>Wysokość nad poziomem morza (metry).</summary>
    public short? AltitudeM { get; set; }

    public short? FirstGpYear { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public ICollection<Race> Races { get; set; } = [];

    public ICollection<CircuitRecord> CircuitRecords { get; set; } = [];
}

namespace F1BettingGame.Models.F1;

/// <summary>
/// Rekord okrążenia na danym torze.
/// Odpowiada tabeli: circuit_records
/// </summary>
public class CircuitRecord
{
    public int RecordId { get; set; }

    public short CircuitId { get; set; }

    public int DriverId { get; set; }

    public short SeasonId { get; set; }

    /// <summary>Czas rekordu okrążenia (TimeSpan odpowiada typowi INTERVAL w PostgreSQL).</summary>
    public TimeSpan LapRecordTime { get; set; }

    /// <summary>FK do wyścigu, w którym ustanowiono rekord.</summary>
    public int? SetAtRaceId { get; set; }

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public Circuit Circuit { get; set; } = null!;

    public Driver Driver { get; set; } = null!;

    public Season Season { get; set; } = null!;
}

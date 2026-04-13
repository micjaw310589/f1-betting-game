namespace F1BettingGame.Models.F1;

/// <summary>
/// Bolid wyścigowy przypisany do zespołu w danym sezonie.
/// Unikalny rekord na kombinację (team_id, season_id).
/// Odpowiada tabeli: cars
/// </summary>
public class Car
{
    public int CarId { get; set; }

    public int TeamId { get; set; }

    public short SeasonId { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public string? EngineCode { get; set; }

    public string? ChassisCode { get; set; }

    public string? PowerUnitSupplier { get; set; }

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public Team Team { get; set; } = null!;

    public Season Season { get; set; } = null!;
}

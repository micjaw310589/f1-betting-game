namespace F1BettingGame.Models.Betting;

/// <summary>
/// Typ zakładu (np. RACE_WINNER, POLE_POSITION, FASTEST_LAP, SAFETY_CAR_DEPLOYED).
/// Odpowiada tabeli: bet_types
/// </summary>
public class BetType
{
    public short BetTypeId { get; set; }

    public short CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Unikalny kod typu zakładu, np. RACE_WINNER, H2H_DRIVER.</summary>
    public string? Code { get; set; }

    public string? Description { get; set; }

    /// <summary>Czy zakład wymaga wskazania kierowcy.</summary>
    public bool RequiresDriver { get; set; } = false;

    /// <summary>Czy zakład wymaga wskazania zespołu.</summary>
    public bool RequiresTeam { get; set; } = false;

    public bool IsActive { get; set; } = true;

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public BetCategory Category { get; set; } = null!;

    public ICollection<Odd> Odds { get; set; } = [];
}

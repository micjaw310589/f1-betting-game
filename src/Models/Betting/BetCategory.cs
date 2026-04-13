namespace F1BettingGame.Models.Betting;

/// <summary>
/// Kategoria typów zakładów (np. Wyścig, Kwalifikacje, Sezon).
/// Odpowiada tabeli: bet_categories
/// </summary>
public class BetCategory
{
    public short CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Unikalny kod kategorii, np. RACE, QUALIFYING, SEASON.</summary>
    public string? Code { get; set; }

    public string? Description { get; set; }

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public ICollection<BetType> BetTypes { get; set; } = [];
}

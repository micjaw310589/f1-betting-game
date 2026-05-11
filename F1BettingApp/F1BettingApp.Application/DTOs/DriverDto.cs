namespace F1BettingApp.Application.DTOs;

/// <summary>
/// DTO for driver information.
/// </summary>
public class DriverDto
{
    /// <summary>
    /// Driver ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Driver full name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Driver abbreviation (e.g., VER, NOR).
    /// </summary>
    public string Abbreviation { get; set; } = string.Empty;

    /// <summary>
    /// Team ID the driver belongs to.
    /// </summary>
    public int TeamId { get; set; }

    /// <summary>
    /// Team name the driver belongs to.
    /// </summary>
    public string TeamName { get; set; } = string.Empty;
}

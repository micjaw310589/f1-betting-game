using F1BettingApp.Domain.Enums;

namespace F1BettingApp.Application.DTOs;

/// <summary>
/// DTO for updating race metadata (name, date, status, circuit, country).
/// </summary>
public class UpdateRaceMetadataDto
{
    public string? Name { get; set; }
    public DateTime? Date { get; set; }
    public string? Circuit { get; set; }
    public string? Country { get; set; }
    public RaceStatus? Status { get; set; }
}

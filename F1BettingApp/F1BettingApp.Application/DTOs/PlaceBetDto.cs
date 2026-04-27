using F1BettingApp.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// DTO for placing a new bet with comprehensive validation
    /// </summary>
    public class PlaceBetDto
    {
    /// <summary>
    /// User ID (will be set from JWT token)
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Race ID to place bet on
    /// </summary>
    [Required(ErrorMessage = "Race ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Race ID must be a positive number")]
    public int RaceId { get; set; }

    /// <summary>
    /// Driver ID to bet on
    /// </summary>
    [Required(ErrorMessage = "Driver ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Driver ID must be a positive number")]
    public int DriverId { get; set; }

    /// <summary>
    /// Bet amount
    /// </summary>
    [Required(ErrorMessage = "Amount is required")]
    [Range(10, 10000, ErrorMessage = "Amount must be between 10 and 10,000")]
    [Display(Name = "Bet Amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Bet type
    /// </summary>
    [Required(ErrorMessage = "Bet type is required")]
    public BetType BetType { get; set; } = BetType.RaceWinner;

    /// <summary>
    /// Optional prediction for certain bet types
    /// </summary>
    [Display(Name = "Prediction")]
    public int? Prediction { get; set; }
    }
}
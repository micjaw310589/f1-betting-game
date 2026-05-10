namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// DTO for listing users in the admin panel.
    /// Contains essential user information for admin management.
    /// </summary>
    public class AdminUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Points { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    /// <summary>
    /// DTO for adjusting a user's point balance (admin action).
    /// </summary>
    public class AdjustUserPointsDto
    {
        /// <summary>
        /// The amount of points to add (positive) or remove (negative).
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Optional reason/description for the adjustment.
        /// </summary>
        public string? Reason { get; set; }
    }

    /// <summary>
    /// DTO for the result of a point adjustment operation.
    /// </summary>
    public class AdjustPointsResultDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int NewBalance { get; set; }
        public int AdjustedBy { get; set; }
        public string? Reason { get; set; }
        public DateTime AdjustedAt { get; set; }
    }

    /// <summary>
    /// DTO for changing a user's account status (suspend/reactivate).
    /// </summary>
    public class ChangeUserStatusDto
    {
        /// <summary>
        /// Whether the user should be active or suspended.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Optional reason for the status change.
        /// </summary>
        public string? Reason { get; set; }
    }
}

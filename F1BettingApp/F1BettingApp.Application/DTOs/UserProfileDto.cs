using System.ComponentModel.DataAnnotations;

namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// DTO for user profile data.
    /// </summary>
    public class UserProfileDto
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = null!;
        
        [EmailAddress]
        [Required]
        [MaxLength(254)]
        public string Email { get; set; } = null!;
        
        [MaxLength(100)]
        public string? FirstName { get; set; }
        
        [MaxLength(100)]
        public string? LastName { get; set; }
        
        [MaxLength(200)]
        public string? Bio { get; set; }
        
        public int Points { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime LastLoginAt { get; set; }
    }

    /// <summary>
    /// DTO for updating user profile.
    /// </summary>
    public class UpdateProfileDto
    {
        [MaxLength(50)]
        public string? Username { get; set; }
        
        [EmailAddress]
        [MaxLength(254)]
        public string? Email { get; set; }
        
        [MaxLength(100)]
        public string? FirstName { get; set; }
        
        [MaxLength(100)]
        public string? LastName { get; set; }
        
        [MaxLength(200)]
        public string? Bio { get; set; }
    }
}
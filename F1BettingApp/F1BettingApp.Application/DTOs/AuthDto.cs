using System.ComponentModel.DataAnnotations;

namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// DTO for user registration request
    /// </summary>
    public class RegisterDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
        public string Password { get; set; }

        /// <summary>
        /// Optional profile image URL
        /// </summary>
        [Url(ErrorMessage = "Invalid profile image URL format")]
        public string? ProfileImageUrl { get; set; }
    }

    /// <summary>
    /// DTO for user login request
    /// </summary>
    public class LoginDto
    {
        [Required(ErrorMessage = "Username or email is required")]
        [StringLength(50)]
        public string UsernameOrEmail { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Optional remember me flag for extended session
        /// </summary>
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// DTO for token refresh request
    /// </summary>
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "Access token is required")]
        [StringLength(500)]
        public string Token { get; set; }

        [Required(ErrorMessage = "Refresh token is required")]
        [StringLength(500)]
        public string RefreshToken { get; set; }
    }

    /// <summary>
    /// DTO for authentication response containing tokens
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        public bool IsSuccess { get; set; } = true;

        /// <summary>
        /// Error message if operation failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Access token (short-lived, typically 15-30 minutes)
        /// </summary>
        public string AccessToken { get; set; } = "";

        /// <summary>
        /// Refresh token (long-lived, used to obtain new access tokens)
        /// </summary>
        public string RefreshToken { get; set; } = "";

        /// <summary>
        /// Token type indicator
        /// </summary>
        public string TokenType => "Bearer";

        /// <summary>
        /// Access token expiration time in seconds
        /// </summary>
        public int AccessTokenExpiration { get; set; } = 1800; // 30 minutes

        /// <summary>
        /// Refresh token expiration time in days
        /// </summary>
        public int RefreshTokenExpiration { get; set; } = 7; // 7 days

        /// <summary>
        /// User information included in response
        /// </summary>
        public UserDto? User { get; set; }
    }

    /// <summary>
    /// DTO for token refresh response
    /// </summary>
    public class RefreshTokenResponseDto : AuthResponseDto
    {
        /// <summary>
        /// Indicates if the refresh was successful
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Error message if refresh failed
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// DTO for login response
    /// </summary>
    public class LoginResponseDto : AuthResponseDto
    {
        /// <summary>
        /// Indicates if login was successful
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Error message if login failed
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// DTO for registration response
    /// </summary>
    public class RegisterResponseDto
    {
        /// <summary>
        /// Indicates if registration was successful
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Error message if registration failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// User information after successful registration
        /// </summary>
        public UserDto? User { get; set; }
    }
}
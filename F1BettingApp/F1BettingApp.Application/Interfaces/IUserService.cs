using F1BettingApp.Application.DTOs;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Interfaces
{
    /// <summary>
    /// Interface for user-related operations
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Gets a user by their ID
        /// </summary>
        /// <param name="id">The ID of the user</param>
        /// <returns>User DTO</returns>
        Task<UserDto> GetUserByIdAsync(int id);

        /// <summary>
        /// Gets a user by their username
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <returns>User DTO</returns>
        Task<UserDto> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Registers a new user (legacy method - kept for backward compatibility)
        /// </summary>
        /// <param name="username">The username for the new user</param>
        /// <param name="email">The email for the new user</param>
        /// <param name="password">The password for the new user</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task RegisterUserAsync(string username, string email, string password);

        /// <summary>
        /// Registers a new user with DTO (recommended method)
        /// </summary>
        /// <param name="dto">The registration data including validated fields</param>
        /// <returns>Authentication response containing tokens and user information</returns>
        Task<AuthResponseDto> RegisterUserAsync(RegisterDto dto);

        /// <summary>
        /// Authenticates a user with DTO (recommended method)
        /// </summary>
        /// <param name="dto">The login data including username/email and password</param>
        /// <returns>Authentication response containing tokens and user information</returns>
        Task<AuthResponseDto> AuthenticateUserAsync(LoginDto dto);

        /// <summary>
        /// Refreshes an access token using a refresh token (recommended method)
        /// </summary>
        /// <param name="dto">The refresh token data</param>
        /// <returns>Authentication response containing new tokens and user information</returns>
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);

        /// <summary>
        /// Validates user credentials
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <returns>True if credentials are valid, false otherwise</returns>
        Task<bool> ValidateUserAsync(string username, string password);

        /// <summary>
        /// Gets the leaderboard position for a user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>Task representing the user's leaderboard position</returns>
        Task<int> GetUserLeaderboardPositionAsync(int userId);

        /// <summary>
        /// Gets statistics for a user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>User statistics DTO</returns>
        Task<UserStatisticsDto> GetUserStatisticsAsync(int userId);

        /// <summary>
        /// Updates a user's points
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="points">The points to add</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task UpdateUserPointsAsync(int userId, int points);

        /// <summary>
        /// Gets a user's profile by their ID
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>User profile DTO</returns>
        Task<UserProfileDto> GetUserProfileAsync(int userId);

        /// <summary>
        /// Updates a user's profile
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="dto">The updated profile data</param>
        /// <returns>Updated user profile DTO</returns>
        Task<UserProfileDto> UpdateUserProfileAsync(int userId, UpdateProfileDto dto);

        /// <summary>
        /// Gets bet history for a user with pagination
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
        /// <returns>Paginated bet history DTO</returns>
        Task<BetHistoryResponseDto> GetUserBetHistoryAsync(int userId, int page = 1, int pageSize = 20);

        // --- Admin Methods ---

        /// <summary>
        /// Gets all users with optional filtering and pagination (admin only).
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
        /// <param name="filterIsActive">Optional filter by active status. null = all users.</param>
        /// <param name="searchTerm">Optional search by username or email.</param>
        /// <returns>Paginated list of admin user DTOs.</returns>
        Task<PagedResult<AdminUserDto>> GetAllUsersAsync(int page = 1, int pageSize = 20, bool? filterIsActive = null, string? searchTerm = null);

        /// <summary>
        /// Adjusts a user's point balance (admin only).
        /// </summary>
        /// <param name="userId">The ID of the user to adjust.</param>
        /// <param name="pointsDelta">Positive to add, negative to remove.</param>
        /// <param name="reason">Optional reason for the adjustment.</param>
        /// <param name="adminUserId">The ID of the admin performing the action.</param>
        /// <returns>Result of the adjustment operation.</returns>
        Task<AdjustPointsResultDto> AdjustUserPointsAsync(int userId, int pointsDelta, string? reason, int adminUserId);

        /// <summary>
        /// Changes a user's active status (suspend/reactivate) (admin only).
        /// </summary>
        /// <param name="userId">The ID of the user to modify.</param>
        /// <param name="isActive">Whether the user should be active.</param>
        /// <param name="reason">Optional reason for the status change.</param>
        /// <param name="adminUserId">The ID of the admin performing the action.</param>
        /// <returns>Updated user DTO.</returns>
        Task<AdminUserDto> ChangeUserStatusAsync(int userId, bool isActive, string? reason, int adminUserId);
    }
}

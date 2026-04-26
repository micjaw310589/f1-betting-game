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
        /// Registers a new user
        /// </summary>
        /// <param name="username">The username for the new user</param>
        /// <param name="email">The email for the new user</param>
        /// <param name="password">The password for the new user</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task RegisterUserAsync(string username, string email, string password);

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
    }
}

using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using System.Transactions;

namespace F1BettingApp.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Bet> _betRepository;
        private readonly IRepository<Result> _resultRepository;

        public UserService(
            IRepository<User> userRepository,
            IRepository<Bet> betRepository,
            IRepository<Result> resultRepository)
        {
            _userRepository = userRepository;
            _betRepository = betRepository;
            _resultRepository = resultRepository;
        }

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Points = user.Points
            };
        }

        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Username == username);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Points = user.Points
            };
        }

        public async Task RegisterUserAsync(string username, string email, string password)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username is required");
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password is required");

            // Check if user already exists
            var existingUsers = await _userRepository.GetAllAsync();
            if (existingUsers.Any(u => u.Username == username)) throw new InvalidOperationException("Username already exists");
            if (existingUsers.Any(u => u.Email == email)) throw new InvalidOperationException("Email already exists");

            var user = new User(username, email, password);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Username == username);
            return user != null && user.PasswordHash == password; // In real app, verify hash
        }

        public async Task<int> GetUserLeaderboardPositionAsync(int userId)
        {
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Id == userId);
            if (user == null) throw new InvalidOperationException("User not found");

            // Order users by points (descending) and get position
            var orderedUsers = users.OrderByDescending(u => u.Points).ToList();
            var position = orderedUsers.FindIndex(u => u.Id == userId) + 1; // +1 because positions start at 1

            return position;
        }

        public async Task<UserStatisticsDto> GetUserStatisticsAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new InvalidOperationException("User not found");

            var bets = (await _betRepository.GetAllAsync()).Where(b => b.UserId == userId);
            var winningBets = bets.Where(b => b.Status == Domain.Enums.BetStatus.Won);

            // Calculate win rate
            decimal winRate = bets.Any() ? (decimal)winningBets.Count() / bets.Count() * 100 : 0;

            // Calculate total winnings
            decimal totalWinnings = winningBets.Sum(b => b.PotentialWinnings);

            // Get leaderboard position
            var leaderboardPosition = await GetUserLeaderboardPositionAsync(userId);

            return new UserStatisticsDto
            {
                UserId = user.Id,
                Username = user.Username,
                TotalBets = bets.Count(),
                WinningBets = winningBets.Count(),
                WinRate = winRate,
                TotalWinnings = totalWinnings,
                Points = user.Points,
                Rank = leaderboardPosition
            };
        }

        public async Task UpdateUserPointsAsync(int userId, int points)
        {
            if (points == 0) return; // No change needed

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new InvalidOperationException("User not found");

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    user.Points += points;
                    if (user.Points < 0) user.Points = 0; // Prevent negative points

                    await _userRepository.UpdateAsync(user);
                    await _userRepository.SaveChangesAsync();

                    transaction.Complete();
                }
                catch
                {
                    transaction.Dispose();
                    throw;
                }
            }
        }
    }
}

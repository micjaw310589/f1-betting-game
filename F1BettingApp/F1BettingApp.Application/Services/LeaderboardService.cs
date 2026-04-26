using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using System.Transactions;

namespace F1BettingApp.Application.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<LeaderboardHistory> _leaderboardHistoryRepository;
        private readonly IRepository<Race> _raceRepository;

        public LeaderboardService(
            IRepository<User> userRepository,
            IRepository<LeaderboardHistory> leaderboardHistoryRepository,
            IRepository<Race> raceRepository)
        {
            _userRepository = userRepository;
            _leaderboardHistoryRepository = leaderboardHistoryRepository;
            _raceRepository = raceRepository;
        }

        public async Task UpdateLeaderboardAsync()
        {
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var users = await _userRepository.GetAllAsync();
                    var currentSeason = DateTime.UtcNow.Year.ToString();

                    // Get all races from current season
                    var currentSeasonRaces = (await _raceRepository.GetAllAsync())
                        .Where(r => r.Season.ToString() == currentSeason && r.Status == Domain.Enums.RaceStatus.Finished);

                    // Clear existing history for current season and recreate
                    var existingHistories = await _leaderboardHistoryRepository.GetAllAsync();
                    var currentSeasonHistories = existingHistories.Where(h => h.Season == currentSeason).ToList();

                    foreach (var history in currentSeasonHistories)
                    {
                        await _leaderboardHistoryRepository.DeleteAsync(history.Id);
                    }

                    // Create new history entries
                    int rank = 1;
                    var orderedUsers = users.OrderByDescending(u => u.Points).ToList();

                    foreach (var user in orderedUsers)
                    {
                        // Use the last finished race as reference, or create a summary entry
                        var lastFinishedRace = currentSeasonRaces.OrderByDescending(r => r.Date).FirstOrDefault();

                        if (lastFinishedRace != null)
                        {
                            var history = new LeaderboardHistory(
                                user.Id,
                                lastFinishedRace.Id,
                                currentSeason,
                                user.Points,
                                rank
                            );

                            await _leaderboardHistoryRepository.AddAsync(history);
                            rank++;
                        }
                    }

                    await _leaderboardHistoryRepository.SaveChangesAsync();
                    transaction.Complete();
                }
                catch
                {
                    transaction.Dispose();
                    throw;
                }
            }
        }

        public async Task<IEnumerable<UserPointsDto>> GetCurrentLeaderboardAsync(int limit = 10)
        {
            var users = await _userRepository.GetAllAsync();
            var leaderboard = users.OrderByDescending(u => u.Points)
                .Take(limit)
                .Select((user, index) => new UserPointsDto
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Points = user.Points,
                    Rank = index + 1
                });

            return leaderboard;
        }

        public async Task<IEnumerable<UserPointsDto>> GetSeasonLeaderboardAsync(int season, int limit = 10)
        {
            var historyEntries = await _leaderboardHistoryRepository.GetAllAsync();
            var seasonEntries = historyEntries.Where(h => h.Season == season.ToString())
                .OrderByDescending(h => h.TotalPoints)
                .Take(limit);

            var leaderboard = seasonEntries.Select((entry, index) => new UserPointsDto
            {
                UserId = entry.UserId,
                Username = GetUsernameForUserId(entry.UserId).Result, // Not ideal, but for demo
                Points = entry.TotalPoints,
                Rank = index + 1
            });

            return leaderboard;
        }

        private async Task CalculateRanksForSeason(int season)
        {
            var historyEntries = (await _leaderboardHistoryRepository.GetAllAsync())
                .Where(h => h.Season == season.ToString())
                .OrderByDescending(h => h.TotalPoints)
                .ToList();

            for (int i = 0; i < historyEntries.Count; i++)
            {
                var entry = historyEntries[i];
                entry.Rank = i + 1; // Ranks start at 1
                await _leaderboardHistoryRepository.UpdateAsync(entry);
            }
        }

        private async Task<string> GetUsernameForUserId(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.Username ?? $"User {userId}";
        }

        public async Task UpdateLeaderboardForRaceAsync(int raceId)
        {
            var race = await _raceRepository.GetByIdAsync(raceId);
            if (race == null) throw new InvalidOperationException("Race not found");

            // This would be called after a race is completed to update leaderboard
            // In a real app, this would recalculate points based on race results
            await UpdateLeaderboardAsync();
        }
    }
}
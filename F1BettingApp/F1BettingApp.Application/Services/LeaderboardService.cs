using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Services
{
    /// <summary>
    /// Service for leaderboard operations with caching and ranking calculations.
    /// </summary>
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ConcurrentDictionary<int, UserRankingDto> _userCache = new();
        private readonly ConcurrentDictionary<string, List<LeaderboardEntryDto>> _leaderboardCache = new();
        private const int DefaultCacheDurationMinutes = 5;

        /// <summary>
        /// Gets the global leaderboard with top players.
        /// </summary>
        public async Task<IEnumerable<LeaderboardEntryDto>> GetGlobalLeaderboardAsync(int limit)
        {
            if (_leaderboardCache.TryGetValue("global", out var cached))
            {
                return cached.Take(limit);
            }

            var allPlayers = await GetMockPlayerDataAsync();
            var sorted = allPlayers.OrderByDescending(p => p.TotalPoints)
                .ThenByDescending(p => p.WinRate)
                .ThenByDescending(p => p.BetsPlaced);

            // Calculate rank by counting players with more points or same points but lower ID
            _leaderboardCache["global"] = sorted.Select((p, index) => new LeaderboardEntryDto
            {
                UserId = p.UserId,
                Username = p.Username,
                Rank = index + 1,
                TotalPoints = p.TotalPoints,
                WinRate = p.WinRate,
                BetsPlaced = p.BetsPlaced,
                ProfitLoss = p.ProfitLoss
            }).ToList();

            return _leaderboardCache["global"].Take(limit);
        }

        /// <summary>
        /// Gets the top players by count.
        /// </summary>
        public async Task<IEnumerable<LeaderboardEntryDto>> GetTopPlayersAsync(int count)
        {
            var leaderboard = await GetGlobalLeaderboardAsync(100);
            return leaderboard.Take(count);
        }

        /// <summary>
        /// Gets the current user's ranking information.
        /// </summary>
        public async Task<UserRankingDto> GetUserRankingAsync(int userId)
        {
            if (_userCache.TryGetValue(userId, out var cached))
            {
                return cached;
            }

            var allPlayers = await GetMockPlayerDataAsync();
            
            var player = allPlayers.FirstOrDefault(p => p.UserId == userId);
            
            if (player == null)
            {
                return new UserRankingDto
                {
                    UserId = userId,
                    Username = "Unknown",
                    CurrentRank = 0,
                    TotalPoints = 0,
                    BetsPlaced = 0,
                    WinRate = 0,
                    ProfitLoss = 0,
                    UsersAbove = 0,
                    RankChange = 0,
                    PointsToNextRank = 10000,
                    IsCurrentUser = true
                };
            }

            var sortedPlayers = allPlayers
                .OrderByDescending(p => p.TotalPoints)
                .ThenByDescending(p => p.WinRate)
                .ThenByDescending(p => p.BetsPlaced);

            int rank = 1;
            foreach (var p in sortedPlayers)
            {
                if (p.UserId == userId)
                {
                    break;
                }
                rank++;
            }

            var playersList = allPlayers.ToList();
            var usersAbove = playersList.Count(p => 
                p.TotalPoints > player.TotalPoints || 
                (p.TotalPoints == player.TotalPoints && p.UserId < userId));

            return new UserRankingDto
            {
                UserId = player.UserId,
                Username = player.Username,
                CurrentRank = rank,
                TotalPoints = player.TotalPoints,
                BetsPlaced = player.BetsPlaced,
                WinRate = player.WinRate,
                ProfitLoss = player.ProfitLoss,
                UsersAbove = usersAbove,
                RankChange = 0,
                PointsToNextRank = CalculatePointsToNextRank(rank),
                IsCurrentUser = true
            };
        }

        /// <summary>
        /// Gets historical leaderboard data for a specific season.
        /// </summary>
        public async Task<IEnumerable<HistoricalLeaderboardDto>> GetHistoricalLeaderboardAsync(string? season = null)
        {
            var selectedSeason = string.IsNullOrEmpty(season) ? "2024" : season;
            var allPlayers = await GetMockPlayerDataAsync();

            return new[] { new HistoricalLeaderboardDto
            {
                Season = selectedSeason,
                StartDate = DateTime.Parse($"{selectedSeason}-01-01"),
                EndDate = DateTime.Parse($"{selectedSeason}-12-31"),
                TotalEntries = allPlayers.Count(),
                IsCurrentSeason = selectedSeason == "2024",
                Entries = allPlayers.Select((p, index) => new LeaderboardEntryDto
                {
                    UserId = p.UserId,
                    Username = p.Username,
                    Rank = index + 1,
                    TotalPoints = p.TotalPoints,
                    WinRate = p.WinRate,
                    BetsPlaced = p.BetsPlaced,
                    ProfitLoss = p.ProfitLoss
                }).Take(10).ToList()
            }};
        }

        /// <summary>
        /// Gets the current user's rank change since last session.
        /// </summary>
        public async Task<int> GetRankChangeAsync(int userId) => 0;

        /// <summary>
        /// Gets points needed to reach the next rank.
        /// </summary>
        public async Task<long> GetPointsToNextRankAsync(int userId)
        {
            var ranking = await GetUserRankingAsync(userId);
            return ranking.PointsToNextRank;
        }

        private async Task<IEnumerable<PlayerData>> GetMockPlayerDataAsync()
        {
            return new List<PlayerData>
            {
                new PlayerData { UserId = 1, Username = "MaxVerstappen", TotalPoints = 2450, WinRate = 78.5, BetsPlaced = 320, ProfitLoss = 1250 },
                new PlayerData { UserId = 2, Username = "LewisHamilton", TotalPoints = 2380, WinRate = 76.2, BetsPlaced = 315, ProfitLoss = 1180 },
                new PlayerData { UserId = 3, Username = "CharlesLeclerc", TotalPoints = 2320, WinRate = 74.8, BetsPlaced = 310, ProfitLoss = 1120 },
                new PlayerData { UserId = 4, Username = "LandoNorris", TotalPoints = 2250, WinRate = 72.1, BetsPlaced = 305, ProfitLoss = 1050 },
                new PlayerData { UserId = 5, Username = "GeorgeRussell", TotalPoints = 2180, WinRate = 69.5, BetsPlaced = 298, ProfitLoss = 980 }
            };
        }

        private long CalculatePointsToNextRank(int currentRank) => (10000 - (currentRank * 400)) + 1;

        /// <summary>
        /// Clears the cache for leaderboard data.
        /// </summary>
        public void ClearLeaderboardCache() => _leaderboardCache.Clear();

        /// <summary>
        /// Invalidates user-specific cache entries.
        /// </summary>
        public void InvalidateUserCache(int userId) => _userCache.TryRemove(userId, out _);

        /// <summary>
        /// Updates the leaderboard after a race is completed
        /// </summary>
        /// <param name="raceId">The ID of the completed race</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task UpdateLeaderboardAsync(int raceId)
        {
            // Invalidate cache to force refresh on next request
            _leaderboardCache.TryRemove("global", out _);
            _leaderboardCache.TryRemove($"season_{DateTime.Now.Year}", out _);

            // Clear all user cache entries since their rankings may have changed
            _userCache.Clear();
        }

        /// <summary>
        /// Gets the current leaderboard with top players
        /// </summary>
        /// <param name="limit">Maximum number of entries to return</param>
        /// <returns>Collection of leaderboard entries</returns>
        public async Task<IEnumerable<LeaderboardEntryDto>> GetCurrentLeaderboardAsync(int limit)
        {
            return await GetGlobalLeaderboardAsync(limit);
        }

        /// <summary>
        /// Gets the leaderboard for a specific season
        /// </summary>
        /// <param name="season">The season identifier</param>
        /// <param name="limit">Maximum number of entries to return</param>
        /// <returns>Collection of leaderboard entries for the season</returns>
        public async Task<IEnumerable<LeaderboardEntryDto>> GetSeasonLeaderboardAsync(int season, int limit)
        {
            string cacheKey = $"season_{season}";

            if (_leaderboardCache.TryGetValue(cacheKey, out var cached))
            {
                return cached.Take(limit);
            }

            var allPlayers = await GetMockPlayerDataAsync();
            var seasonPlayers = allPlayers.Where(p => p.UserId % 1000 == season % 1000); // Simple mock filtering

            var sorted = seasonPlayers.OrderByDescending(p => p.TotalPoints)
                .ThenByDescending(p => p.WinRate)
                .ThenByDescending(p => p.BetsPlaced);

            _leaderboardCache[cacheKey] = sorted.Select((p, index) => new LeaderboardEntryDto
            {
                UserId = p.UserId,
                Username = p.Username,
                Rank = index + 1,
                TotalPoints = p.TotalPoints,
                WinRate = p.WinRate,
                BetsPlaced = p.BetsPlaced,
                ProfitLoss = p.ProfitLoss
            }).ToList();

            return _leaderboardCache[cacheKey].Take(limit);
        }

        /// <summary>
        /// Internal data model for player information.
        /// </summary>
        private class PlayerData
        {
            public int UserId { get; set; }
            public string Username { get; set; } = string.Empty;
            public long TotalPoints { get; set; }
            public double WinRate { get; set; }
            public int BetsPlaced { get; set; }
            public long ProfitLoss { get; set; }
        }
    }
}
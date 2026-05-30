using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCryptNet = BCrypt.Net.BCrypt;

namespace F1BettingApp.Application.Services
{
    public class UserService : IUserService
    {
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Bet> _betRepository;
    private readonly IRepository<UserBetStatisticsCache> _statsCacheRepository;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public UserService(
        IRepository<User> userRepository,
        IRepository<Bet> betRepository,
        IRepository<UserBetStatisticsCache> statsCacheRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _betRepository = betRepository;
        _statsCacheRepository = statsCacheRepository;
        _secretKey = configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        _issuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
        _audience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
        _tokenHandler = new JwtSecurityTokenHandler();
    }

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Points = user.Points,
                IsAdmin = user.IsAdmin,
                IsActive = user.IsActive,
            };
        }

        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.UserName == username);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Points = user.Points,
                IsAdmin = user.IsAdmin,
                IsActive = user.IsActive,
            };
        }

        public async Task RegisterUserAsync(string username, string email, string password)
        {
            var dto = new RegisterDto { Username = username, Email = email, Password = password };
            await RegisterUserAsync(dto);
        }

        public async Task<AuthResponseDto> RegisterUserAsync(RegisterDto dto)
        {
            // Validate DTO
            if (string.IsNullOrWhiteSpace(dto.Username)) throw new ArgumentException("Username is required");
            if (string.IsNullOrWhiteSpace(dto.Email)) throw new ArgumentException("Email is required");
            if (string.IsNullOrWhiteSpace(dto.Password)) throw new ArgumentException("Password is required");
            if (dto.Password.Length < 8) throw new ArgumentException("Password must be at least 8 characters");

            // Check if user already exists
            var existingUsers = await _userRepository.GetAllAsync();
            if (existingUsers.Any(u => u.UserName == dto.Username)) throw new InvalidOperationException("Username already exists");
            if (existingUsers.Any(u => u.Email == dto.Email)) throw new InvalidOperationException("Email already exists");

            // Hash password before storing
            var hashedPassword = BCryptNet.HashPassword(dto.Password);

            var user = new User(dto.Username, dto.Email, hashedPassword);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Generate tokens for the newly registered user
            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            return new AuthResponseDto
            {
                IsSuccess = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiration = 1800,
                RefreshTokenExpiration = 7,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Points = 0,
                    IsAdmin = user.IsAdmin,
                    IsActive = user.IsActive,
                }
            };
        }

        public async Task<AuthResponseDto> AuthenticateUserAsync(LoginDto dto)
        {
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.UserName == dto.UsernameOrEmail || u.Email == dto.UsernameOrEmail);

            if (user == null)
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid credentials"
                };

            // Verify password
            if (!BCryptNet.Verify(dto.Password, user.PasswordHash))
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid credentials"
                };

            // Check if user account is active (not suspended)
            if (!user.IsActive)
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Account suspended"
                };

            // Generate tokens
            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            return new AuthResponseDto
            {
                IsSuccess = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiration = 1800,
                RefreshTokenExpiration = 7,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Points = user.Points,
                    IsAdmin = user.IsAdmin,
                    IsActive = user.IsActive,
                }
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
        {
            // Validate the refresh token
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Refresh token is required"
                };
            }

            // Find user by email (simplified approach without refresh token storage)
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault();

            if (user == null)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid or expired refresh token"
                };
            }

            // Check if user account is active (not suspended)
            if (!user.IsActive)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Account suspended"
                };
            }

            // Generate new tokens
            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            return new AuthResponseDto
            {
                IsSuccess = true,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiration = 1800, // 30 minutes
                RefreshTokenExpiration = 7, // 7 days
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Points = user.Points,
                    IsAdmin = user.IsAdmin,
                    IsActive = user.IsActive,
                }
            };
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.UserName == username);
            if (user == null) return false;

            return BCryptNet.Verify(password, user.PasswordHash);
        }

        public async Task<int> GetUserLeaderboardPositionAsync(int userId)
        {
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return 0;

            return users.Count(u => u.Points > user.Points) + 1;
        }

        public async Task<UserStatisticsDto> GetUserStatisticsAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            var bets = await _betRepository.GetAllAsync();
            var userBets = bets.Where(b => b.UserId == userId);

            var winningBets = userBets.Count(b => b.Status == BetStatus.Won);
            var totalBets = userBets.Count();

            return new UserStatisticsDto
            {
                UserId = userId,
                Username = user.UserName,
                TotalBets = totalBets,
                WinningBets = winningBets,
                WinRate = totalBets > 0 ? (decimal)winningBets / totalBets * 100 : 0,
                TotalWinnings = userBets.Sum(b => b.Winnings),
                Points = user.Points,
                Rank = 0 // TODO: Calculate rank
            };
        }

        public async Task UpdateUserPointsAsync(int userId, int points)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.Points += points;
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();
            }
        }

        public async Task<UserProfileDto> GetUserProfileAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            return new UserProfileDto
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Points = user.Points,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLogin ?? DateTime.MinValue
            };
        }

        public async Task<UserProfileDto> UpdateUserProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(dto.Username))
                user.UserName = dto.Username;
            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return new UserProfileDto
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Points = user.Points,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLogin ?? DateTime.MinValue
            };
        }

        public async Task<BetHistoryResponseDto> GetUserBetHistoryAsync(int userId, int page = 1, int pageSize = 20)
        {
            var bets = await _betRepository.GetAllAsync();
            var userBets = bets.Where(b => b.UserId == userId)
                              .OrderByDescending(b => b.CreatedAt)
                              .Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .ToList();

            var betHistoryDtos = userBets.Select(b => new BetHistoryDto
            {
                Id = b.Id,
                UserId = userId.ToString(),
                RaceId = b.RaceId,
                DriverId = b.DriverId,
                Amount = b.Amount,
                BetType = b.BetType,
                Status = b.Status,
                Winnings = b.Winnings,
                CreatedAt = b.CreatedAt,
                ResolvedAt = b.ResolvedAt
            }).ToList();

            var totalCount = bets.Count(b => b.UserId == userId);

            return new BetHistoryResponseDto
            {
                Bets = betHistoryDtos,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        // --- Enhanced Statistics Methods ---
        public async Task<EnhancedUserStatisticsDto> GetEnhancedUserStatisticsAsync(int userId)
        {
            // 1. Check cache first
            var cachedStats = await CheckStatisticsCache(userId);
            if (cachedStats != null) return cachedStats;

            // 2. Calculate from bet history
            var stats = await CalculateUserStatisticsFromBets(userId);

            // 3. Update cache
            await UpdateStatisticsCache(userId, stats);

            return stats;
        }

        public async Task<IEnumerable<BetHistoryDto>> GetBetHistoryAsync(int userId, int limit = 50, int offset = 0, BetStatus? status = null, int? driverId = null)
        {
            var bets = await _betRepository.GetAllAsync();
            var userBets = bets.Where(b => b.UserId == userId);

            // Apply filters
            if (status.HasValue)
            {
                userBets = userBets.Where(b => b.Status == status.Value);
            }

            if (driverId.HasValue)
            {
                userBets = userBets.Where(b => b.DriverId == driverId.Value);
            }

            var betHistoryDtos = userBets.OrderByDescending(b => b.CreatedAt)
                                        .Skip(offset)
                                        .Take(limit)
                                        .Select(b => new BetHistoryDto
                                        {
                                            Id = b.Id,
                                            UserId = userId.ToString(),
                                            RaceId = b.RaceId,
                                            DriverId = b.DriverId,
                                            Amount = b.Amount,
                                            BetType = b.BetType,
                                            Status = b.Status,
                                            Winnings = b.Winnings,
                                            CreatedAt = b.CreatedAt,
                                            ResolvedAt = b.ResolvedAt
                                        }).ToList();

            return betHistoryDtos;
        }

        public async Task<UserBetAnalysisDto> GetUserBetAnalysisAsync(int userId)
        {
            var bets = await _betRepository.GetAllAsync();
            var userBets = bets.Where(b => b.UserId == userId).ToList();

            return await CalculateUserBetAnalysis(userBets, userId);
        }

        public async Task<EnhancedUserStatisticsDto> GetUserStatisticsByTimeRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            var bets = await _betRepository.GetAllAsync();
            var userBets = bets.Where(b => b.UserId == userId &&
                                         b.CreatedAt >= startDate &&
                                         b.CreatedAt <= endDate).ToList();

            return await CalculateUserStatisticsFromBets(userId, userBets);
        }

        public async Task UpdateUserStatisticsCacheAsync(int userId)
        {
            var stats = await CalculateUserStatisticsFromBets(userId);
            await UpdateStatisticsCache(userId, stats);
        }

        public async Task RecalculateAllUserStatisticsAsync()
        {
            var users = await _userRepository.GetAllAsync();
            foreach (var user in users)
            {
                await UpdateUserStatisticsCacheAsync(user.Id);
            }
        }

        // --- Private Helper Methods ---
        private async Task<EnhancedUserStatisticsDto> CheckStatisticsCache(int userId)
        {
            try
            {
                // Check if cached statistics exist and are recent (within last hour)
                var cachedStats = await _statsCacheRepository.GetAllAsync();
                var userCache = cachedStats.FirstOrDefault(c => c.UserId == userId);

                if (userCache != null && userCache.LastUpdated > DateTime.UtcNow.AddHours(-1))
                {
                    // Convert cache entity to DTO
                    return new EnhancedUserStatisticsDto
                    {
                        UserId = userCache.UserId,
                        Username = (await _userRepository.GetByIdAsync(userId))?.UserName ?? "Unknown",
                        TotalBets = userCache.TotalBets,
                        WinningBets = userCache.WinningBets,
                        LosingBets = userCache.LosingBets,
                        PushBets = userCache.PushBets,
                        WinRate = userCache.TotalBets > 0 ? (decimal)userCache.WinningBets / userCache.TotalBets * 100 : 0,
                        TotalWinnings = userCache.TotalWinnings,
                        Points = (await _userRepository.GetByIdAsync(userId))?.Points ?? 0,
                        Rank = 0, // TODO: Calculate rank
                        ReturnOnInvestment = userCache.TotalAmountBet > 0 ? (userCache.TotalWinnings / userCache.TotalAmountBet) * 100 : 0,
                        CurrentWinStreak = userCache.CurrentWinStreak,
                        CurrentLoseStreak = userCache.CurrentLoseStreak,
                        LongestWinStreak = userCache.LongestWinStreak,
                        FavoriteDriverId = userCache.FavoriteDriverId,
                        FavoriteDriverName = userCache.FavoriteDriverId > 0 ? "Driver " + userCache.FavoriteDriverId : "None",
                        AverageBetAmount = userCache.TotalBets > 0 ? userCache.TotalAmountBet / userCache.TotalBets : 0,
                        LargestWin = userCache.LargestWin,
                        LargestLoss = userCache.LargestLoss,
                        LastBetDate = null, // Not stored in cache
                        TotalAmountBet = userCache.TotalAmountBet,
                        BetsThisWeek = 0, // Not stored in cache
                        BetsThisMonth = 0 // Not stored in cache
                    };
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail - fall through to calculate from bets
                Console.Error.WriteLine($"Error checking statistics cache: {ex.Message}");
            }

            return null;
        }

        private async Task<EnhancedUserStatisticsDto> CalculateUserStatisticsFromBets(int userId, List<Bet> userBets = null)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            // Get bets if not provided
            if (userBets == null)
            {
                var allBets = await _betRepository.GetAllAsync();
                userBets = allBets.Where(b => b.UserId == userId).ToList();
            }

            // Handle case when user has no bets
            if (userBets.Count == 0)
            {
                return new EnhancedUserStatisticsDto
                {
                    UserId = userId,
                    Username = user.UserName,
                    TotalBets = 0,
                    WinningBets = 0,
                    LosingBets = 0,
                    PushBets = 0,
                    WinRate = 0,
                    TotalWinnings = 0,
                    Points = user.Points,
                    Rank = 0,
                    ReturnOnInvestment = 0,
                    CurrentWinStreak = 0,
                    CurrentLoseStreak = 0,
                    LongestWinStreak = 0,
                    FavoriteDriverId = 0,
                    FavoriteDriverName = "None",
                    AverageBetAmount = 0,
                    LargestWin = 0,
                    LargestLoss = 0,
                    LastBetDate = null,
                    TotalAmountBet = 0,
                    BetsThisWeek = 0,
                    BetsThisMonth = 0
                };
            }

            // Calculate basic statistics
            var totalBets = userBets.Count();
            var winningBets = userBets.Count(b => b.Status == BetStatus.Won);
            var losingBets = userBets.Count(b => b.Status == BetStatus.Lost);
            var pushBets = userBets.Count(b => b.Status == BetStatus.Push);

            // Calculate financial metrics
            var totalWinnings = userBets.Where(b => b.Status == BetStatus.Won)
                                       .Sum(b => b.Winnings);
            var totalAmountBet = userBets.Sum(b => b.Amount);
            var averageBetAmount = totalBets > 0 ? totalAmountBet / totalBets : 0m;

            // Calculate streaks
            var (currentWinStreak, currentLoseStreak, longestWinStreak) = CalculateStreaks(userBets);

            // Calculate ROI
            var roi = totalAmountBet > 0 ? (totalWinnings / totalAmountBet) * 100 : 0m;

            // Find favorite driver
            var (favoriteDriverId, favoriteDriverName) = FindFavoriteDriver(userBets);

            // Find largest win/loss
            var (largestWin, largestLoss) = FindLargestBets(userBets);

            // Calculate time-based statistics
            var lastBetDate = userBets.OrderByDescending(b => b.CreatedAt).FirstOrDefault()?.CreatedAt;
            var betsThisWeek = userBets.Count(b => b.CreatedAt >= DateTime.UtcNow.AddDays(-7));
            var betsThisMonth = userBets.Count(b => b.CreatedAt >= DateTime.UtcNow.AddDays(-30));

            return new EnhancedUserStatisticsDto
            {
                UserId = userId,
                Username = user.UserName,
                TotalBets = totalBets,
                WinningBets = winningBets,
                LosingBets = losingBets,
                PushBets = pushBets,
                WinRate = totalBets > 0 ? (decimal)winningBets / totalBets * 100 : 0,
                TotalWinnings = totalWinnings,
                Points = user.Points,
                Rank = 0, // TODO: Calculate rank
                ReturnOnInvestment = roi,
                CurrentWinStreak = currentWinStreak,
                CurrentLoseStreak = currentLoseStreak,
                LongestWinStreak = longestWinStreak,
                FavoriteDriverId = favoriteDriverId,
                FavoriteDriverName = favoriteDriverName,
                AverageBetAmount = averageBetAmount,
                LargestWin = largestWin,
                LargestLoss = largestLoss,
                LastBetDate = lastBetDate,
                TotalAmountBet = totalAmountBet,
                BetsThisWeek = betsThisWeek,
                BetsThisMonth = betsThisMonth
            };
        }

        private (int currentWinStreak, int currentLoseStreak, int longestWinStreak) CalculateStreaks(List<Bet> bets)
        {
            int currentWinStreak = 0;
            int currentLoseStreak = 0;
            int longestWinStreak = 0;

            // Order by date to analyze streaks chronologically
            var orderedBets = bets.OrderBy(b => b.CreatedAt).ToList();

            foreach (var bet in orderedBets)
            {
                if (bet.Status == BetStatus.Won)
                {
                    currentWinStreak++;
                    currentLoseStreak = 0;
                    longestWinStreak = Math.Max(longestWinStreak, currentWinStreak);
                }
                else if (bet.Status == BetStatus.Lost)
                {
                    currentLoseStreak++;
                    currentWinStreak = 0;
                }
                else // Push or other status
                {
                    currentWinStreak = 0;
                    currentLoseStreak = 0;
                }
            }

            return (currentWinStreak, currentLoseStreak, longestWinStreak);
        }

        private (int favoriteDriverId, string favoriteDriverName) FindFavoriteDriver(List<Bet> bets)
        {
            var driverBets = bets.GroupBy(b => b.DriverId)
                                .Select(g => new {
                                    DriverId = g.Key,
                                    Count = g.Count()
                                })
                                .OrderByDescending(x => x.Count)
                                .FirstOrDefault();

            if (driverBets != null)
            {
                return (driverBets.DriverId, "Driver " + driverBets.DriverId);
            }

            return (0, "None");
        }

        private (decimal largestWin, decimal largestLoss) FindLargestBets(List<Bet> bets)
        {
            decimal largestWin = 0;
            decimal largestLoss = 0;

            foreach (var bet in bets)
            {
                if (bet.Status == BetStatus.Won && bet.Winnings > largestWin)
                {
                    largestWin = bet.Winnings;
                }
                else if (bet.Status == BetStatus.Lost && bet.Amount > largestLoss)
                {
                    largestLoss = bet.Amount;
                }
            }

            return (largestWin, largestLoss);
        }

        private async Task UpdateStatisticsCache(int userId, EnhancedUserStatisticsDto stats)
{
    try
    {
        var cachedStats = await _statsCacheRepository.GetAllAsync();
        var existingCache = cachedStats.FirstOrDefault(c => c.UserId == userId);

        if (existingCache != null)
        {
            existingCache.TotalBets = stats.TotalBets;
            existingCache.WinningBets = stats.WinningBets;
            existingCache.LosingBets = stats.LosingBets;
            existingCache.PushBets = stats.PushBets;
            existingCache.TotalWinnings = stats.TotalWinnings;
            existingCache.TotalAmountBet = stats.TotalAmountBet;
            existingCache.CurrentWinStreak = stats.CurrentWinStreak;
            existingCache.CurrentLoseStreak = stats.CurrentLoseStreak;
            existingCache.LongestWinStreak = stats.LongestWinStreak;
            existingCache.LastUpdated = DateTime.UtcNow;
            existingCache.FavoriteDriverId = stats.FavoriteDriverId;
            existingCache.LargestWin = stats.LargestWin;
            existingCache.LargestLoss = stats.LargestLoss;

            await _statsCacheRepository.UpdateAsync(existingCache);
        }
        else
        {
            var cacheEntity = new UserBetStatisticsCache
            {
                UserId = userId,
                TotalBets = stats.TotalBets,
                WinningBets = stats.WinningBets,
                LosingBets = stats.LosingBets,
                PushBets = stats.PushBets,
                TotalWinnings = stats.TotalWinnings,
                TotalAmountBet = stats.TotalAmountBet,
                CurrentWinStreak = stats.CurrentWinStreak,
                CurrentLoseStreak = stats.CurrentLoseStreak,
                LongestWinStreak = stats.LongestWinStreak,
                LastUpdated = DateTime.UtcNow,
                FavoriteDriverId = stats.FavoriteDriverId,
                LargestWin = stats.LargestWin,
                LargestLoss = stats.LargestLoss
            };

            await _statsCacheRepository.AddAsync(cacheEntity);
        }

        await _statsCacheRepository.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error updating statistics cache: {ex.Message}");
    }
}

        private async Task<UserBetAnalysisDto> CalculateUserBetAnalysis(List<Bet> userBets, int userId)
        {
            // Initialize analysis DTO
            var analysis = new UserBetAnalysisDto
            {
                UserId = userId,
                BetTypeAnalysis = new Dictionary<BetType, BetTypeAnalysisDto>(),
                DriverAnalysis = new Dictionary<int, DriverAnalysisDto>(),
                TeamAnalysis = new Dictionary<int, TeamAnalysisDto>(),
                MonthlyAnalysis = new MonthlyAnalysisDto[12], // Last 12 months
                TimeOfDayAnalysis = new TimeOfDayAnalysisDto()
            };

            // Group by bet type
            var betTypeGroups = userBets.GroupBy(b => b.BetType);
            foreach (var group in betTypeGroups)
            {
                var winningBets = group.Count(b => b.Status == BetStatus.Won);
                var totalAmount = group.Sum(b => b.Amount);
                var totalWinnings = group.Sum(b => b.Winnings);
                var roi = totalAmount > 0 ? (totalWinnings / totalAmount) * 100 : 0;

                analysis.BetTypeAnalysis[group.Key] = new BetTypeAnalysisDto
                {
                    TotalBets = group.Count(),
                    WinningBets = winningBets,
                    WinRate = group.Count() > 0 ? (decimal)winningBets / group.Count() * 100 : 0,
                    TotalAmount = totalAmount,
                    TotalWinnings = totalWinnings,
                    ROI = roi
                };
            }

            // Group by driver
            var driverGroups = userBets.GroupBy(b => b.DriverId);
            foreach (var group in driverGroups)
            {
                var driverName = "Driver " + group.Key;
                var winningBets = group.Count(b => b.Status == BetStatus.Won);
                var totalWinnings = group.Sum(b => b.Winnings);

                analysis.DriverAnalysis[group.Key] = new DriverAnalysisDto
                {
                    DriverName = driverName,
                    TotalBets = group.Count(),
                    WinningBets = winningBets,
                    WinRate = group.Count() > 0 ? (decimal)winningBets / group.Count() * 100 : 0,
                    TotalWinnings = totalWinnings
                };
            }

            // Monthly analysis for last 12 months
            for (int i = 0; i < 12; i++)
            {
                var monthDate = DateTime.UtcNow.AddMonths(-i);
                var monthlyBets = userBets.Where(b => b.CreatedAt.Year == monthDate.Year && b.CreatedAt.Month == monthDate.Month).ToList();
                var winningBets = monthlyBets.Count(b => b.Status == BetStatus.Won);
                var totalWinnings = monthlyBets.Sum(b => b.Winnings);

                analysis.MonthlyAnalysis[i] = new MonthlyAnalysisDto
                {
                    Year = monthDate.Year,
                    Month = monthDate.Month,
                    TotalBets = monthlyBets.Count,
                    WinningBets = winningBets,
                    TotalWinnings = totalWinnings
                };
            }

            // Time of day analysis
            int morningBets = 0, afternoonBets = 0, eveningBets = 0, nightBets = 0;
            int morningWins = 0, afternoonWins = 0, eveningWins = 0, nightWins = 0;

            foreach (var bet in userBets)
            {
                var hour = bet.CreatedAt.Hour;
                if (hour >= 6 && hour < 12) // Morning: 6AM - 12PM
                {
                    morningBets++;
                    if (bet.Status == BetStatus.Won) morningWins++;
                }
                else if (hour >= 12 && hour < 18) // Afternoon: 12PM - 6PM
                {
                    afternoonBets++;
                    if (bet.Status == BetStatus.Won) afternoonWins++;
                }
                else if (hour >= 18 && hour < 24) // Evening: 6PM - 12AM
                {
                    eveningBets++;
                    if (bet.Status == BetStatus.Won) eveningWins++;
                }
                else // Night: 12AM - 6AM
                {
                    nightBets++;
                    if (bet.Status == BetStatus.Won) nightWins++;
                }
            }

            analysis.TimeOfDayAnalysis = new TimeOfDayAnalysisDto
            {
                MorningBets = morningBets,
                AfternoonBets = afternoonBets,
                EveningBets = eveningBets,
                NightBets = nightBets,
                MorningWinRate = morningBets > 0 ? (decimal)morningWins / morningBets * 100 : 0,
                AfternoonWinRate = afternoonBets > 0 ? (decimal)afternoonWins / afternoonBets * 100 : 0,
                EveningWinRate = eveningBets > 0 ? (decimal)eveningWins / eveningBets * 100 : 0,
                NightWinRate = nightBets > 0 ? (decimal)nightWins / nightBets * 100 : 0
            };

            return analysis;
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            // Add admin role claim if user is an admin
            if (user.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims.ToArray(),
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return _tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var random = new Random();
            var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";
            var refreshToken = new string(
                characters.Skip(random.Next(0, characters.Length)).Take(64).ToArray()
            );
            return refreshToken;
        }

        // --- Admin Methods ---

        public async Task<PagedResult<AdminUserDto>> GetAllUsersAsync(int page = 1, int pageSize = 20, bool? filterIsActive = null, string? searchTerm = null)
        {
            var allUsers = await _userRepository.GetAllAsync();

            // Apply filters
            var query = allUsers.AsQueryable();

            if (filterIsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == filterIsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(u => u.UserName.ToLower().Contains(term) || u.Email.ToLower().Contains(term));
            }

            var totalCount = query.Count();
            var pagedUsers = query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var adminUserDtos = pagedUsers.Select(u => new AdminUserDto
            {
                Id = u.Id,
                Username = u.UserName,
                Email = u.Email,
                Points = u.Points,
                IsActive = u.IsActive,
                IsAdmin = u.IsAdmin,
                CreatedAt = u.CreatedAt,
                LastLogin = u.LastLogin
            }).ToList();

            return new PagedResult<AdminUserDto>
            {
                Items = adminUserDtos,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalCount,
                TotalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0
            };
        }

        public async Task<AdjustPointsResultDto> AdjustUserPointsAsync(int userId, int pointsDelta, string? reason, int adminUserId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            // Prevent negative balance
            var newBalance = user.Points + pointsDelta;
            if (newBalance < 0)
            {
                throw new InvalidOperationException("Cannot deduct points below zero.");
            }

            // Apply the adjustment
            if (pointsDelta > 0)
            {
                user.AddPoints(pointsDelta);
            }
            else if (pointsDelta < 0)
            {
                user.DeductPoints(Math.Abs(pointsDelta));
            }

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return new AdjustPointsResultDto
            {
                UserId = user.Id,
                Username = user.UserName,
                NewBalance = user.Points,
                AdjustedBy = adminUserId,
                Reason = reason,
                AdjustedAt = DateTime.UtcNow
            };
        }

        public async Task<AdminUserDto> ChangeUserStatusAsync(int userId, bool isActive, string? reason, int adminUserId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            user.IsActive = isActive;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return new AdminUserDto
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Points = user.Points,
                IsActive = user.IsActive,
                IsAdmin = user.IsAdmin,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin
            };
        }
    }
}
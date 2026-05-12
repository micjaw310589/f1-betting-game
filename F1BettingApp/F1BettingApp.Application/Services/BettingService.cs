using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Exceptions;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.Persistence;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
namespace F1BettingApp.Application.Services
{
    /// <summary>
    /// Service for handling all betting-related operations
    /// </summary>
    public class BettingService : IBettingService
    {
private readonly IBetRepositoryExtensions _betRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Race> _raceRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IUserService _userService;
        private readonly IRaceService _raceService;
        private readonly INotificationService _notificationService;
        private readonly AppDbContext _dbContext;

        /// <summary>
        /// Constructor for BettingService
        /// </summary>
        public BettingService(
            IBetRepositoryExtensions betRepository,
            IRepository<User> userRepository,
            IRaceRepositoryExtensions raceRepository,
            IRepository<Driver> driverRepository,
            IUserService userService,
            IRaceService raceService,
            INotificationService notificationService,
            AppDbContext dbContext)
        {
            _betRepository = betRepository;
            _userRepository = userRepository;
            _raceRepository = raceRepository;
            _driverRepository = driverRepository;
            _userService = userService;
            _raceService = raceService;
            _notificationService = notificationService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Places a bet on a specific driver in a race using the current authenticated user
        /// </summary>
        /// <param name="dto">Bet details with validation</param>
        /// <returns>The created bet as DTO</returns>
        public async Task<BetResponseDto> PlaceBetAsync(PlaceBetDto dto)
        {
            throw new NotSupportedException("Use PlaceBetAsync(userId, dto) instead.");
        }

        /// <summary>
        /// Places a bet on a specific driver in a race
        /// </summary>
        /// <param name="userId">User ID (passed from controller)</param>
        /// <param name="dto">Bet details with validation</param>
        /// <returns>The created bet as DTO</returns>
        public async Task<BetResponseDto> PlaceBetAsync(int userId, PlaceBetDto dto)
        {
            if (dto.Amount <= 0)
            {
                throw new ArgumentException("Bet amount must be greater than zero.");
            }
    

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException("User not found");
            }

            if ((decimal)user.Points < dto.Amount)
            {
                throw new InsufficientFundsException(dto.Amount, (decimal)user.Points);
            }

            var race = await _raceRepository.GetByIdAsync(dto.RaceId);
            if (race == null)
            {
                throw new RaceNotFoundException(dto.RaceId);
            }

            // Task 03 strict requirement: only Scheduled races accept new bets
            if (race.Status != RaceStatus.Scheduled)
            {
                throw new RaceNotUpcomingException();
            }

            if (race.Status == RaceStatus.Finished)
            {
                throw new RaceCompletedException();
            }

            var driver = await _driverRepository.GetByIdAsync(dto.DriverId);
            if (driver == null)
            {
                throw new DriverNotFoundException(dto.DriverId);
            }

            decimal odds;
            try
            {
                odds = race.OddsForDriver(driver.Id);
            }
            catch (NotImplementedException)
            {
                odds = 1m;
            }

            var bet = new Bet(user.Id, dto.RaceId, dto.DriverId, dto.Amount, dto.BetType, odds);
            await _betRepository.AddAsync(bet);

            user.Points = (int)((decimal)user.Points - dto.Amount);
            await _userRepository.UpdateAsync(user);

            race.TotalBets = (race.TotalBets ?? 0m) + 1m;
            race.TotalAmount = (race.TotalAmount ?? 0m) + dto.Amount;
            await _raceRepository.UpdateAsync(race);

            return MapBetToDto(bet);
        }

        /// <summary>
        /// Cancels an existing bet
        /// </summary>
        /// <param name="betId">Bet identifier</param>
        /// <param name="userId">User identifier for authorization (extracted from auth context)</param>
        /// <returns>The updated bet as DTO</returns>
        public async Task<BetResponseDto> CancelBetAsync(int betId, int userId)
        {
            // Parse userId if it's a string format


            // Find the bet
            var bet = await _betRepository.GetByIdAsync(betId);
            if (bet == null)
            {
                throw new BetNotFoundException(betId);
            }

            // Verify bet belongs to the current user
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || bet.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can only cancel your own bets");
            }

            // Verify race hasn't started yet
            var race = await _raceService.GetRaceByIdAsync(bet.RaceId);
            if (race == null)
            {
                throw new RaceNotFoundException(bet.RaceId);
            }

            if (race.Status == RaceStatus.InProgress || race.Status == RaceStatus.Finished)
            {
                throw new RaceAlreadyStartedException();
            }

            // Update bet status
            bet.Status = BetStatus.Canceled;
            await _betRepository.UpdateAsync(bet);

            // Refund the bet amount to user
            user.Points = (int)((decimal)user.Points + bet.Amount);
            await _userRepository.UpdateAsync(user);

            return MapBetToDto(bet);
        }

        /// <summary>
        /// Gets all bets for a specific user
        /// </summary>
        /// <param name="userId">User identifier (extracted from auth context)</param>
        /// <returns>Collection of bet response DTOs</returns>
        public async Task<IEnumerable<BetResponseDto>> GetUserBetsAsync(int userId)
        {


            var bets = await _betRepository.GetByUserIdAsync(userId);
            return bets.Select(MapBetToDto).ToList();
        }

        /// <summary>
        /// Gets a specific bet by ID with authorization check
        /// </summary>
        /// <param name="betId">Bet identifier</param>
        /// <param name="userId">User identifier for authorization (extracted from auth context)</param>
        /// <returns>The bet details or null if not found</returns>
        public async Task<BetResponseDto?> GetBetByIdAsync(int betId, int userId)
        {
            var bet = await _betRepository.GetByIdAsync(betId);
            if (bet == null)
            {
                return null;
            }



            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || bet.UserId != user.Id)
            {
                throw new UnauthorizedAccessException("You can only view your own bets");
            }

            return MapBetToDto(bet);
        }

/// <summary>
        /// Processes race results and updates bet statuses
        /// Ensures idempotency: running this method multiple times for the same race
        /// will not credit points twice or cause duplicate side effects.
        /// </summary>
        /// <param name="raceId">Race identifier</param>
        public async Task ProcessRaceResultsAsync(int raceId)
        {
            // Use database context for transactional operations
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Get the race directly from the database
                var race = await _dbContext.Races.FindAsync(raceId);
                if (race == null)
                {
                    throw new RaceNotFoundException(raceId);
                }

                // Idempotency check: if already processed, do nothing
                if (race.Status == RaceStatus.ResultsProcessed)
                {
                    // Already processed - return without doing anything
                    return;
                }

                // Verify race is finished (not just scheduled or in progress)
                if (race.Status != RaceStatus.Finished)
                {
                    throw new InvalidOperationException("Race must be completed before processing results");
                }

                // Get all pending bets for this race from the database
                var pendingBets = await _dbContext.Bets
                    .Where(b => b.RaceId == raceId && b.Status == BetStatus.Pending)
                    .ToListAsync();

                // Get race results from the database
                var results = await _dbContext.Results
                    .Where(r => r.RaceId == raceId)
                    .ToListAsync();

                // If no results exist, we can't process
                if (!results.Any())
                {
                    throw new InvalidOperationException($"No race results found for race ID {raceId}");
                }

// Track which users have been notified to avoid duplicate notifications
                var notifiedUsers = new HashSet<int>();
                var totalWinningsByUser = new Dictionary<int, decimal>();

                // Process each pending bet
                foreach (var bet in pendingBets)
                {
                    var betResult = EvaluateBet(bet, results);

                    // Update bet in the database
                    bet.Status = betResult.NewStatus;
                    bet.Winnings = betResult.Winnings;
                    bet.ResolvedAt = DateTime.UtcNow;
                    _dbContext.Bets.Update(bet);

                    // Accumulate winnings per user
                    if (betResult.Winnings > 0)
                    {
                        totalWinningsByUser[bet.UserId] = totalWinningsByUser.GetValueOrDefault(bet.UserId) + betResult.Winnings;
                    }

                    // Credit user balance if won
                    if (betResult.Winnings > 0)
                    {
                        // Use raw SQL to avoid state management conflicts
                        await _dbContext.Database.ExecuteSqlRawAsync(
                            $"UPDATE \"Users\" SET \"Points\" = \"Points\" + {betResult.Winnings} WHERE \"Id\" = {bet.UserId}");
                    }
                }

                // Send notifications after processing all bets
                foreach (var userId in totalWinningsByUser.Keys)
                {
                    // Get the first winning bet for this user to extract driver name
                    var winningBet = pendingBets.FirstOrDefault(b => b.UserId == userId && totalWinningsByUser[userId] > 0);
                    if (winningBet != null)
                    {
                        var driver = await _dbContext.Drivers.FindAsync(winningBet.DriverId);
                        await _notificationService.CreateNotificationAsync(
                            userId,
                            "🏁 Race Results - You Won!",
                            $"Congratulations! Your bet on {driver?.Name ?? "Driver"} has won! Total winnings: {totalWinningsByUser[userId]} points"
                        );
                    }
                    notifiedUsers.Add(userId);
                }

                // Send loss notifications for users who didn't win anything
                var losingBetters = pendingBets
                    .Where(b => !totalWinningsByUser.ContainsKey(b.UserId) && !notifiedUsers.Contains(b.UserId))
                    .GroupBy(b => b.UserId)
                    .Select(g => g.First())
                    .ToList();

                foreach (var losingBet in losingBetters)
                {
                    var driver = await _dbContext.Drivers.FindAsync(losingBet.DriverId);
                    await _notificationService.CreateNotificationAsync(
                        losingBet.UserId,
                        "🏁 Race Results",
                        $"The race has finished! Your bet on {driver?.Name ?? "Driver"} did not win."
                    );
                    notifiedUsers.Add(losingBet.UserId);
                }

                // Update race status to ResultsProcessed
                race.Status = RaceStatus.ResultsProcessed;
                _dbContext.Races.Update(race);

                // Save all changes
                await _dbContext.SaveChangesAsync();
                
                // Commit the transaction
                await transaction.CommitAsync();
            }
            catch
            {
                // Rollback the transaction on any error
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Evaluates a single bet against race results and returns the evaluation result.
        /// This method is pure logic that can be tested in isolation.
        /// </summary>
        internal BetResult EvaluateBet(Bet bet, IEnumerable<Result> results)
        {
            if (bet == null) throw new ArgumentNullException(nameof(bet));
            if (results == null) throw new ArgumentNullException(nameof(results));

            // Find the driver's finishing position
            var result = results.FirstOrDefault(r => r.DriverId == bet.DriverId);
            
            // If driver didn't finish or has no position, it's a loss
            if (result == null || result.Position == null)
            {
                return new BetResult(BetStatus.Lost, 0m);
            }

            var position = result.Position;
            decimal winnings = 0;
            BetStatus newStatus;

            switch (bet.BetType)
            {
                case BetType.RaceWinner:
                    // Race Winner: driver must finish in position 1
                    if (position == 1)
                    {
                        winnings = bet.Amount * bet.Odds;
                        newStatus = BetStatus.Won;
                    }
                    else
                    {
                        newStatus = BetStatus.Lost;
                    }
                    break;

                case BetType.PodiumFinish:
                    // TOP 3 Finish: driver must finish in position 1, 2, or 3
                    if (position <= 3)
                    {
                        winnings = bet.Amount * bet.Odds;
                        newStatus = BetStatus.Won;
                    }
                    else
                    {
                        newStatus = BetStatus.Lost;
                    }
                    break;

                case BetType.Top10Finish:
                    // TOP 10 Finish: driver must finish in position 1-10
                    if (position <= 10)
                    {
                        winnings = bet.Amount * bet.Odds;
                        newStatus = BetStatus.Won;
                    }
                    else
                    {
                        newStatus = BetStatus.Lost;
                    }
                    break;

                case BetType.FastestLap:
                    // Fastest Lap: driver must have set the fastest lap in the race
                    var fastestLapResult = results.FirstOrDefault(r => r.FastestLap.HasValue);
                    if (fastestLapResult != null && fastestLapResult.DriverId == bet.DriverId)
                    {
                        winnings = bet.Amount * bet.Odds;
                        newStatus = BetStatus.Won;
                    }
                    else
                    {
                        newStatus = BetStatus.Lost;
                    }
                    break;

                default:
                    // Unsupported bet types throw an exception
                    throw new NotSupportedException(
                        $"Bet type '{bet.BetType}' is not supported for automatic bet resolution. " +
                        "Only RaceWinner, PodiumFinish, Top10Finish, and FastestLap bet types are supported.");
            }

            return new BetResult(newStatus, winnings);
        }

        /// <summary>
        /// Represents the result of evaluating a bet.
        /// </summary>
        internal record BetResult(BetStatus NewStatus, decimal Winnings);

        /// <summary>
        /// Calculates winnings for a bet based on race results
        /// </summary>
        /// <param name="bet">The bet to calculate winnings for</param>
        /// <param name="result">The race result containing outcome information</param>
        /// <returns>The calculated winnings amount</returns>
        public async Task<decimal> CalculateWinningsAsync(Bet bet, Result result)
        {
            if (bet == null)
                throw new ArgumentNullException(nameof(bet));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            // Calculate winnings based on bet type and result
            if (bet.BetType == BetType.RaceWinner)
            {
                // Winner bet - only wins if first place
                if (result.Position == 1)
                {
                    return bet.Amount * bet.Odds;
                }
            }
            else if (bet.BetType == BetType.PodiumFinish)
            {
                // Podium finish bet - wins if position is 1, 2, or 3
                if (result.Position <= 3)
                {
                    return bet.Amount * bet.Odds;
                }
            }
            else if (bet.BetType == BetType.Top10Finish)
            {
                // Top 10 bet - wins if position is 1-10
                if (result.Position <= 10)
                {
                    return bet.Amount * bet.Odds;
                }
            }
            else if (bet.BetType == BetType.FastestLap)
            {
                // Fastest lap bet - wins if position is 0 (fastest)
                if (result.Position == 0)
                {
                    return bet.Amount * bet.Odds;
                }
            }

            // Default: no winnings for other cases
            return 0m;
        }

        /// <summary>
        /// Gets user's bet history with pagination support
        /// </summary>
        public async Task<BetHistoryResponseDto> GetUserBetHistoryAsync(int userId, int page = 1, int pageSize = 20)
        {


            var allBets = await _betRepository.GetByUserIdAsync(userId);
            var races = await _raceService.GetRacesByIdsAsync(allBets.Select(b => b.RaceId).Distinct().ToList());

            var paginatedBets = allBets
                .OrderBy(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new BetHistoryResponseDto
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = allBets.Count(),
                Bets = paginatedBets.Select(b => MapBetHistoryDto(b, races)).ToList()
            };
        }

        /// <summary>
        /// Validates a bet before placing it (without creating)
        /// </summary>
        public async Task<BetValidationResult> ValidateBetAsync(int userId, PlaceBetDto dto)
        {
            var result = new BetValidationResult();

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                result.IsValid = false;
                result.Errors.Add("User not found");
                return result;
            }

            if ((decimal)user.Points < dto.Amount)
            {
                result.IsValid = false;
                result.Errors.Add($"Insufficient funds. Current points: {(decimal)user.Points}, Required: {dto.Amount}");
                return result;
            }

            var race = await _raceService.GetRaceByIdAsync(dto.RaceId);
            if (race == null)
            {
                result.IsValid = false;
                result.Errors.Add($"Race not found. Race ID: {dto.RaceId}");
                return result;
            }

            if (race.Status != RaceStatus.Scheduled && race.Status != RaceStatus.InProgress)
            {
                result.IsValid = false;
                result.Errors.Add("Race is not upcoming or has already started");
                return result;
            }

            var driver = await _driverRepository.GetByIdAsync(dto.DriverId);
            if (driver == null)
            {
                result.IsValid = false;
                result.Errors.Add($"Driver not found. Driver ID: {dto.DriverId}");
                return result;
            }

            var odds = GetOddsForDriver(race, driver.Id);
            result.IsValid = true;
            result.Odds = odds;
            result.PotentialWinnings = dto.Amount * odds;
            return result;
        }

        /// <summary>
        /// Gets available races that can accept bets
        /// </summary>
        public async Task<IEnumerable<RaceDetailDto>> GetAvailableRacesAsync(int userId)
        {
 

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException("User not found");
            }

            var races = await _raceService.GetUpcomingRacesAsync();
            return races.Select(r => new RaceDetailDto
            {
                Id = r.Id,
                Name = r.Name ?? string.Empty,
                Circuit = r.Circuit ?? string.Empty,
                Country = r.Country ?? string.Empty,
                RaceDate = r.RaceDate,
                Status = r.Status,
                OpenF1RaceId = string.Empty,
                Season = 0,
                Weather = string.Empty,
                TrackCondition = string.Empty,
                Flag = string.Empty,
                Paddock = string.Empty,
                CircuitLayout = string.Empty,
                SprintRace = string.Empty,
                SprintDate = string.Empty
            }).ToList();
        }


        private static BetResponseDto MapBetToDto(Bet bet)
        {
            return new BetResponseDto
            {
                Id = bet.Id,
                UserId = bet.UserId,
                RaceId = bet.RaceId,
                DriverId = bet.DriverId,
                DriverName = $"Driver {bet.DriverId}",
                Amount = bet.Amount,
                BetType = bet.BetType,
                Status = bet.Status,
                Winnings = bet.Winnings,
                CreatedAt = bet.CreatedAt,
                ResolvedAt = bet.ResolvedAt
            };
        }

        private static BetHistoryDto MapBetHistoryDto(Bet bet, IEnumerable<RaceDto> races)
        {
            var race = races.FirstOrDefault(r => r.Id == bet.RaceId);
            return new BetHistoryDto
            {
                Id = bet.Id,
                UserId = bet.UserId.ToString(),
                RaceId = bet.RaceId,
                DriverId = bet.DriverId,
                DriverName = $"Driver {bet.DriverId}",
                BetType = bet.BetType,
                Amount = bet.Amount,
                Winnings = bet.Winnings,
                Status = bet.Status,
                CreatedAt = bet.CreatedAt,
                ResolvedAt = bet.ResolvedAt,
                RaceName = race?.Name,
                RaceDate = race?.RaceDate
            };
        }

        private static decimal GetOddsForDriver(RaceDto race, int driverId)
        {
            if (race.Odds != null && race.Odds.TryGetValue(driverId, out var odds))
            {
                return odds;
            }

            return 1m;
        }

        /// <summary>
        /// Gets all bets with pagination and optional filtering (admin only).
        /// </summary>
        public async Task<PagedResult<AdminBetResponseDto>> GetAllBetsAsync(int page = 1, int pageSize = 20, BetStatus? filterStatus = null, string? searchTerm = null)
        {
            var allBets = await _betRepository.GetAllAsync();
            var betsList = allBets.ToList();

            // Apply status filter
            if (filterStatus.HasValue)
            {
                betsList = betsList.Where(b => b.Status == filterStatus.Value).ToList();
            }

            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchTermLower = searchTerm.ToLower();
                var searchUsers = await _userRepository.GetAllAsync();
                var searchUserLookup = searchUsers.ToDictionary(u => u.Id, u => u.UserName?.ToLower() ?? string.Empty);
                var searchRaces = await _raceService.GetAllRacesAsync();
                var searchRaceLookup = searchRaces.ToDictionary(r => r.Id, r => r.Name?.ToLower() ?? string.Empty);

                betsList = betsList.Where(b =>
                    (searchUserLookup.ContainsKey(b.UserId) && searchUserLookup[b.UserId].Contains(searchTermLower)) ||
                    (searchRaceLookup.ContainsKey(b.RaceId) && searchRaceLookup[b.RaceId].Contains(searchTermLower))
                ).ToList();
            }

            var totalItems = betsList.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var paginatedBets = betsList
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Build lookup collections for mapping
            var raceIds = paginatedBets.Select(b => b.RaceId).Distinct().ToList();

            // Materialize to client-side first, then filter by the paginated bet IDs
            var userIds = paginatedBets.Select(b => b.UserId).Distinct().ToList();
            var userEntities = await _userRepository.GetAllAsync();
            var userLookup = userEntities
                .Where(u => userIds.Contains(u.Id))
                .ToDictionary(u => u.Id, u => u.UserName ?? $"User {u.Id}");

            var races = await _raceService.GetRacesByIdsAsync(raceIds);
            var raceLookup = races.ToDictionary(r => r.Id, r => r.Name ?? $"Race {r.Id}");

            var driverIds = paginatedBets.Select(b => b.DriverId).Distinct().ToList();
            var driverEntities = await _driverRepository.GetAllAsync();
            var driverLookup = driverEntities
                .Where(d => driverIds.Contains(d.Id))
                .ToDictionary(d => d.Id, d => d.Name ?? $"Driver {d.Id}");

            var items = paginatedBets.Select(b => new AdminBetResponseDto
            {
                Id = b.Id,
                UserId = b.UserId,
                Username = userLookup.GetValueOrDefault(b.UserId, $"User {b.UserId}"),
                RaceId = b.RaceId,
                RaceName = raceLookup.GetValueOrDefault(b.RaceId, $"Race {b.RaceId}"),
                DriverId = b.DriverId,
                DriverName = driverLookup.GetValueOrDefault(b.DriverId, $"Driver {b.DriverId}"),
                Amount = b.Amount,
                Odds = b.Odds,
                BetType = b.BetType,
                Status = b.Status,
                Winnings = b.Winnings,
                PotentialWinnings = b.PotentialWinnings,
                CreatedAt = b.CreatedAt,
                ResolvedAt = b.ResolvedAt
            }).ToList();

            return new PagedResult<AdminBetResponseDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

        /// <summary>
        /// Creates a new bet on behalf of a user (admin only).
        /// </summary>
        public async Task<AdminBetResponseDto> CreateBetAsync(CreateBetDto dto, int adminUserId)
        {
            if (dto.Amount <= 0)
            {
                throw new ArgumentException("Bet amount must be greater than zero.");
            }

            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                throw new UserNotFoundException("User not found");
            }

            if ((decimal)user.Points < dto.Amount)
            {
                throw new InsufficientFundsException(dto.Amount, (decimal)user.Points);
            }

            var race = await _raceRepository.GetByIdAsync(dto.RaceId);
            if (race == null)
            {
                throw new RaceNotFoundException(dto.RaceId);
            }

            if (race.Status != RaceStatus.Scheduled)
            {
                throw new RaceNotUpcomingException();
            }

            var driver = await _driverRepository.GetByIdAsync(dto.DriverId);
            if (driver == null)
            {
                throw new DriverNotFoundException(dto.DriverId);
            }

            decimal odds;
            try
            {
                odds = race.OddsForDriver(driver.Id);
            }
            catch (NotImplementedException)
            {
                odds = 1m;
            }

            var bet = new Bet(user.Id, dto.RaceId, dto.DriverId, dto.Amount, dto.BetType, odds);
            await _betRepository.AddAsync(bet);

            user.Points = (int)((decimal)user.Points - dto.Amount);
            await _userRepository.UpdateAsync(user);

            race.TotalBets = (race.TotalBets ?? 0m) + 1m;
            race.TotalAmount = (race.TotalAmount ?? 0m) + dto.Amount;
            await _raceRepository.UpdateAsync(race);

            return MapBetToAdminDto(bet, user.UserName, race.Name, driver.Name);
        }

        /// <summary>
        /// Updates a bet (admin only). Supports partial updates.
        /// </summary>
        public async Task<AdminBetResponseDto> UpdateBetAsync(int betId, UpdateBetDto dto, int adminUserId)
        {
            var bet = await _betRepository.GetByIdAsync(betId);
            if (bet == null)
            {
                throw new BetNotFoundException(betId);
            }

            var user = await _userRepository.GetByIdAsync(bet.UserId);
            if (user == null)
            {
                throw new UserNotFoundException("User not found");
            }

            var race = await _raceRepository.GetByIdAsync(bet.RaceId);
            if (race == null)
            {
                throw new RaceNotFoundException(bet.RaceId);
            }

            var driver = await _driverRepository.GetByIdAsync(bet.DriverId);
            if (driver == null)
            {
                throw new DriverNotFoundException(bet.DriverId);
            }

            // Handle status change - if changing to Won or Lost, we need to handle winnings
            if (dto.Status.HasValue)
            {
                var oldStatus = bet.Status;
                bet.Status = dto.Status.Value;

                // If status is changing from Pending to Won or Lost (and winnings are specified)
                if (oldStatus == BetStatus.Pending && (dto.Status.Value == BetStatus.Won || dto.Status.Value == BetStatus.Lost))
                {
                    if (dto.Winnings.HasValue)
                    {
                        bet.Winnings = dto.Winnings.Value;
                        bet.ResolvedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        bet.ResolvedAt = DateTime.UtcNow;
                    }
                }
            }

            // Handle winnings override
            if (dto.Winnings.HasValue)
            {
                bet.Winnings = dto.Winnings.Value;
            }

            // Handle bet type change (may need to recalculate odds)
            if (dto.BetType.HasValue && dto.BetType.Value != bet.BetType)
            {
                bet.BetType = dto.BetType.Value;
            }

            // Handle driver change
            if (dto.DriverId.HasValue && dto.DriverId.Value != bet.DriverId)
            {
                var newDriver = await _driverRepository.GetByIdAsync(dto.DriverId.Value);
                if (newDriver == null)
                {
                    throw new DriverNotFoundException(dto.DriverId.Value);
                }

                // Refund the old bet amount to user
                user.Points = (int)((decimal)user.Points + bet.Amount);

                // Update the bet with new driver
                bet.DriverId = dto.DriverId.Value;

                // Recalculate odds
                decimal newOdds;
                try
                {
                    newOdds = race.OddsForDriver(bet.DriverId);
                }
                catch (NotImplementedException)
                {
                    newOdds = 1m;
                }
                bet.Odds = newOdds;
                bet.PotentialWinnings = bet.Amount * newOdds;

                driver = newDriver;
            }

            // Handle amount change
            if (dto.Amount.HasValue)
            {
                var oldAmount = bet.Amount;
                bet.Amount = dto.Amount.Value;

                // Adjust user points by the difference
                user.Points = (int)((decimal)user.Points - (bet.Amount - oldAmount));

                // Recalculate potential winnings
                bet.PotentialWinnings = bet.Amount * bet.Odds;
            }

            await _betRepository.UpdateAsync(bet);
            await _userRepository.UpdateAsync(user);

            return MapBetToAdminDto(bet, user.UserName, race.Name, driver.Name);
        }

        /// <summary>
        /// Deletes (cancels) a bet (admin only). Only works on pending bets.
        /// Refunds the bet amount to the user's balance.
        /// </summary>
        public async Task DeleteBetAsync(int betId, int adminUserId)
        {
            var bet = await _betRepository.GetByIdAsync(betId);
            if (bet == null)
            {
                throw new BetNotFoundException(betId);
            }

            if (bet.Status != BetStatus.Pending)
            {
                throw new InvalidOperationException("Cannot delete a bet that is not in Pending status.");
            }

            var user = await _userRepository.GetByIdAsync(bet.UserId);
            if (user == null)
            {
                throw new UserNotFoundException("User not found");
            }

            // Refund the bet amount to user
            user.Points = (int)((decimal)user.Points + bet.Amount);
            await _userRepository.UpdateAsync(user);

            // Update race stats
            var race = await _raceRepository.GetByIdAsync(bet.RaceId);
            if (race != null)
            {
                race.TotalBets = (race.TotalBets ?? 0m) - 1m;
                race.TotalAmount = (race.TotalAmount ?? 0m) - bet.Amount;
                await _raceRepository.UpdateAsync(race);
            }

            // Delete the bet
            await _betRepository.DeleteAsync(betId);
        }

        private static AdminBetResponseDto MapBetToAdminDto(Bet bet, string? username, string? raceName, string? driverName)
        {
            return new AdminBetResponseDto
            {
                Id = bet.Id,
                UserId = bet.UserId,
                Username = username ?? $"User {bet.UserId}",
                RaceId = bet.RaceId,
                RaceName = raceName ?? $"Race {bet.RaceId}",
                DriverId = bet.DriverId,
                DriverName = driverName ?? $"Driver {bet.DriverId}",
                Amount = bet.Amount,
                Odds = bet.Odds,
                BetType = bet.BetType,
                Status = bet.Status,
                Winnings = bet.Winnings,
                PotentialWinnings = bet.PotentialWinnings,
                CreatedAt = bet.CreatedAt,
                ResolvedAt = bet.ResolvedAt
            };
        }
    }
}

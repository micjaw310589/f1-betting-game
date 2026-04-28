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
using F1BettingApp.Infrastructure.Persistence.Repositories;
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

        /// <summary>
        /// Constructor for BettingService
        /// </summary>
        public BettingService(
            IBetRepositoryExtensions betRepository,
            IRepository<User> userRepository,
            IRaceRepositoryExtensions raceRepository,
            IRepository<Driver> driverRepository,
            IUserService userService,
            IRaceService raceService)
        {
            _betRepository = betRepository;
            _userRepository = userRepository;
            _raceRepository = raceRepository;
            _driverRepository = driverRepository;
            _userService = userService;
            _raceService = raceService;
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

            if (race.Status != RaceStatus.Scheduled && race.Status != RaceStatus.InProgress)
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
        /// </summary>
        /// <param name="raceId">Race identifier</param>
        public async Task ProcessRaceResultsAsync(int raceId)
        {
            // Get the race using RaceService for comprehensive validation
            var race = await _raceService.GetRaceByIdAsync(raceId);
            if (race == null)
            {
                throw new RaceNotFoundException(raceId);
            }

            // Verify race is finished
            if (race.Status != RaceStatus.Finished)
            {
                throw new InvalidOperationException("Race must be completed before processing results");
            }

            // Get all bets for this race
            var bets = await _betRepository.GetByRaceIdAsync(raceId);

            // Get race results - assuming this returns a list of (DriverId, Position) pairs
            var results = await _raceService.GetResultsAsync(raceId);

            // Update each bet
            foreach (var bet in bets)
            {
                // Find the bet's position in race results
                var position = results.FirstOrDefault(r => r.DriverId == bet.DriverId)?.Position;

                if (position != null)
                {
                    // Calculate winnings
                    decimal winnings = 0;
                    BetStatus newStatus;

                    if (bet.BetType == BetType.RaceWinner)
                    {
                        // Winner bet - only wins if first place
                        if (position == 1)
                        {
                            winnings = bet.Amount * bet.Odds;
                            newStatus = BetStatus.Won;
                        }
                        else
                        {
                            newStatus = BetStatus.Lost;
                        }
                    }
                    else if (bet.BetType == BetType.PodiumFinish)
                    {
                        // Podium finish bet - wins if position is 1, 2, or 3
                        if (position <= 3)
                        {
                            winnings = bet.Amount * bet.Odds;
                            newStatus = BetStatus.Won;
                        }
                        else
                        {
                            newStatus = BetStatus.Lost;
                        }
                    }
                    else if (bet.BetType == BetType.Top10Finish)
                    {
                        // Top 10 bet - wins if position is 1-10
                        if (position <= 10)
                        {
                            winnings = bet.Amount * bet.Odds;
                            newStatus = BetStatus.Won;
                        }
                        else
                        {
                            newStatus = BetStatus.Lost;
                        }
                    }
                    else if (bet.BetType == BetType.FastestLap)
                    {
                        // Fastest lap bet - wins if position is 0 (fastest)
                        if (position == 0)
                        {
                            winnings = bet.Amount * bet.Odds;
                            newStatus = BetStatus.Won;
                        }
                        else
                        {
                            newStatus = BetStatus.Lost;
                        }
                    }
                    else
                    {
                        // Default: treat as lost for other bet types
                        newStatus = BetStatus.Lost;
                    }

                    // Update bet
                    bet.Status = newStatus;
                    bet.PotentialWinnings = winnings;
                    bet.Winnings = winnings;
                    await _betRepository.UpdateAsync(bet);

                    // Add winnings to user balance
                    var user = await _userRepository.GetByIdAsync(bet.UserId);
                    if (user != null)
                    {
                        user.Points = (int)((decimal)user.Points + winnings);
                        await _userRepository.UpdateAsync(user);
                    }
                }
            }
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
                UserId = bet.UserId.ToString(),
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
    }
}
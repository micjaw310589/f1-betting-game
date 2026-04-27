using F1BettingApp.Application.Interfaces;
using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Exceptions;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Services
{
    /// <summary>
    /// Service for handling all betting-related operations
    /// </summary>
    public class BettingService : IBettingService
    {
        private readonly IRepository<Bet> _betRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Race> _raceRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IUserService _userService;

        /// <summary>
        /// Constructor for BettingService
        /// </summary>
        public BettingService(
            IRepository<Bet> betRepository,
            IRepository<User> userRepository,
            IRepository<Race> raceRepository,
            IRepository<Driver> driverRepository,
            IUserService userService)
        {
            _betRepository = betRepository;
            _userRepository = userRepository;
            _raceRepository = raceRepository;
            _driverRepository = driverRepository;
            _userService = userService;
        }

        /// <summary>
        /// Places a bet on a specific driver in a race
        /// </summary>
        public async Task PlaceBetAsync(int userId, int raceId, int driverId, decimal amount)
        {
            // Verify user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException("User not found");
            }

            // Verify user has sufficient funds
            if (user.Balance < amount)
            {
                throw new InsufficientFundsException(amount, user.Balance);
            }

            // Verify race exists
            var race = await _raceRepository.GetByIdAsync(raceId);
            if (race == null)
            {
                throw new RaceNotFoundException(raceId);
            }

            // Verify race is upcoming
            if (race.Status != RaceStatus.Scheduled)
            {
                throw new RaceNotUpcomingException();
            }

            // Verify race is not already completed
            if (race.Status == RaceStatus.Completed)
            {
                throw new RaceCompletedException();
            }

            // Verify driver exists
            var driver = await _driverRepository.GetByIdAsync(driverId);
            if (driver == null)
            {
                throw new DriverNotFoundException(driverId);
            }

            // Create the bet
            var bet = new Bet
            {
                UserId = userId,
                RaceId = raceId,
                DriverId = driverId,
                Amount = amount,
                BetType = BetType.RaceWinner,
                Odds = race.OddsForDriver(driverId),
                PotentialWinnings = amount * race.OddsForDriver(driverId),
                Status = BetStatus.Pending,
                CreatedAt = DateTime.Now
            };

            // Save the bet
            await _betRepository.AddAsync(bet);

            // Deduct funds from user
            user.Balance -= amount;
            await _userRepository.UpdateAsync(user);

            // Update race with bet information
            race.TotalBets++;
            race.TotalAmount += amount;
            await _raceRepository.UpdateAsync(race);
        }

        /// <summary>
        /// Cancels an existing bet
        /// </summary>
        public async Task CancelBetAsync(int betId)
        {
            // Find the bet
            var bet = await _betRepository.GetByIdAsync(betId);
            if (bet == null)
            {
                throw new BetNotFoundException(betId);
            }

            // Verify bet belongs to the current user
            var user = await _userRepository.GetByIdAsync(bet.UserId);
            if (user == null)
            {
                throw new UserNotFoundException("User not found");
            }

            // Verify race hasn't started yet
            var race = await _raceRepository.GetByIdAsync(bet.RaceId);
            if (race == null)
            {
                throw new RaceNotFoundException(bet.RaceId);
            }

            if (race.Status == RaceStatus.InProgress || race.Status == RaceStatus.Completed)
            {
                throw new RaceAlreadyStartedException();
            }

            // Update bet status
            bet.Status = BetStatus.Cancelled;
            await _betRepository.UpdateAsync(bet);

            // Refund the bet amount to user
            user.Balance += bet.Amount;
            await _userRepository.UpdateAsync(user);
        }

        /// <summary>
        /// Gets all bets for a specific user
        /// </summary>
        public async Task<IEnumerable<BetDto>> GetUserBetsAsync(int userId)
        {
            // Get all bets for the user
            var bets = await _betRepository.GetByUserIdAsync(userId);

            // Convert to DTOs
            return bets.Select(b => new BetDto
            {
                Id = b.Id,
                RaceId = b.RaceId,
                DriverId = b.DriverId,
                BetType = b.BetType,
                Amount = b.Amount,
                Odds = b.Odds,
                PotentialWinnings = b.PotentialWinnings,
                Status = b.Status,
                CreatedAt = b.CreatedAt
            });
        }

        /// <summary>
        /// Gets a specific bet by ID
        /// </summary>
        public async Task<BetDto?> GetBetByIdAsync(int betId)
        {
            var bet = await _betRepository.GetByIdAsync(betId);
            if (bet == null)
            {
                return null;
            }

            return new BetDto
            {
                Id = bet.Id,
                RaceId = bet.RaceId,
                DriverId = bet.DriverId,
                BetType = bet.BetType,
                Amount = bet.Amount,
                Odds = bet.Odds,
                PotentialWinnings = bet.PotentialWinnings,
                Status = bet.Status,
                CreatedAt = bet.CreatedAt
            };
        }

        /// <summary>
        /// Processes race results and updates bet statuses
        /// </summary>
        public async Task ProcessRaceResultsAsync(int raceId)
        {
            // Get the race
            var race = await _raceRepository.GetByIdAsync(raceId);
            if (race == null)
            {
                throw new RaceNotFoundException(raceId);
            }

            // Verify race is completed
            if (race.Status != RaceStatus.Completed)
            {
                throw new InvalidOperationException("Race must be completed before processing results");
            }

            // Get all bets for this race
            var bets = await _betRepository.GetByRaceIdAsync(raceId);

            // Get race results
            var results = await _raceRepository.GetResultsAsync(raceId);

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
                    else if (bet.BetType == BetType.Top3)
                    {
                        // Top 3 bet - wins if position is 1, 2, or 3
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
                    else if (bet.BetType == BetType.Place)
                    {
                        // Place bet - wins if position matches exactly
                        if (position == bet.PlacePosition)
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
                        user.Balance += winnings;
                        await _userRepository.UpdateAsync(user);
                    }
                }
            }
        }
    }
}
using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using System.Transactions;

namespace F1BettingApp.Application.Services
{
    public class BettingService : IBettingService
    {
        private readonly IRepository<Bet> _betRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Race> _raceRepository;
        private readonly IRepository<Result> _resultRepository;

        public BettingService(
            IRepository<Bet> betRepository,
            IRepository<User> userRepository,
            IRepository<Race> raceRepository,
            IRepository<Result> resultRepository)
        {
            _betRepository = betRepository;
            _userRepository = userRepository;
            _raceRepository = raceRepository;
            _resultRepository = resultRepository;
        }

        public async Task PlaceBetAsync(int userId, int raceId, int driverId, decimal amount)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Points < amount) throw new InvalidOperationException("Insufficient balance");

            var race = await _raceRepository.GetByIdAsync(raceId);
            if (race == null || !race.CanPlaceBets()) throw new InvalidOperationException("Race not available for betting");

            // Calculate odds based on bet type (simplified for example)
            decimal odds = CalculateOdds(BetType.RaceWinner); // Default to race winner

            var bet = new Bet(userId, raceId, driverId, amount, BetType.RaceWinner, odds);

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    await _betRepository.AddAsync(bet);

                    // Deduct balance
                    user.Points -= (int)amount;
                    await _userRepository.UpdateAsync(user);

                    await _betRepository.SaveChangesAsync();
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

        public async Task CancelBetAsync(int betId)
        {
            var bet = await _betRepository.GetByIdAsync(betId);
            if (bet == null || bet.Status != BetStatus.Pending) throw new InvalidOperationException("Cannot cancel bet");

            var user = await _userRepository.GetByIdAsync(bet.UserId);
            if (user == null) throw new InvalidOperationException("User not found");

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    bet.Status = BetStatus.Canceled;

                    // Refund the bet amount
                    user.Points += (int)bet.Amount;

                    await _betRepository.UpdateAsync(bet);
                    await _userRepository.UpdateAsync(user);

                    await _betRepository.SaveChangesAsync();
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

        public async Task<IEnumerable<BetDto>> GetUserBetsAsync(int userId)
        {
            var bets = await _betRepository.GetAllAsync();
            var userBets = bets.Where(b => b.UserId == userId);

            return userBets.Select(b => new BetDto
            {
                Id = b.Id,
                UserId = b.UserId,
                RaceId = b.RaceId,
                DriverId = b.DriverId,
                Amount = b.Amount,
                Status = b.Status,
                CreatedAt = b.CreatedAt
            });
        }

        public async Task ProcessRaceResultsAsync(int raceId)
        {
            var race = await _raceRepository.GetByIdAsync(raceId);
            if (race == null || !race.IsRaceFinished()) throw new InvalidOperationException("Race not finished or not found");

            var bets = (await _betRepository.GetAllAsync()).Where(b => b.RaceId == raceId && b.Status == BetStatus.Pending);
            var results = (await _resultRepository.GetAllAsync()).Where(r => r.RaceId == raceId);

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    foreach (var bet in bets)
                    {
                        var result = results.FirstOrDefault(r => r.DriverId == bet.DriverId);
                        bool betWon = IsBetWinning(bet, result);

                        bet.Status = betWon ? BetStatus.Won : BetStatus.Lost;
                        await _betRepository.UpdateAsync(bet);
                    }

                    await _betRepository.SaveChangesAsync();
                    transaction.Complete();
                }
                catch
                {
                    transaction.Dispose();
                    throw;
                }
            }
        }

        public async Task<decimal> CalculateWinningsAsync(int userId, int raceId)
        {
            var bets = (await _betRepository.GetAllAsync())
                .Where(b => b.UserId == userId && b.RaceId == raceId && b.Status == BetStatus.Won);

            decimal totalWinnings = 0;

            foreach (var bet in bets)
            {
                totalWinnings += bet.PotentialWinnings;
            }

            return totalWinnings;
        }

        private decimal CalculateOdds(BetType betType)
        {
            // Simplified odds calculation - in real app, this would be more sophisticated
            switch (betType)
            {
                case BetType.RaceWinner: return 2.5m;
                case BetType.PodiumFinish: return 1.8m;
                case BetType.Top10Finish: return 1.2m;
                case BetType.FastestLap: return 3.0m;
                case BetType.FastestPitStop: return 2.8m;
                case BetType.DNFCount: return 4.0m;
                case BetType.DriverVsDriver: return 2.0m;
                case BetType.TeamVsTeam: return 1.9m;
                default: return 1.0m;
            }
        }

        private bool IsBetWinning(Bet bet, Result result)
        {
            if (result == null) return false;

            switch (bet.BetType)
            {
                case BetType.RaceWinner: return result.Position == 1;
                case BetType.PodiumFinish: return result.Position <= 3;
                case BetType.Top10Finish: return result.Position <= 10;
                case BetType.FastestLap: return result.FastestLap.HasValue; // Simplified
                case BetType.FastestPitStop: return result.PitStopTime.HasValue; // Simplified
                case BetType.DNFCount: return result.Position == 0; // Assuming 0 means DNF
                // DriverVsDriver and TeamVsTeam would need more complex logic
                default: return false;
            }
        }
    }
}

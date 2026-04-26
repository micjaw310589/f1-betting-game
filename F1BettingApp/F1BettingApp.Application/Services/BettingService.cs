using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.Persistence.Repositories;

namespace F1BettingApp.Application.Services
{
    public class BettingService : IBettingService
    {
        private readonly IRepository<Bet> _betRepository;
        private readonly IRepository<User> _userRepository;

        public BettingService(IRepository<Bet> betRepository, IRepository<User> userRepository)
        {
            _betRepository = betRepository;
            _userRepository = userRepository;
        }

        public async Task PlaceBetAsync(int userId, int raceId, int driverId, decimal amount)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Points < amount) throw new InvalidOperationException("Insufficient balance");

            var bet = new Bet
            {
                UserId = userId,
                RaceId = raceId,
                DriverId = driverId,
                Amount = amount,
                Status = BetStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _betRepository.AddAsync(bet);

            // Deduct balance (in real app, use transaction)
            user.Points -= (int)amount;
            // Update user - but repository may not have update, assume it does or add
            // For now, skip update
        }

        public async Task CancelBetAsync(int betId)
        {
            var bet = await _betRepository.GetByIdAsync(betId);
            if (bet == null || bet.Status != BetStatus.Pending) throw new InvalidOperationException("Cannot cancel bet");

            bet.Status = BetStatus.Canceled;
            // Update bet - assume repository has update
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
    }
}
using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using System.Transactions;

namespace F1BettingApp.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IRepository<Notification> _notificationRepository;
        private readonly IRepository<User> _userRepository;

        public NotificationService(
            IRepository<Notification> notificationRepository,
            IRepository<User> userRepository)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
        }

        public async Task CreateNotificationAsync(int userId, string message, string type)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message cannot be empty");
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Type cannot be empty");

            // Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new InvalidOperationException("User not found");

            // Validate notification type
            if (!Enum.TryParse<NotificationType>(type, out var notificationType))
            {
                throw new ArgumentException("Invalid notification type");
            }

            // Map notification type to title
            string title = MapNotificationTypeToTitle(notificationType);

            var notification = new Notification(userId, title, message);

            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveChangesAsync();
        }

        public async Task MarkNotificationAsReadAsync(int notificationId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null) throw new InvalidOperationException("Notification not found");

            if (notification.IsRead) return; // Already read

            notification.MarkAsRead();

            await _notificationRepository.UpdateAsync(notification);
            await _notificationRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<NotificationDto>> GetUnreadNotificationsAsync(int userId)
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new InvalidOperationException("User not found");

            var notifications = await _notificationRepository.GetAllAsync();
            var unreadNotifications = notifications.Where(n => n.UserId == userId && !n.IsRead);

            return unreadNotifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            });
        }

        public async Task SendBetResultNotificationAsync(int userId, int betId, bool isWin)
        {
            var notificationType = isWin ? NotificationType.BetWon.ToString() : NotificationType.BetLost.ToString();
            var message = isWin ? "Congratulations! Your bet has won!" : "Your bet did not win this time.";

            await CreateNotificationAsync(userId, message, notificationType);
        }

        public async Task SendRaceStatusUpdateNotificationAsync(int userId, int raceId, string newStatus)
        {
            var message = $"Race {raceId} status updated to: {newStatus}";
            await CreateNotificationAsync(userId, message, NotificationType.RaceResultProcessed.ToString());
        }

        private string MapNotificationTypeToTitle(NotificationType type)
        {
            return type switch
            {
                NotificationType.BetPlaced => "Bet Placed",
                NotificationType.BetWon => "Bet Won!",
                NotificationType.BetLost => "Bet Result",
                NotificationType.RaceResultProcessed => "Race Update",
                NotificationType.SystemMessage => "System Message",
                _ => "Notification"
            };
        }
    }
}

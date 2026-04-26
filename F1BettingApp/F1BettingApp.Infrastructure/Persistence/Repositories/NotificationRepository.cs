using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Infrastructure.Persistence.Repositories;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        private readonly ILogger<NotificationRepository> _logger;

        public NotificationRepository(AppDbContext context, ILogger<NotificationRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<IQueryable<Notification>> GetUnreadNotificationsAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Getting unread notifications for user: {userId}");
                return _dbSet
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .OrderByDescending(n => n.CreatedAt)
                    .AsQueryable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting unread notifications for user: {userId}");
                throw;
            }
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            try
            {
                _logger.LogInformation($"Marking notification as read: {notificationId}");
                var notification = await _dbSet.FindAsync(notificationId);
                if (notification != null)
                {
                    notification.MarkAsRead();
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking notification as read: {notificationId}");
                throw;
            }
        }
    }
}
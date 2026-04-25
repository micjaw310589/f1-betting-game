using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using F1BettingApp.Domain.Entities;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context)
        {
        }

        public IQueryable<Notification> GetUnreadNotifications(int userId)
        {
            return _dbSet.Where(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await GetByIdAsync(notificationId);
            if (notification == null)
            {
                return;
            }

            notification.MarkAsRead();
            await SaveChangesAsync();
        }
    }
}

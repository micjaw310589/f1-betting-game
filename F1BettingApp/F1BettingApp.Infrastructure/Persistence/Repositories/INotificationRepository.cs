using System.Collections.Generic;
using System.Threading.Tasks;
using F1BettingApp.Domain.Entities;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    public interface INotificationRepository : IRepository<Notification>
    {
        IQueryable<Notification> GetUnreadNotifications(int userId);
        Task MarkAsReadAsync(int notificationId);
    }
}

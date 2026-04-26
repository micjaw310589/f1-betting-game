using F1BettingApp.Application.DTOs;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Interfaces
{
    /// <summary>
    /// Interface for notification-related operations
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Creates a new notification for a user
        /// </summary>
        /// <param name="userId">The ID of the user to notify</param>
        /// <param name="message">The notification message</param>
        /// <param name="type">The type of notification</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task CreateNotificationAsync(int userId, string message, string type);

        /// <summary>
        /// Marks a notification as read
        /// </summary>
        /// <param name="notificationId">The ID of the notification to mark as read</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task MarkNotificationAsReadAsync(int notificationId);

        /// <summary>
        /// Gets all unread notifications for a user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>Collection of unread notification DTOs</returns>
        Task<IEnumerable<NotificationDto>> GetUnreadNotificationsAsync(int userId);
    }
}
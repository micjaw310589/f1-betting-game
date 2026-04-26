using System;

namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// Data transfer object for notifications
    /// </summary>
    public class NotificationDto
    {
        /// <summary>
        /// Gets or sets the notification ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the notification title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the notification message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets whether the notification has been read
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}

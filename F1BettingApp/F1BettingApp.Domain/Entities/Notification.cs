using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F1BettingApp.Domain.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        
        // Foreign Keys
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        public Notification(int userId, string title, string message)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Invalid parameters provided for Notification.");
            }
            
            this.UserId = userId;
            this.Title = title;
            this.Message = message;
            this.IsRead = false;
            this.CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the notification as read.
        /// </summary>
        public void MarkAsRead()
        {
            this.IsRead = true;
        }
    }
}
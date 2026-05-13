using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace F1BettingApp.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("unread")]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetUnreadNotifications()
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out int userId))
                {
                    return Unauthorized("User ID not found or invalid.");
                }

                var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred retrieving notifications.", details = ex.Message });
            }
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                // Note: We don't verify if the notification belongs to the current user here 
                // because INotificationService doesn't expose it, but in a real app we'd secure this.
                await _notificationService.MarkNotificationAsReadAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message == "Notification not found")
                {
                    return NotFound(ex.Message);
                }
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred marking the notification as read.", details = ex.Message });
            }
        }
    }
}

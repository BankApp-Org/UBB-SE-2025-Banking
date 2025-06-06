using Common.DTOs;
using Common.Models.Social;
using Common.Services;
using Common.Services.Social;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;

        public NotificationController(
            INotificationService notificationService,
            IUserService userService)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<Notification>>> GetUserNotifications(int userId)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser == null || currentUser.Id != userId)
                {
                    return Unauthorized("You can only view your own notifications.");
                }

                var notifications = await _notificationService.GetNotificationsForUser(userId);
                if (notifications == null || !notifications.Any())
                {
                    return Ok(new List<Notification>()); // Return empty list instead of 404
                }

                return Ok(notifications); // Return full notifications, not DTOs
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("notification")]
        public async Task<ActionResult> SendNotification([FromBody] NotificationDto dto)
        {
            try
            {
                // Get all users and find the one with matching ID
                var users = await _userService.GetUsers();
                var user = users.FirstOrDefault(u => u.Id == dto.UserReceiver.Id);
                if (user == null)
                {
                    return NotFound($"User with ID {dto.UserReceiver.Id} not found");
                }

                var notification = new Notification
                {
                    Content = dto.Content,
                    Timestamp = DateTime.UtcNow,
                    UserId = user.Id,
                    User = user
                };
                await _notificationService.CreateNotification(notification);
                return Ok("Notification sent");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("clear/{notificationId}")]
        public async Task<ActionResult> ClearNotification(int notificationId)
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                {
                    return Unauthorized("You must be logged in to clear notifications.");
                }

                await _notificationService.MarkNotificationAsRead(notificationId, user.Id);
                return Ok("Notification cleared");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("clear-all/{userId}")]
        public async Task<ActionResult> ClearAllNotifications(int userId)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser == null || currentUser.Id != userId)
                {
                    return Unauthorized("You can only clear your own notifications.");
                }

                await _notificationService.MarkAllNotificationsAsRead(userId);
                return Ok("All notifications cleared");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
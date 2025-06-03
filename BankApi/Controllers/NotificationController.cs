using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Common.DTOs;
using Common.Models;
using Common.Models.Social;
using Common.Services;
using Common.Services.Social;

namespace LoanShark.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<Notification>>> GetUserNotifications(int userId)
        {
            var notifications = await _notificationService.GetNotificationsForUser(userId);
            if (notifications == null)
            {
                return NotFound("No notifications found");
            }

            var dtos = notifications.Select(n => new NotificationDto
            {
                NotificationID = n.NotificationID,
                Timestamp = n.Timestamp,
                Content = n.Content,
                UserReceiver = n.User
            }).ToList();

            return Ok(dtos);
        }

        [HttpPost("notification")]
        public async Task<ActionResult> SendNotification([FromBody] NotificationDto dto)
        {
            var notification = new Notification
            {
                Content = dto.Content,
                NotificationID = dto.NotificationID,
                Timestamp = dto.Timestamp,
                User = dto.UserReceiver,
                UserId = dto.UserReceiver.Id
            };
            await _notificationService.CreateNotification(notification);
            return Ok("Notification sent");
        }

        [HttpPost("clear")]
        public async Task<ActionResult> ClearNotification([FromBody] int notificationId)
        {
            var user = await _userService.GetCurrentUserAsync();
            await _notificationService.MarkNotificationAsRead(notificationId, user.Id);
            return Ok("Notification cleared");
        }

        [HttpPost("clear-all")]
        public async Task<ActionResult> ClearAllNotifications([FromBody] int userId)
        {
            await _notificationService.MarkAllNotificationsAsRead(userId);
            return Ok("All notifications cleared");
        }
    }
}
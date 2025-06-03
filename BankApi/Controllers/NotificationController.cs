using Microsoft.AspNetCore.Mvc;
using LoanShark.Service.SocialService.Implementations;
using LoanShark.Domain;
using System.Threading.Tasks;
using System.Collections.Generic;
using Common.Models.Social;
using Common.Services.Social;
using LoanShark.Service.SocialService.Interfaces;

namespace LoanShark.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<Notification>>> GetUserNotifications(int userId)
        {
            UserSession.Instance.SetUserData("id_user", "2"); // Hardcoded for now
            var notifications = await _notificationService.GetNotifications(userId);
            if (notifications == null)
            {
                return NotFound("No notifications found");
            }

            var dtos = notifications.Select(n => new NotificationViewModel
            {
                NotificationID = n.NotificationID,
                Timestamp = n.Timestamp,
                Content = n.Content,
                UserReceiverID = n.UserReceiverID,
            }).ToList();

            return Ok(dtos);
        }

        [HttpPost("friend")]
        public async Task<ActionResult> SendFriendNotification([FromBody] FriendNotificationDto dto)
        {
            await Task.Run(async () => await _notificationService.SendFriendNotification(dto.UserId, dto.NewFriendId));
            return Ok("Friend notification sent");
        }

        [HttpPost("remove-friend")]
        public async Task<ActionResult> SendRemoveFriendNotification([FromBody] FriendNotificationDto dto)
        {
            await Task.Run(async () => await _notificationService.SendRemoveFriendNotification(dto.UserId, dto.OldFriendId));
            return Ok("Remove friend notification sent");
        }

        [HttpPost("message")]
        public async Task<ActionResult> SendMessageNotification([FromBody] MessageNotificationDto dto)
        {
            await Task.Run(async () => await _notificationService.SendMessageNotification(dto.MessageSenderId, dto.ChatId));
            return Ok("Message notification sent");
        }

        [HttpPost("transaction")]
        public async Task<ActionResult> SendTransactionNotification([FromBody] TransactionNotificationDto dto)
        {
            await Task.Run(async () => await _notificationService.SendTransactionNotification(dto.ReceiverId, dto.ChatId, dto.Type, dto.Amount, dto.Currency));
            return Ok("Transaction notification sent");
        }

        [HttpPost("new-chat")]
        public async Task<ActionResult> SendNewChatNotification([FromBody] NewChatNotificationDto dto)
        {
            await Task.Run(async () => await _notificationService.SendNewChatNotification(dto.ChatId));
            return Ok("New chat notification sent");
        }

        [HttpPost("clear")]
        public async Task<ActionResult> ClearNotification([FromBody] int notificationId)
        {
            await Task.Run(async () => await _notificationService.ClearNotification(notificationId));
            return Ok("Notification cleared");
        }

        [HttpPost("clear-all")]
        public async Task<ActionResult> ClearAllNotifications([FromBody] int userId)
        {
            await Task.Run(async () => await _notificationService.ClearAllNotifications(userId));
            return Ok("All notifications cleared");
        }
    }
}
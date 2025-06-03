using Common.DTOs;
using Common.Models;
using Common.Models.Social;
using Common.Services;
using Common.Services.Social;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;

        public MessageController(INotificationService notificationService, IUserService userService, IMessageService messageService)
        {
            _messageService = messageService;
            _notificationService = notificationService;
            _userService = userService;
        }

        [HttpPost("text")]
        public async Task<ActionResult> SendTextMessage([FromBody] TextMessageDto messageDto)
        {
            var user = await _userService.GetCurrentUserAsync();
            if (messageDto == null || string.IsNullOrEmpty(messageDto.Content))
                return BadRequest("Message content is required.");
            TextMessage message = new TextMessage
            {
                UserId = user.Id,
                ChatId = messageDto.ChatID,
                MessageContent = messageDto.Content,
                CreatedAt = DateTime.Now,
                Type = MessageType.Text,
                UsersReport = messageDto.UsersReport ?? new List<User>()
            };
            await _messageService.SendMessageAsync(messageDto.ChatID, user, message);
            return NoContent();
        }

        [HttpPost("image")]
        public async Task<ActionResult> SendImageMessage([FromBody] ImageMessageDto messageDto)
        {
            var user = await _userService.GetCurrentUserAsync();
            if (messageDto == null || string.IsNullOrEmpty(messageDto.ImageURL))
                return BadRequest("Image URL is required.");
            ImageMessage message = new ImageMessage
            {
                UserId = user.Id,
                ChatId = messageDto.ChatID,
                ImageUrl = messageDto.ImageURL,
                CreatedAt = DateTime.Now,
                Type = MessageType.Image,
                UsersReport = messageDto.UsersReport ?? new List<User>()
            };
            await _messageService.SendMessageAsync(messageDto.ChatID, user, message);
            return NoContent();
        }

        [HttpPost("transfer")]
        public async Task<ActionResult> SendTransferMessage([FromBody] TransferMessageDto messageDto)
        {
            var user = await _userService.GetCurrentUserAsync();
            if (messageDto == null || string.IsNullOrEmpty(messageDto.Description) || string.IsNullOrEmpty(messageDto.Currency))
                return BadRequest("Transfer details are required.");

            TransferMessage message = new TransferMessage
            {
                UserId = user.Id,
                ChatId = messageDto.ChatID,
                CreatedAt = DateTime.Now,
                Status = messageDto.Status,
                Amount = messageDto.Amount,
                Description = messageDto.Description,
                Currency = messageDto.Currency,
                Type = MessageType.Transfer,
                ListOfReceivers = messageDto.ListOfReceivers
            };
            await _messageService.SendMessageAsync(messageDto.ChatID, user, message);
            return NoContent();
        }

        [HttpPost("request")]
        public async Task<ActionResult> SendRequestMessage([FromBody] RequestMessageDto messageDto)
        {
            var user = await _userService.GetCurrentUserAsync();
            if (messageDto == null || string.IsNullOrEmpty(messageDto.Description) || string.IsNullOrEmpty(messageDto.Currency))
                return BadRequest("Request details are required.");

            RequestMessage message = new RequestMessage
            {
                UserId = user.Id,
                ChatId = messageDto.ChatID,
                CreatedAt = DateTime.Now,
                Status = messageDto.Status,
                Amount = messageDto.Amount,
                Description = messageDto.Description,
                Currency = messageDto.Currency,
                Type = MessageType.Request,
            };
            await _messageService.SendMessageAsync(messageDto.ChatID, user, message);
            return NoContent();
        }

        [HttpPost("delete")]
        public async Task<ActionResult> DeleteMessage([FromBody] MessageDto messageDto)
        {
            var user = await _userService.GetCurrentUserAsync();
            if (messageDto == null)
                return BadRequest("Message data is required.");

            await _messageService.DeleteMessageAsync(messageDto.ChatID, messageDto.MessageID, user);
            return NoContent();
        }

        [HttpPost("report")]
        public async Task<ActionResult> ReportMessage([FromBody] MessageDto messageDto, ReportReason reportReason)
        {
            var user = await _userService.GetCurrentUserAsync();
            if (messageDto == null)
                return BadRequest("Message data is required.");

            await _messageService.ReportMessage(messageDto.ChatID, messageDto.MessageID, user, reportReason);
            return NoContent();
        }
    }
}
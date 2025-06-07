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
    public class ChatController : ControllerBase
    {
        private readonly IChatService chatService;
        private readonly IMessageService messageService;
        private readonly IUserService userService;

        public ChatController(IChatService chatService, IMessageService messageService, IUserService userService)
        {
            this.chatService = chatService;
            this.messageService = messageService;
            this.userService = userService;
        }

        [HttpGet("{chatId}/participants/count")]
        public async Task<ActionResult<int>> GetNumberOfParticipants(int chatId)
        {
            return Ok(await chatService.GetNumberOfParticipants(chatId));
        }

        //[HttpPost("request-money")]
        //public async Task<IActionResult> RequestMoney([FromBody] RequestMoneyDto dto)
        //{
        //    try
        //    {
        //        await chatService.RequestMoneyViaChat(dto.Amount, dto.Currency, dto.ChatID, dto.Description);
        //        return Ok("Request sent.");
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[HttpPost("send-money")]
        //public async Task<IActionResult> SendMoney([FromBody] SendMoneyDto dto)
        //{
        //    try
        //    {
        //        await chatService.SendMoneyViaChat(dto.Amount, dto.Currency, dto.Description, dto.ChatID);
        //        return Ok("Transfer processed.");
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[HttpPost("accept-request")]
        //public async Task<IActionResult> AcceptRequest([FromBody] AcceptRequestDto dto)
        //{
        //    await chatService.AcceptRequestViaChat(dto.Amount, dto.Currency, dto.AccepterID, dto.RequesterID, dto.ChatID);
        //    return Ok("Request handled.");
        //}

        [HttpPost("create-chat")]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatDto dto)
        {
            Chat newchat = new Chat
            {
                ChatName = dto.ChatName,
                Users = dto.Participants,
                Messages = []
            };
            await chatService.CreateChat(newchat);
            return Ok("Chat created.");
        }

        [HttpDelete("{chatId}")]
        public async Task<IActionResult> DeleteChat(int chatId)
        {
            await chatService.DeleteChat(chatId);
            return Ok("Chat deleted.");
        }

        //[HttpGet("{chatId}/last-message-time")]
        //public async Task<ActionResult<DateTime>> GetLastMessageTimestamp(int chatId)
        //{
        //    return Ok(await chatService.GetLastMessageTimeStamp(chatId));
        //}

        [HttpGet("{chatId}/history")]
        public async Task<IActionResult> GetChatHistory(int chatId)
        {
            var messages = await chatService.GetChatById(chatId);
            var dtos = messages.Messages.Select(m =>
            {
                var messageType = m.Type;

                MessageDto dto = messageType.ToString() switch
                {
                    "Text" => new TextMessageDto
                    {
                        MessageID = m.Id,
                        SenderID = m.UserId,
                        ChatID = m.ChatId,
                        Timestamp = m.CreatedAt.ToString("O"),
                        SenderUsername = m.Sender.FirstName,
                        Type = messageType.ToString(),
                        Content = ((TextMessage)m).MessageContent,
                        UsersReport = ((TextMessage)m).UsersReport
                    },
                    "Image" => new ImageMessageDto
                    {
                        MessageID = m.Id,
                        SenderID = m.UserId,
                        ChatID = m.ChatId,
                        Timestamp = m.CreatedAt.ToString("O"),
                        SenderUsername = m.Sender.FirstName,
                        Type = messageType.ToString(),
                        ImageURL = ((ImageMessage)m).ImageUrl,
                        UsersReport = ((ImageMessage)m).UsersReport
                    },
                    "Transfer" => new TransferMessageDto
                    {
                        MessageID = m.Id,
                        SenderID = m.UserId,
                        ChatID = m.ChatId,
                        Timestamp = m.CreatedAt.ToString("O"),
                        SenderUsername = m.Sender.FirstName,
                        Type = messageType.ToString(),
                        Status = ((TransferMessage)m).Status,
                        Amount = ((TransferMessage)m).Amount,
                        Description = ((TransferMessage)m).Description,
                        Currency = ((TransferMessage)m).Currency,
                        ListOfReceivers = ((TransferMessage)m).ListOfReceivers
                    },
                    "Request" => new RequestMessageDto
                    {
                        MessageID = m.Id,
                        SenderID = m.UserId,
                        ChatID = m.ChatId,
                        Timestamp = m.CreatedAt.ToString("O"),
                        SenderUsername = m.Sender.FirstName,
                        Type = messageType.ToString(),
                        Status = ((RequestMessage)m).Status,
                        Amount = ((RequestMessage)m).Amount,
                        Description = ((RequestMessage)m).Description,
                        Currency = ((RequestMessage)m).Currency
                    },
                    "BillSplit" => new BillSplitMessageDto
                    {
                        MessageID = m.Id,
                        SenderID = m.UserId,
                        ChatID = m.ChatId,
                        Timestamp = m.CreatedAt.ToString("O"),
                        SenderUsername = m.Sender.FirstName,
                        Type = messageType.ToString(),
                        Description = ((BillSplitMessage)m).Description,
                        TotalAmount = ((BillSplitMessage)m).TotalAmount,
                        Currency = ((BillSplitMessage)m).Currency,
                        Participants = ((BillSplitMessage)m).Participants,
                        Status = ((BillSplitMessage)m).Status
                    },
                    _ => throw new InvalidOperationException($"Unknown message type: {messageType}")
                };
                return dto;
            }).ToList();

            //var options = new JsonSerializerOptions
            //{
            //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            //    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            //    ReferenceHandler = ReferenceHandler.Preserve,
            //};

            return Ok(dtos);
        }

        [HttpPost("{chatId}/add-user/{userCNP}")]
        public async Task<IActionResult> AddUserToChat(int chatId, string userCNP)
        {
            var user = await userService.GetUserByCnpAsync(userCNP);
            await chatService.AddUserToChat(chatId, user);
            return Ok("User added to chat.");
        }

        [HttpDelete("{chatId}/remove-user/{userCNP}")]
        public async Task<IActionResult> RemoveUserFromChat(int chatId, string userCNP)
        {
            var user = await userService.GetUserByCnpAsync(userCNP);
            await chatService.RemoveUserFromChat(chatId, user);
            return Ok("User removed from chat.");
        }

        [HttpGet("{chatId}/name")]
        public async Task<ActionResult<string>> GetChatNameById(int chatId)
        {
            try
            {
                var chat = await chatService.GetChatById(chatId);
                return Ok(chat.ChatName);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{chatId}/participants/usernames")]
        public async Task<ActionResult<List<string>>> GetParticipantUsernames(int chatId)
        {
            var chat = await chatService.GetChatById(chatId);
            return Ok(chat.Users.Select(u => u.FirstName).ToList());
        }

        [HttpGet("{chatId}")]
        public async Task<ActionResult<Chat>> GetChatByIdAsync(int chatId)
        {
            var chat = await chatService.GetChatById(chatId);
            return Ok(chat);
        }

        [HttpGet("{chatId}/participants")]
        public async Task<ActionResult<List<User>>> GetParticipants(int chatId)
        {
            var chat = await chatService.GetChatById(chatId);
            var friends = chat.Users;
            var dtos = friends.Select(f => new SocialUserDto
            {
                UserID = f.Id,
                Username = f.FirstName,
                FirstName = f.FirstName,
                LastName = f.LastName,
                Email = f.Email?.ToString(),
                Cnp = f.CNP?.ToString(),
                ReportedCount = f.ReportedCount
            }).ToList();
            return Ok(dtos);
        }        // Message-related endpoints (consolidated from MessageController)
        [HttpPost("{chatId}/messages")]
        public async Task<ActionResult> SendMessage(int chatId, [FromBody] MessageDto messageDto)
        {
            var user = await userService.GetCurrentUserAsync();
            if (messageDto == null)
                return BadRequest("Message data is required.");

            Message message = messageDto switch
            {
                TextMessageDto textDto => new TextMessage
                {
                    UserId = user.Id,
                    ChatId = chatId,
                    MessageContent = textDto.Content ?? throw new ArgumentException("Message content is required."),
                    CreatedAt = DateTime.Now,
                    Type = MessageType.Text.ToString(),
                    MessageType = MessageType.Text,
                    UsersReport = textDto.UsersReport ?? []
                },
                ImageMessageDto imageDto => new ImageMessage
                {
                    UserId = user.Id,
                    ChatId = chatId,
                    ImageUrl = imageDto.ImageURL ?? throw new ArgumentException("Image URL is required."),
                    CreatedAt = DateTime.Now,
                    Type = MessageType.Image.ToString(),
                    MessageType = MessageType.Image,
                    UsersReport = imageDto.UsersReport ?? []
                },
                TransferMessageDto transferDto => new TransferMessage
                {
                    UserId = user.Id,
                    ChatId = chatId,
                    CreatedAt = DateTime.Now,
                    Status = transferDto.Status ?? throw new ArgumentException("Transfer status is required."),
                    Amount = transferDto.Amount,
                    Description = transferDto.Description ?? throw new ArgumentException("Transfer description is required."),
                    Currency = transferDto.Currency,
                    Type = MessageType.Transfer.ToString(),
                    MessageType = MessageType.Transfer,
                    ListOfReceivers = transferDto.ListOfReceivers ?? []
                },
                RequestMessageDto requestDto => new RequestMessage
                {
                    UserId = user.Id,
                    ChatId = chatId,
                    CreatedAt = DateTime.Now,
                    Status = requestDto.Status ?? throw new ArgumentException("Request status is required."),
                    Amount = requestDto.Amount,
                    Description = requestDto.Description ?? throw new ArgumentException("Request description is required."),
                    Currency = requestDto.Currency,
                    Type = MessageType.Request.ToString(),
                    MessageType = MessageType.Request
                },
                BillSplitMessageDto billSplitDto => new BillSplitMessage
                {
                    UserId = user.Id,
                    ChatId = chatId,
                    CreatedAt = DateTime.Now,
                    Description = billSplitDto.Description ?? throw new ArgumentException("Bill description is required."),
                    TotalAmount = billSplitDto.TotalAmount,
                    Currency = billSplitDto.Currency,
                    Participants = billSplitDto.Participants ?? [],
                    Status = billSplitDto.Status ?? "Pending",
                    Type = MessageType.BillSplit.ToString(),
                    MessageType = MessageType.BillSplit
                },
                _ => throw new ArgumentException($"Unsupported message type: {messageDto.GetType().Name}")
            };

            try
            {
                return Ok(await messageService.SendMessageAsync(chatId, user, message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{chatId}/messages/{messageId}")]
        public async Task<ActionResult> DeleteMessage(int chatId, int messageId)
        {
            var user = await userService.GetCurrentUserAsync();
            await messageService.DeleteMessageAsync(chatId, messageId, user);
            return NoContent();
        }
        [HttpPost("{chatId}/messages/{messageId}/report")]
        public async Task<ActionResult> ReportMessage(int chatId, int messageId, [FromBody] MessageReportRequest request)
        {
            var user = await userService.GetCurrentUserAsync();
            if (request == null)
                return BadRequest("Report data is required.");

            await messageService.ReportMessage(chatId, messageId, user, request.ReportReason);
            return NoContent();
        }
    }

    public class MessageReportRequest
    {
        public ReportReason ReportReason { get; set; }
    }
}
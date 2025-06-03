using BankApi.JSONConverters;
using Common.DTOs;
using Common.Models;
using Common.Models.Social;
using Common.Services;
using Common.Services.Social;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService chatService;
        private readonly IMessageService messageService;
        private readonly IUserService userService;

        public ChatController(IChatService chatService, IMessageService messageService)
        {
            this.chatService = chatService;
            this.messageService = messageService;
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
                Messages = new List<Message>()
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
                        MessageType = messageType.ToString(),
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
                        MessageType = messageType.ToString(),
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
                        MessageType = messageType.ToString(),
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
                        MessageType = messageType.ToString(),
                        Status = ((RequestMessage)m).Status,
                        Amount = ((RequestMessage)m).Amount,
                        Description = ((RequestMessage)m).Description,
                        Currency = ((RequestMessage)m).Currency
                    },
                    _ => throw new InvalidOperationException($"Unknown message type: {messageType}")
                };
                return dto;
            }).ToList();

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.Preserve,
                Converters = { new MessageDtoConverter() }
            };

            string json = JsonSerializer.Serialize(dtos, options);
            return Content(json, "application/json");
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
        }
    }
}
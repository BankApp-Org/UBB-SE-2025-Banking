using Microsoft.AspNetCore.Mvc;
using LoanShark.Service.SocialService.Implementations;
using LoanShark.Domain;
using System.Collections.Generic;
using LoanShark.Domain.MessageClasses;
using LoanShark.API.Models;
using LoanShark.Service.SocialService.Interfaces;
using System.Text.Json.Serialization;
using System.Text.Json;
using Common.Services.Social;
using LoanShark.API.JSONConverters;

namespace LoanShark.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService chatService;
        private readonly IMessageService messageService;

        public ChatController(IChatService chatService, IMessageService messageService)
        {
            this.chatService = chatService;
            this.messageService = messageService;
        }

        [HttpGet("current-user-id")]
        public ActionResult<int> GetCurrentUserID()
        {
            return Ok(chatService.GetCurrentUserID());
        }

        [HttpGet("{chatId}/participants/count")]
        public async Task<ActionResult<int>> GetNumberOfParticipants(int chatId)
        {
            return Ok(await chatService.GetNumberOfParticipants(chatId));
        }

        [HttpPost("request-money")]
        public async Task<IActionResult> RequestMoney([FromBody] RequestMoneyDto dto)
        {
            try
            {
                await chatService.RequestMoneyViaChat(dto.Amount, dto.Currency, dto.ChatID, dto.Description);
                return Ok("Request sent.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("send-money")]
        public async Task<IActionResult> SendMoney([FromBody] SendMoneyDto dto)
        {
            try
            {
                await chatService.SendMoneyViaChat(dto.Amount, dto.Currency, dto.Description, dto.ChatID);
                return Ok("Transfer processed.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("accept-request")]
        public async Task<IActionResult> AcceptRequest([FromBody] AcceptRequestDto dto)
        {
            await chatService.AcceptRequestViaChat(dto.Amount, dto.Currency, dto.AccepterID, dto.RequesterID, dto.ChatID);
            return Ok("Request handled.");
        }

        [HttpPost("create-chat")]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatDto dto)
        {
            await chatService.CreateChat(dto.ParticipantsID, dto.ChatName);
            return Ok("Chat created.");
        }

        [HttpDelete("{chatId}")]
        public async Task<IActionResult> DeleteChat(int chatId)
        {
            await chatService.DeleteChat(chatId);
            return Ok("Chat deleted.");
        }

        [HttpGet("{chatId}/last-message-time")]
        public async Task<ActionResult<DateTime>> GetLastMessageTimestamp(int chatId)
        {
            return Ok(await chatService.GetLastMessageTimeStamp(chatId));
        }

        [HttpGet("{chatId}/history")]
        public async Task<IActionResult> GetChatHistory(int chatId)
        {
            var messages = await chatService.GetChatHistory(chatId);
            var dtosTasks = messages.Select(async m =>
            {
                var messageType = await this.messageService.GetMessageTypeByMessageId(m.MessageID);

                MessageViewModel viewModel = messageType.ToString() switch
                {
                    "Text" => new TextMessageViewModel
                    {
                        MessageID = m.MessageID,
                        SenderID = m.SenderID,
                        ChatID = m.ChatID,
                        Timestamp = m.Timestamp.ToString("O"),
                        SenderUsername = m.SenderUsername,
                        MessageType = messageType.ToString(),
                        Content = ((TextMessage)m).Content,
                        UsersReport = ((TextMessage)m).UsersReport
                    },
                    "Image" => new ImageMessageViewModel
                    {
                        MessageID = m.MessageID,
                        SenderID = m.SenderID,
                        ChatID = m.ChatID,
                        Timestamp = m.Timestamp.ToString("O"),
                        SenderUsername = m.SenderUsername,
                        MessageType = messageType.ToString(),
                        ImageURL = ((ImageMessage)m).ImageURL,
                        UsersReport = ((ImageMessage)m).UsersReport
                    },
                    "Transfer" => new TransferMessageViewModel
                    {
                        MessageID = m.MessageID,
                        SenderID = m.SenderID,
                        ChatID = m.ChatID,
                        Timestamp = m.Timestamp.ToString("O"),
                        SenderUsername = m.SenderUsername,
                        MessageType = messageType.ToString(),
                        Status = ((TransferMessage)m).Status,
                        Amount = ((TransferMessage)m).Amount,
                        Description = ((TransferMessage)m).Description,
                        Currency = ((TransferMessage)m).Currency,
                        ListOfReceiversID = ((TransferMessage)m).ListOfReceiversID
                    },
                    "Request" => new RequestMessageViewModel
                    {
                        MessageID = m.MessageID,
                        SenderID = m.SenderID,
                        ChatID = m.ChatID,
                        Timestamp = m.Timestamp.ToString("O"),
                        SenderUsername = m.SenderUsername,
                        MessageType = messageType.ToString(),
                        Status = ((RequestMessage)m).Status,
                        Amount = ((RequestMessage)m).Amount,
                        Description = ((RequestMessage)m).Description,
                        Currency = ((RequestMessage)m).Currency
                    },
                    _ => throw new InvalidOperationException($"Unknown message type: {messageType}")
                };
                return viewModel;
            }).ToList();

            var dtos = await Task.WhenAll(dtosTasks);

            //// Manually serialize using runtime types
            //var options = new JsonSerializerOptions
            //{
            //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            //    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            //    WriteIndented = true
            //};

            //string json = JsonSerializer.Serialize(dtos, options);
            //return Content(json, "application/json");

            //return Ok(dtos);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new MessageViewModelConverter() }
            };

            string json = JsonSerializer.Serialize(dtos, options);
            return Content(json, "application/json");
        }

        [HttpPost("{chatId}/add-user/{userId}")]
        public async Task<IActionResult> AddUserToChat(int chatId, int userId)
        {
            await chatService.AddUserToChat(userId, chatId);
            return Ok("User added to chat.");
        }

        [HttpDelete("{chatId}/remove-user/{userId}")]
        public async Task<IActionResult> RemoveUserFromChat(int chatId, int userId)
        {
            await chatService.RemoveUserFromChat(userId, chatId);
            return Ok("User removed from chat.");
        }

        [HttpGet("{chatId}/name")]
        public async Task<ActionResult<string>> GetChatNameById(int chatId)
        {
            try
            {
                return Ok(await chatService.GetChatNameByID(chatId));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{chatId}/participants/usernames")]
        public async Task<ActionResult<List<string>>> GetParticipantUsernames(int chatId)
        {
            return Ok(await chatService.GetChatParticipantsStringList(chatId));
        }

        [HttpGet("{chatId}/participants")]
        public async Task<ActionResult<List<User>>> GetParticipants(int chatId)
        {
            var friends = await chatService.GetChatParticipantsList(chatId);
            var dtos = friends.Select(f => new SocialUserViewModel
            {
                UserID = f.UserID,
                Username = f.Username,
                FirstName = f.FirstName,
                LastName = f.LastName,
                Email = f.Email?.ToString(),
                PhoneNumber = f.PhoneNumber?.ToString(),
                Cnp = f.Cnp?.ToString(),
                HashedPassword = f.HashedPassword?.ToString(),
                ReportedCount = f.ReportedCount
            }).ToList();
            return Ok(dtos);
        }
    }
}
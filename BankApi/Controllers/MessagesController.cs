using BankApi.Repositories;
using BankApi.Repositories.Impl.Social;
using Common.DTOs;
using Common.Models;
using Common.Models.Social;
using Common.Services;
using Common.Services.Social;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messagesService;
        private readonly IUserRepository _userRepository;
        private readonly MessagesRepository _messagesRepository;
        private readonly IUserService _userService;

        public MessagesController(
            IMessageService messagesService,
            IUserRepository userRepository,
            MessagesRepository messagesRepository,
            IUserService userService)
        {
            _messagesService = messagesService ?? throw new ArgumentNullException(nameof(messagesService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _messagesRepository = messagesRepository ?? throw new ArgumentNullException(nameof(messagesRepository));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        private async Task<string> GetCurrentUserCnp()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }
            var user = await _userRepository.GetByIdAsync(int.Parse(userId));
            return user == null ? throw new Exception("User not found") : user.CNP;
        }
        private int GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
            {
                throw new UnauthorizedAccessException("User is not authenticated or has invalid ID.");
            }
            return id;
        }

        [HttpPost("user/{userCnp}/give")]
        [Authorize(Roles = "Admin")] // Or any other appropriate authorization
        public async Task<IActionResult> GiveMessageToUser(string userCnp, [FromBody] MessageRequest request)
        {
            try
            {
                throw new NotImplementedException("This method should be removed.");
                // await _messagesService.GiveMessageToUserAsync(userCnp, request.Type, request.MessageText);
                return Ok($"Message sent to user {userCnp}.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user")]
        public async Task<ActionResult<List<Message>>> GetMessagesForCurrentUser()
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                throw new NotImplementedException("This method should be removed.");
                // var messages = await _messagesService.GetMessagesForUserAsync(userCnp);
                // return Ok(messages);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user/{userCnp}")]
        [Authorize(Roles = "Admin")] // Only admins can get messages for other users
        public async Task<ActionResult<List<Message>>> GetMessagesForUser(string userCnp)
        {
            try
            {
                throw new NotImplementedException("This method should be removed.");
                // var messages = await _messagesService.GetMessagesForUserAsync(userCnp);
                // return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<Message>>> GetAllMessages()
        {
            try
            {
                var messages = await _messagesRepository.GetAllMessagesAsync();
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{messageId}")]
        [Authorize]
        public async Task<ActionResult<Message>> GetMessageById(int messageId)
        {
            try
            {
                var message = await _messagesService.GetMessageByIdAsync(messageId);
                return Ok(message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{chatId}")]
        [Authorize]
        public async Task<ActionResult<List<Message>>> GetMessagesByChatId(int chatId, [FromQuery] int page = 0, [FromQuery] int pageSize = 20)
        {
            try
            {
                var messages = await _messagesService.GetMessagesAsync(chatId, page, pageSize);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class MessageRequest
    {
        public string Type { get; set; } = string.Empty;
        public string MessageText { get; set; } = string.Empty;
    }
}

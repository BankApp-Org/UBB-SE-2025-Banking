using BankApi.Repositories;
using Common.Models;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController(IMessagesService messagesService, IUserRepository userRepository) : ControllerBase
    {
        private readonly IMessagesService _messagesService = messagesService ?? throw new ArgumentNullException(nameof(messagesService));
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

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

        [HttpPost("user/{userCnp}/give")]
        [Authorize(Roles = "Admin")] // Or any other appropriate authorization
        public async Task<IActionResult> GiveMessageToUser(string userCnp, [FromBody] MessageRequest request)
        {
            try
            {
                await _messagesService.GiveMessageToUserAsync(userCnp, request.Type, request.MessageText);
                return Ok($"Attempted to give a message to user {userCnp}.");
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
                var messages = await _messagesService.GetMessagesForUserAsync(userCnp);
                return Ok(messages);
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
                var messages = await _messagesService.GetMessagesForUserAsync(userCnp);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public class MessageRequest
        {
            public string Type { get; set; } = string.Empty;
            public string MessageText { get; set; } = string.Empty;
        }
    }
}

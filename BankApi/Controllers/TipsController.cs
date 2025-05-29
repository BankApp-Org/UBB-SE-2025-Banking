using BankApi.Repositories;
using Common.Models;
using Common.Services.Social;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TipsController(ITipsService tipsService, IUserRepository userRepository) : ControllerBase
    {
        private readonly ITipsService _tipsService = tipsService ?? throw new ArgumentNullException(nameof(tipsService));
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
        [Authorize(Roles = "Admin")] // Only admins can give tips to users
        public async Task<IActionResult> GiveTipToUser(string userCnp)
        {
            try
            {
                await _tipsService.GiveTipToUserAsync(userCnp);
                return Ok($"Tip successfully given to user {userCnp}.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user")]
        public async Task<ActionResult<List<Tip>>> GetTipsForCurrentUser()
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                var tips = await _tipsService.GetTipsForUserAsync(userCnp);
                return Ok(tips);
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
        [Authorize(Roles = "Admin")] // Only admins can get tips for other users
        public async Task<ActionResult<List<Tip>>> GetTipsForUser(string userCnp)
        {
            try
            {
                var tips = await _tipsService.GetTipsForUserAsync(userCnp);
                return Ok(tips);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
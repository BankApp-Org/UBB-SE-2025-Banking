using BankApi.Repositories;
using Common.Exceptions;
using Common.Models;
using Common.Models.Trading;
using Common.Services.Trading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StoreController(IStoreService storeService, IUserRepository userRepository) : ControllerBase
    {
        private readonly IStoreService _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
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

        [HttpGet("user-gem-balance")]
        public async Task<ActionResult<int>> GetUserGemBalance()
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                var balance = await _storeService.GetUserGemBalanceAsync(userCnp);
                return Ok(balance);
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

        [HttpPut("user-gem-balance")]
        [Authorize(Roles = "Admin")] // Only admins can directly update gem balances
        public async Task<IActionResult> UpdateUserGemBalance([FromBody] UpdateGemBalanceDto dto)
        {
            try
            {
                await _storeService.UpdateUserGemBalanceAsync(dto.NewBalance, dto.UserCnp);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("buy-gems")]
        public async Task<ActionResult<string>> BuyGems([FromBody] GemDealDto dealDto)
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                // Use the GemDeal constructor instead of object initializer
                var deal = new GemDeal(dealDto.Title, dealDto.GemAmount, dealDto.Price);
                var result = await _storeService.BuyGems(deal, dealDto.SelectedAccountId, userCnp);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (GemTransactionFailedException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("sell-gems")]
        public async Task<ActionResult<string>> SellGems([FromBody] SellGemsDto dto)
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                var result = await _storeService.SellGems(dto.GemAmount, dto.SelectedAccountId, userCnp);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InsufficientGemsException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (GemTransactionFailedException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class UpdateGemBalanceDto
    {
        public string UserCnp { get; set; }
        public int NewBalance { get; set; }
    }

    public class GemDealDto
    {
        public string Title { get; set; } // Added Title property
        public int GemAmount { get; set; }
        public double Price { get; set; }
        public string SelectedAccountId { get; set; }
    }

    public class SellGemsDto
    {
        public int GemAmount { get; set; }
        public string SelectedAccountId { get; set; }
    }
}

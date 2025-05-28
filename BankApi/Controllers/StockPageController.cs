using BankApi.Repositories;
using Common.Models;
using Common.Models.Trading;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StockPageController(IStockPageService stockPageService, IUserRepository userRepository) : ControllerBase
    {
        private readonly IStockPageService _stockPageService = stockPageService ?? throw new ArgumentNullException(nameof(stockPageService));
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

        [HttpGet("history/{stockName}")]
        public async Task<ActionResult<List<int>>> GetStockHistory(string stockName)
        {
            try
            {
                var history = await _stockPageService.GetStockHistoryAsync(stockName);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("owned-stocks/{stockName}")]
        public async Task<ActionResult<int>> GetOwnedStocks(string stockName)
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                var count = await _stockPageService.GetOwnedStocksAsync(stockName, userCnp);
                return Ok(count);
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

        [HttpGet("user-stock/{stockName}")]
        public async Task<ActionResult<UserStock>> GetUserStock(string stockName)
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                var userStock = await _stockPageService.GetUserStockAsync(stockName, userCnp);
                return Ok(userStock);
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

        [HttpPost("buy")]
        public async Task<ActionResult<bool>> BuyStock([FromBody] BuySellStockDto dto)
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                var result = await _stockPageService.BuyStockAsync(dto.StockName, dto.Quantity, userCnp);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentNullException ex) when (ex.ParamName == "stockName")
            {
                return BadRequest("StockName is required.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("sell")]
        public async Task<ActionResult<bool>> SellStock([FromBody] BuySellStockDto dto)
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                var result = await _stockPageService.SellStockAsync(dto.StockName, dto.Quantity, userCnp);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentNullException ex) when (ex.ParamName == "stockName")
            {
                return BadRequest("StockName is required.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("favorite/{stockName}")]
        public async Task<ActionResult<bool>> GetFavorite(string stockName)
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                var isFavorite = await _stockPageService.GetFavoriteAsync(stockName, userCnp);
                return Ok(isFavorite);
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

        [HttpPost("favorite/toggle")]
        public async Task<IActionResult> ToggleFavorite([FromBody] ToggleFavoriteDto dto)
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                await _stockPageService.ToggleFavoriteAsync(dto.StockName, dto.State, userCnp);
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentNullException ex) when (ex.ParamName == "stockName")
            {
                return BadRequest("StockName is required.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("author/{stockName}")]
        public async Task<ActionResult<User>> GetStockAuthor(string stockName)
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                var userStock = await _stockPageService.GetUserStockAsync(stockName, userCnp);
                return userStock == null ? (ActionResult<User>)NotFound("User stock not found.") : (ActionResult<User>)Ok(userStock.User);
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
    }

    public class BuySellStockDto
    {
        public required string StockName { get; set; }
        public int Quantity { get; set; }
    }

    public class ToggleFavoriteDto
    {
        public required string StockName { get; set; }
        public required bool State { get; set; }
    }
}

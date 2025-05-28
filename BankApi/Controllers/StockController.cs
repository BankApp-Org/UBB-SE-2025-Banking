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
    public class StockController(IStockService stockService, IUserRepository userRepository) : ControllerBase
    {
        private readonly IStockService _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Stock>>> GetAllStocks()
        {
            try
            {
                var stocks = await _stockService.GetAllStocksAsync();
                return Ok(stocks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Stock>> GetStockById(int id)
        {
            try
            {
                var stock = await _stockService.GetStockByIdAsync(id);
                return stock == null ? (ActionResult<Stock>)NotFound($"Stock with ID {id} not found.") : (ActionResult<Stock>)Ok(stock);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Stock>> CreateStock([FromBody] PartialStock partialStock)
        {
            try
            {
                // Get the current user's CNP from the principal
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Forbid();
                }
                var user = await _userRepository.GetByIdAsync(int.Parse(userId));
                if (user == null)
                {
                    return Forbid();
                }

                Stock stock = new()
                {
                    Price = partialStock.Price,
                    Quantity = partialStock.Quantity,
                    AuthorCNP = userId,
                    Name = partialStock.Name,
                    Symbol = partialStock.Symbol,
                };

                var createdStock = await _stockService.CreateStockAsync(stock);
                return CreatedAtAction(nameof(GetStockById), new { id = createdStock.Id }, createdStock);
            }
            catch (Exception ex)
            {
                // Log the exception details if necessary
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Assuming only admins can update stocks
        public async Task<ActionResult<Stock>> UpdateStock(int id, [FromBody] Stock stock)
        {
            try
            {
                if (id != stock.Id)
                {
                    return BadRequest("Stock ID mismatch.");
                }
                var updatedStock = await _stockService.UpdateStockAsync(id, stock);
                return updatedStock == null ? (ActionResult<Stock>)NotFound($"Stock with ID {id} not found.") : (ActionResult<Stock>)Ok(updatedStock);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Assuming only admins can delete stocks
        public async Task<IActionResult> DeleteStock(int id)
        {
            try
            {
                var result = await _stockService.DeleteStockAsync(id);
                return !result ? NotFound($"Stock with ID {id} not found.") : NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user")]
        public async Task<ActionResult<List<Stock>>> GetUserStocks()
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                var stocks = await _stockService.UserStocksAsync(userCnp);
                return Ok(stocks);
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

        [HttpGet("user/{cnp}")]
        [Authorize(Roles = "Admin")] // Only admins can view other users' stocks by CNP
        public async Task<ActionResult<List<Stock>>> GetUserStocksByCnp(string cnp)
        {
            try
            {
                var stocks = await _stockService.UserStocksAsync(cnp);
                return Ok(stocks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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

        [HttpGet("stocks")]
        public async Task<ActionResult<List<HomepageStock>>> GetFilteredAndSortedStocks(
            [FromQuery] string query,
            [FromQuery] string sortOption,
            [FromQuery] bool favoritesOnly = false)
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                var stocks = await _stockService.GetFilteredAndSortedStocksAsync(query ?? string.Empty, sortOption ?? string.Empty, favoritesOnly, userCnp);
                return Ok(stocks);
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

        [HttpPost("favorites/add")]
        public async Task<IActionResult> AddToFavorites([FromBody] HomepageStock stock)
        {
            try
            {
                // Ensure the stock has an ID, or handle cases where it might not.
                if (stock == null || stock.Id == 0)
                {
                    return BadRequest("Invalid stock data.");
                }
                await _stockService.AddToFavoritesAsync(stock);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("favorites/remove")]
        public async Task<IActionResult> RemoveFromFavorites([FromBody] HomepageStock stock)
        {
            try
            {
                if (stock == null || stock.Id == 0)
                {
                    return BadRequest("Invalid stock data.");
                }
                await _stockService.RemoveFromFavoritesAsync(stock);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class PartialStock
    {
        required public string Name { get; set; }
        required public string Symbol { get; set; }
        required public int Price { get; set; }
        required public int Quantity { get; set; }
    }
}

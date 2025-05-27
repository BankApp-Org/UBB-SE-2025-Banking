using BankApi.Repositories;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BaseStocksController(IBaseStocksRepository baseStocksRepository, IUserRepository userRepository) : ControllerBase
    {
        private readonly IBaseStocksRepository _baseStocksRepository = baseStocksRepository ?? throw new ArgumentNullException(nameof(baseStocksRepository));
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

        [HttpGet]
        public async Task<ActionResult<List<BaseStock>>> GetAllStocks()
        {
            try
            {
                var stocks = await _baseStocksRepository.GetAllStocksAsync();
                return Ok(stocks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{name}")]
        public async Task<ActionResult<BaseStock>> GetStockByName(string name)
        {
            try
            {
                var stock = await _baseStocksRepository.GetStockByNameAsync(name);
                return stock == null ? (ActionResult<BaseStock>)NotFound($"Stock with name '{name}' not found") : (ActionResult<BaseStock>)Ok(stock);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] // Only admins can create stocks
        public async Task<ActionResult<BaseStock>> AddStock([FromBody] BaseStockDto stockDto)
        {
            try
            {
                // Create a new BaseStock from the DTO
                var stock = new BaseStock
                {
                    Name = stockDto.Name,
                    Symbol = stockDto.Symbol,
                    AuthorCNP = await GetCurrentUserCnp()
                };

                var createdStock = await _baseStocksRepository.AddStockAsync(stock, stockDto.InitialPrice);
                return CreatedAtAction(nameof(GetStockByName), new { name = createdStock.Name }, createdStock);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{name}")]
        [Authorize(Roles = "Admin")] // Only admins can update stocks
        public async Task<ActionResult<BaseStock>> UpdateStock(string name, [FromBody] BaseStockUpdateDto stockDto)
        {
            try
            {
                // Get the existing stock
                var existingStock = await _baseStocksRepository.GetStockByNameAsync(name);
                if (existingStock == null)
                {
                    return NotFound($"Stock with name '{name}' not found");
                }

                // Update the stock properties
                existingStock.Name = stockDto.Name ?? existingStock.Name;
                existingStock.Symbol = stockDto.Symbol ?? existingStock.Symbol;

                var updatedStock = await _baseStocksRepository.UpdateStockAsync(existingStock);
                return Ok(updatedStock);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{name}")]
        [Authorize(Roles = "Admin")] // Only admins can delete stocks
        public async Task<IActionResult> DeleteStock(string name)
        {
            try
            {
                var success = await _baseStocksRepository.DeleteStockAsync(name);
                return !success ? NotFound($"Stock with name '{name}' not found") : NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class BaseStockDto
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        public int InitialPrice { get; set; } = 100;
    }

    public class BaseStockUpdateDto
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
    }
}
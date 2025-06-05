using BankApi.Repositories; // Required for IUserRepository
using Common.Models.Trading; // Required for List
using Common.Services.Trading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StockTransactionController(ITransactionService transactionService, IUserRepository userRepository) : ControllerBase
    {
        private readonly ITransactionService _transactionService = transactionService;
        private readonly IUserRepository _userRepository = userRepository;

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

        [HttpPost]
        public async Task<IActionResult> AddTransaction([FromBody] StockTransaction transaction)
        {
            await _transactionService.AddTransactionAsync(transaction);
            return Ok();
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<StockTransaction>>> GetAllTransactions()
        {
            return await _transactionService.GetAllTransactionsAsync();
        }

        [HttpPost("filter")]
        public async Task<ActionResult<List<StockTransaction>>> GetTransactionsByFilter([FromBody] StockTransactionFilterCriteria criteria)
        {
            return await _transactionService.GetByFilterCriteriaAsync(criteria);
        }
    }
}

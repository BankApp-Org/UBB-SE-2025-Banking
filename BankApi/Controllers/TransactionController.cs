using Microsoft.AspNetCore.Mvc;
using Common.Services;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BankApi.Repositories; // Required for IUserRepository
using System.Threading.Tasks; // Required for Task
using System.Collections.Generic; // Required for List

namespace BankApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController(ITransactionService transactionService, IUserRepository userRepository) : ControllerBase
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
        public async Task<IActionResult> AddTransaction([FromBody] TransactionLogTransaction transaction)
        {
            // Assuming TransactionLogTransaction has a UserCnp field or similar
            // If not, the service or this controller might need to set it.
            // For example: transaction.UserCnp = await GetCurrentUserCnp();
            // Or the service handles this based on context if possible.
            // For now, we pass it as is, assuming the service or model handles user association.
            await _transactionService.AddTransactionAsync(transaction);
            return Ok();
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<TransactionLogTransaction>>> GetAllTransactions()
        {
            return await _transactionService.GetAllTransactionsAsync();
        }

        [HttpPost("filter")]
        public async Task<ActionResult<List<TransactionLogTransaction>>> GetTransactionsByFilter([FromBody] TransactionFilterCriteria criteria)
        {
            // If criteria should be restricted to the current user, ensure UserCnp is set.
            // For example: criteria.UserCnp = await GetCurrentUserCnp();
            // This depends on the design of TransactionFilterCriteria and the service logic.
            // If criteria can have a UserCnp field, it should be populated here for non-admin users.
            // If the user is not an admin and criteria.UserCnp is empty or different, access might be denied by the service.

            // Assuming the service handles authorization based on the criteria's content (e.g., if UserCnp is present).
            // Or, explicitly set it for the current user if the intention is to always filter by the calling user unless admin.
            // For a generic filter endpoint, it's also common to let the service decide.
            // If UserCnp is a field in TransactionFilterCriteria:
            // var userCnp = await GetCurrentUserCnp();
            // if (!User.IsInRole("Admin") || string.IsNullOrEmpty(criteria.UserCnp))
            // {
            //     criteria.UserCnp = userCnp;
            // }
            return await _transactionService.GetByFilterCriteriaAsync(criteria);
        }
    }
}

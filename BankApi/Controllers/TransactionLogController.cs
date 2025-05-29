using BankApi.Repositories;
using Common.Exceptions;
using Common.Models.Trading;
using Common.Services.Stock;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionLogController(ITransactionLogService transactionLogService, IUserRepository userRepository) : ControllerBase
    {
        private readonly ITransactionLogService _transactionLogService = transactionLogService ?? throw new ArgumentNullException(nameof(transactionLogService));
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

        [HttpPost("filter")]
        public async Task<ActionResult<List<StockTransaction>>> GetFilteredTransactions([FromBody] StockTransactionFilterCriteria criteria)
        {
            try
            {
                // If the user is not an admin, ensure they can only filter their own transactions.
                if (!User.IsInRole("Admin"))
                {
                    var currentUserCnp = await GetCurrentUserCnp();
                    if (criteria.UserCnp != currentUserCnp)
                    {
                        // If a non-admin tries to filter for another user, or doesn't specify a CNP,
                        // default to their own CNP or return a Forbid result.
                        // For this example, we'll default to their own CNP.
                        criteria.UserCnp = currentUserCnp;
                    }
                }
                var transactions = await _transactionLogService.GetFilteredTransactions(criteria);
                return Ok(transactions);
            }
            catch (InvalidTransactionFilterCriteriaException ex) // This custom exception might need to be created if it doesn't exist
            {
                return BadRequest(ex.Message);
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

        [HttpPost("sort")] // Assuming transactions are passed in the body for sorting
        public ActionResult<List<StockTransaction>> SortTransactions([FromBody] SortTransactionsRequestDto request)
        {
            try
            {
                var sortedTransactions = _transactionLogService.SortTransactions(request.Transactions, request.SortType, request.Ascending);
                return Ok(sortedTransactions);
            }
            catch (InvalidSortTypeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportTransactions([FromBody] ExportTransactionsRequestDto request)
        {
            try
            {
                // Fetch transactions based on criteria. 
                // Non-admins should only be able to export their own transactions.
                if (!User.IsInRole("Admin"))
                {
                    var currentUserCnp = await GetCurrentUserCnp();
                    if (request.Criteria.UserCnp != currentUserCnp)
                    {
                        request.Criteria.UserCnp = currentUserCnp;
                    }
                }
                var transactions = await _transactionLogService.GetFilteredTransactions(request.Criteria);

                if (transactions == null || transactions.Count == 0) // Added .Any() check
                {
                    return NotFound("No transactions found for the given criteria.");
                }

                // Define a temporary file path for the export
                var tempFilePath = Path.Combine(Path.GetTempPath(), $"transactions_{Guid.NewGuid()}.{request.Format.ToLower()}");

                _transactionLogService.ExportTransactions(transactions, tempFilePath, request.Format);

                var memory = new MemoryStream();
                using (var stream = new FileStream(tempFilePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                // Delete the temporary file
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }

                return File(memory, GetContentType(request.Format), Path.GetFileName(tempFilePath));
            }
            catch (InvalidTransactionFilterCriteriaException ex) // This custom exception might need to be created
            {
                return BadRequest(ex.Message);
            }
            catch (ExportFormatNotSupportedException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex) // Catch other argument exceptions from the service
            {
                return BadRequest(ex.Message);
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

        private static string GetContentType(string format)
        {
            return format.ToLower() switch
            {
                "csv" => "text/csv",
                "json" => "application/json",
                "html" => "text/html",
                _ => "application/octet-stream",
            };
        }
    }

    public class SortTransactionsRequestDto
    {
        public List<StockTransaction> Transactions { get; set; }
        public string SortType { get; set; } = "Date";
        public bool Ascending { get; set; } = true;
    }

    public class ExportTransactionsRequestDto
    {
        public StockTransactionFilterCriteria Criteria { get; set; }
        public string Format { get; set; }
    }
}

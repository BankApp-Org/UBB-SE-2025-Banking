using BankApi.Repositories;
using Common.Models.Bank;
using Common.Services.Bank;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BankApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LoanController(ILoanService loanService, IUserRepository userRepository, ILoanRequestService loanRequestService) : ControllerBase
    {
        private readonly ILoanService _loanService = loanService ?? throw new ArgumentNullException(nameof(loanService));
        private readonly ILoanRequestService _loanRequestService = loanRequestService ?? throw new ArgumentNullException(nameof(loanRequestService));
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
        public async Task<ActionResult<List<Loan>>> GetAllLoans()
        {
            try
            {
                if (User.IsInRole("Admin"))
                {
                    return Ok(await _loanService.GetLoansAsync());
                }

                // Non-admin users can only see their own loans
                var userCnp = await GetCurrentUserCnp();
                return Ok(await _loanService.GetUserLoansAsync(userCnp));
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

        [HttpGet("user")]
        public async Task<ActionResult<List<Loan>>> GetUserLoans()
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                return Ok(await _loanService.GetUserLoansAsync(userCnp));
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
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<Loan>>> GetLoansByUser(string userCnp)
        {
            try
            {
                return Ok(await _loanService.GetUserLoansAsync(userCnp));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateLoan([FromBody] LoanRequest newLoan)
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();

                newLoan.Loan.UserCnp = userCnp; // Ensure the loan request is associated with the current user

                await _loanService.AddLoanAsync(newLoan);

                // Assuming AddLoanAsync populates the Id of the loanRequest.
                return CreatedAtAction(nameof(GetAllLoans), new { id = newLoan.Id }, newLoan);
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

        [HttpPost("{loanId}/increment-payment")]
        public async Task<IActionResult> IncrementMonthlyPaymentsCompleted(int loanId, [FromBody] PaymentDto payment)
        {
            try
            {
                // Get loans for the current user
                var userCnp = await GetCurrentUserCnp();
                var userLoans = await _loanService.GetUserLoansAsync(userCnp);

                // Check if the loan belongs to the current user or if the user is an admin
                var loan = userLoans.Find(l => l.Id == loanId);
                if (loan == null && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                throw new NotImplementedException("This method should be removed.");

                // await _loanService.IncrementMonthlyPaymentsCompletedAsync(loanId, payment.Penalty);
                return Ok();
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

        [HttpPost("check")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CheckLoans()
        {
            try
            {
                await _loanService.CheckLoansAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("update-history")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateHistoryForUser(string userCnp, int newScore)
        {
            try
            {
                await _loanService.UpdateHistoryForUserAsync(userCnp, newScore);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("PayLoan")]
        public async Task<IActionResult> PayLoan([FromBody] PaymentDto dto)
        {
           try
            {
                  await _loanService.PayLoanAsync(dto.LoanId, dto.Amount, dto.UserCNP, dto.Iban);
                  return Ok();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return StatusCode(500, $"Failed to pay loan: {ex.Message}");
            }
        }
    }

    public class NewLoanDTO
    {
        public int Id { get; set; }
        public string UserCnp { get; set; }
        public decimal Amount { get; set; }
        public DateTime ApplicationDate { get; set; }
        public DateTime RepaymentDate { get; set; }
        public string Status { get; set; }
    }
    public class PaymentDto
    {
        public int LoanId { get; set; }
        public decimal Amount { get; set; }
        public string UserCNP { get; set; }
        public string Iban { get; set; }
    }
}
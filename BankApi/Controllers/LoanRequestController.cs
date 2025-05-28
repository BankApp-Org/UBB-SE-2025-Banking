using Common.Models.Bank;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LoanRequestController(ILoanRequestService loanRequestService) : ControllerBase
    {
        private readonly ILoanRequestService _loanRequestService = loanRequestService ?? throw new ArgumentNullException(nameof(loanRequestService));

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<LoanRequest>>> GetLoanRequests()
        {
            try
            {
                return Ok(await _loanRequestService.GetLoanRequests());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("unsolved")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<LoanRequest>>> GetUnsolvedLoanRequests()
        {
            try
            {
                return Ok(await _loanRequestService.GetUnsolvedLoanRequests());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("suggestion")]
        public async Task<ActionResult<string>> GiveSuggestion([FromBody] LoanRequest loanRequest)
        {
            try
            {
                var suggestion = await _loanRequestService.GiveSuggestion(loanRequest);
                return Ok(suggestion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{loanRequestId}/solve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SolveLoanRequest(int loanRequestId)
        {
            try
            {
                await _loanRequestService.SolveLoanRequest(loanRequestId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{loanRequestId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteLoanRequest(int loanRequestId)
        {
            try
            {
                await _loanRequestService.DeleteLoanRequest(loanRequestId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
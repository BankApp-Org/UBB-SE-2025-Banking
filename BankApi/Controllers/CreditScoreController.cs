using Common.Models;
using Common.Models.Bank;
using Common.Models.Trading;
using Common.Services.Bank;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CreditScoreController : ControllerBase
    {
        private readonly ICreditScoringService _creditScoringService;

        public CreditScoreController(ICreditScoringService creditScoringService)
        {
            _creditScoringService = creditScoringService ?? throw new ArgumentNullException(nameof(creditScoringService));
        }

        [HttpPost("bank-transaction-impact/{userCnp}")]
        public async Task<ActionResult<int>> CalculateBankTransactionImpact(string userCnp, [FromBody] BankTransaction transaction)
        {
            try
            {
                var impact = await _creditScoringService.CalculateBankTransactionImpactAsync(userCnp, transaction);
                return Ok(impact);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error calculating bank transaction impact: {ex.Message}");
            }
        }

        [HttpPost("loan-payment-impact/{userCnp}")]
        public async Task<ActionResult<int>> CalculateLoanPaymentImpact(string userCnp, [FromBody] LoanPaymentRequest request)
        {
            try
            {
                var impact = await _creditScoringService.CalculateLoanPaymentImpactAsync(userCnp, request.Loan, request.PaymentAmount, request.IsOnTime);
                return Ok(impact);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error calculating loan payment impact: {ex.Message}");
            }
        }

        [HttpPost("gem-transaction-impact/{userCnp}")]
        public async Task<ActionResult<int>> CalculateGemTransactionImpact(string userCnp, [FromBody] GemTransactionRequest request)
        {
            try
            {
                var impact = await _creditScoringService.CalculateGemTransactionImpactAsync(userCnp, request.IsBuying, request.GemAmount, request.TransactionValue);
                return Ok(impact);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error calculating gem transaction impact: {ex.Message}");
            }
        }

        [HttpPost("stock-transaction-impact/{userCnp}")]
        public async Task<ActionResult<int>> CalculateStockTransactionImpact(string userCnp, [FromBody] StockTransaction stockTransaction)
        {
            try
            {
                var impact = await _creditScoringService.CalculateStockTransactionImpactAsync(userCnp, stockTransaction);
                return Ok(impact);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error calculating stock transaction impact: {ex.Message}");
            }
        }

        [HttpPost("update/{userCnp}")]
        public async Task<ActionResult> UpdateCreditScore(string userCnp, [FromBody] CreditScoreUpdateRequest request)
        {
            try
            {
                await _creditScoringService.UpdateCreditScoreAsync(userCnp, request.NewScore, request.Reason);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating credit score: {ex.Message}");
            }
        }

        [HttpGet("current/{userCnp}")]
        public async Task<ActionResult<int>> GetCurrentCreditScore(string userCnp)
        {
            try
            {
                var score = await _creditScoringService.GetCurrentCreditScoreAsync(userCnp);
                return Ok(score);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error getting current credit score: {ex.Message}");
            }
        }

        [HttpGet("comprehensive/{userCnp}")]
        public async Task<ActionResult<int>> CalculateComprehensiveCreditScore(string userCnp)
        {
            try
            {
                var score = await _creditScoringService.CalculateComprehensiveCreditScoreAsync(userCnp);
                return Ok(score);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error calculating comprehensive credit score: {ex.Message}");
            }
        }
    }

    // Request DTOs
    public class LoanPaymentRequest
    {
        public Loan Loan { get; set; } = null!;
        public decimal PaymentAmount { get; set; }
        public bool IsOnTime { get; set; }
    }

    public class GemTransactionRequest
    {
        public bool IsBuying { get; set; }
        public int GemAmount { get; set; }
        public double TransactionValue { get; set; }
    }

    public class CreditScoreUpdateRequest
    {
        public int NewScore { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
} 
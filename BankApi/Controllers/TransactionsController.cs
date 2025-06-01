using LoanShark.API.Models;
using LoanShark.API.Proxies;
using LoanShark.Domain;
using LoanShark.Service.BankService;
using Microsoft.AspNetCore.Mvc;

namespace LoanShark.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionsService transactionsService;

        public TransactionsController(ITransactionsService transactionsService)
        {
            this.transactionsService = transactionsService;
        }

        [HttpPost("AddTransaction")]
        public async Task<IActionResult> AddTransaction([FromBody] AddTransactionDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await transactionsService.AddTransaction(dto.SenderIban, dto.ReceiverIban, dto.Amount, dto.TransactionDescription);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to add transaction: {ex.Message}");
            }
        }

        [HttpPost("TakeLoanTransaction")]
        public async Task<IActionResult> TakeLoanTransaction([FromBody] TakeLoanTransactionDTO dto)
        {
            try
            {
                var result = await transactionsService.TakeLoanTransaction(dto.Iban, dto.LoanAmount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to take loan: {ex.Message}");
            }
        }

        [HttpPost("PayLoanTransaction")]
        public async Task<IActionResult> PayLoanTransaction([FromBody] PayLoanTransactionDTO dto)
        {
            try
            {
                var result = await transactionsService.PayLoanTransaction(dto.Iban, dto.PaymentAmount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to pay loan: {ex.Message}");
            }
        }

        [HttpGet("GetAllCurrencyExchangeRates")]
        public async Task<ActionResult<List<CurrencyExchange>>> GetAllCurrencyExchangeRates()
        {
            try
            {
                var result = await transactionsService.GetAllCurrencyExchangeRates();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving exchange rates: {ex.Message}");
            }
        }
    }

}

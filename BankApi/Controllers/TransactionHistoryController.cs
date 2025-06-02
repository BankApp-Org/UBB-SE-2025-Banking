using System.Collections.ObjectModel;
using Common.Models.Bank;
using Common.Services.Bank;
using Microsoft.AspNetCore.Mvc;

namespace LoanShark.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionHistoryController : ControllerBase
    {
        private readonly IBankTransactionService _transactionHistoryService;
        private readonly IBankAccountService _bankAccountService;

        public TransactionHistoryController(IBankTransactionService transactionHistoryService, IBankAccountService bankAccountService
        {
            this._bankAccountService = bankAccountService;
            this._transactionHistoryService = transactionHistoryService;
        }

        [HttpGet]
        public async Task<ActionResult<Collection<BankTransaction>>> GetAllTransactions([FromBody] TransactionFilters filters)
        {
            try
            {
                var transactions = await _transactionHistoryService.GetTransactions(filters);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving transactions: {ex.Message}");
            }
        }

        [HttpPost("CreateCSV/{iban}")]
        public async Task<IActionResult> CreateCSV(string iban)
        {
            try
            {
                // see Repositories.Exporters

                return Ok("CSV file created successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating CSV file: {ex.Message}");
            }
        }

        [HttpGet("GetTransactionTypeCounts/{iban}")]
        public async Task<ActionResult<Dictionary<TransactionType, int>>> GetTransactionTypeCounts(string iban)
        {
            try
            {
                Dictionary<TransactionType, int> typeCounts = [];
                var result = await _bankAccountService.FindBankAccount(iban) ?? throw new Exception("Bank account not found");
                foreach (BankTransaction transaction in result.Transactions)
                {
                    typeCounts.Add(transaction.TransactionType, typeCounts.GetValueOrDefault(transaction.TransactionType, 0) + 1);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting transaction type counts: {ex.Message}");
            }
        }
    }

    public class UpdateTransactionDescriptionDTO
    {
        public int TransactionId { get; set; }
        public string NewDescription { get; set; }
    }
}
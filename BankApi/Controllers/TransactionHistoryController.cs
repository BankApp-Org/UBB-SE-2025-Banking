using BankApi.Repositories.Exporters;
using Common.Models.Bank;
using Common.Services.Bank;
using Microsoft.AspNetCore.Mvc;
using System.Collections.ObjectModel;

namespace BankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionHistoryController : ControllerBase
    {
        private readonly IBankTransactionService _transactionHistoryService;
        private readonly IBankAccountService _bankAccountService;

        public TransactionHistoryController(IBankTransactionService transactionHistoryService, IBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
            _transactionHistoryService = transactionHistoryService;
        }

        [HttpPost]
        public async Task<ActionResult<List<BankTransaction>>> GetAllTransactions([FromBody] TransactionFilters filters)
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
        [HttpGet]
        public async Task<IActionResult> CreateCSV(string iban)
        {
            if (string.IsNullOrEmpty(iban))
                return BadRequest("IBAN is required.");

            var matchingTypes = Enum.GetValues(typeof(Common.Models.Bank.TransactionType))
                .Cast<Common.Models.Bank.TransactionType>()
                .ToList();

            List<BankTransaction> results = new List<BankTransaction>();

            foreach (var type in matchingTypes)
            {
                var filters = new TransactionFilters
                {
                    Type = type,
                    SenderIban = iban,
                    StartDate = new DateTime(2000, 1, 1),
                    EndDate = new DateTime(3000, 1, 1),
                };

                results.AddRange(await _transactionHistoryService.GetTransactions(filters));

                filters.SenderIban = string.Empty;
                filters.ReceiverIban = iban;

                results.AddRange(await _transactionHistoryService.GetTransactions(filters));
            }

            var exporter = new CSVBankTransactionExporter();
            string csvContent = exporter.ExportToString(results); 

            var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
            return File(bytes, "text/csv", "transactions.csv");
        }

        [HttpGet("ExportCsvString/{iban}")]
        public async Task<IActionResult> ExportCsvString(string iban)
        {
            var matchingTypes = Enum.GetValues(typeof(TransactionType))
                .Cast<TransactionType>()
                .ToList();

            List<BankTransaction> results = new();

            foreach (var type in matchingTypes)
            {
                var filters = new TransactionFilters
                {
                    Type = type,
                    SenderIban = iban,
                    StartDate = new DateTime(2000, 1, 1),
                    EndDate = new DateTime(3000, 1, 1),
                };

                results.AddRange(await _transactionHistoryService.GetTransactions(filters));

                filters.SenderIban = string.Empty;
                filters.ReceiverIban = iban;

                results.AddRange(await _transactionHistoryService.GetTransactions(filters));
            }

            var exporter = new CSVBankTransactionExporter();
            string csvContent = exporter.ExportToString(results); 

            return Ok(csvContent);
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
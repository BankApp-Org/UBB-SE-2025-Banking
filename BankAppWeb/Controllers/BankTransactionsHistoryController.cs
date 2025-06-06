using BankAppWeb.Models;
using Common.Models;
using Common.Models.Bank;
using Common.Services;
using Common.Services.Bank;
using Common.Services.Proxy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.ObjectModel;

namespace BankAppWeb.Controllers
{
    [Authorize]
    public class BankTransactionsHistoryController : Controller
    {
        private readonly IBankTransactionService _transactionsHistoryService;

        public BankTransactionsHistoryController(IBankTransactionService transactionsHistoryService)
        {
            _transactionsHistoryService = transactionsHistoryService;
        }

        public async Task<IActionResult> Index(BankTransactionsHistoryViewModel viewModel)
        {
            string IBAN = viewModel.IBAN;
            string Filter = viewModel.Filter;
            if(string.IsNullOrWhiteSpace(IBAN))
            {
                return NotFound();
            }

            // Normalize the filter
            string normalizedFilter = Filter?.ToLowerInvariant() ?? string.Empty;

            // Get all matching enum values based on string match
            var matchingTypes = Enum.GetValues(typeof(Common.Models.Bank.TransactionType))
                .Cast<Common.Models.Bank.TransactionType>()
                .Where(t => t.ToString().ToLowerInvariant().Contains(normalizedFilter) || string.IsNullOrEmpty(normalizedFilter))
                .ToList();

            List<BankTransaction> results = new List<BankTransaction>();

            foreach (var type in matchingTypes)
            {
                TransactionFilters filters = new TransactionFilters
                {
                    Type = type,
                    SenderIban = IBAN,
                    StartDate = new DateTime(2000, 1, 1),
                    EndDate = new DateTime(3000, 1, 1),
                };

                var transactions1 = await _transactionsHistoryService.GetTransactions(filters);

                filters.SenderIban = string.Empty;
                filters.ReceiverIban = IBAN;

                var transactions2 = await _transactionsHistoryService.GetTransactions(filters);

                results.AddRange(transactions1);
                results.AddRange(transactions2);
            }


            List<TransactionsHistoryDTO> trans = new List<TransactionsHistoryDTO>();
            foreach (var transaction in results)
            {
                trans.Add(new TransactionsHistoryDTO
                {
                    SenderIBAN = transaction.SenderIban,
                    ReceiverIBAN = transaction.ReceiverIban,
                    SentAmount = transaction.SenderAmount.ToString(),
                    ReceivedAmount = transaction.ReceiverAmount.ToString(),
                    Date = transaction.TransactionDatetime.ToString(),
                    Type = transaction.TransactionType.ToString(),
                });
            }

            return View(new BankTransactionsHistoryViewModel
            {
                Transactions = trans,
                IBAN = IBAN,
                Filter = Filter
            });
        }

        public async Task<IActionResult> ExportToCsv(string IBAN)
        {
            bool answer = await ((BankTransactionProxyService) _transactionsHistoryService).CreateCSV(IBAN);

            TempData["AlertMessage"] = "Exported to CSV on desktop!";

            return RedirectToAction("Index", "BankTransactionsHistory", new { IBAN = IBAN });
        }

        public IActionResult Chart(string IBAN)
        {
            return RedirectToAction("Index", "TransactionHistoryChart");
        }
    }
}

using BankAppWeb.ViewModels;
using Common.Services.Bank;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace BankAppWeb.Controllers
{
    [Authorize]
    public class TransactionHistoryChartController : Controller
    {
        private readonly IBankTransactionService _transactionHistoryService;

        public TransactionHistoryChartController(IBankTransactionService transactionHistoryService)
        {
            _transactionHistoryService = transactionHistoryService;
        }

        public async Task<IActionResult> Index()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("User not logged in"));
            List<TransactionTypeCountDTO> transactionTypesCount = await _transactionHistoryService.GetTransactionTypeCounts(userId);

            Dictionary<string, int> answers = new Dictionary<string, int>();

            foreach (var transactionType in transactionTypesCount)
            {
                if (answers.ContainsKey(transactionType.TransactionType.TransactionType.ToString()))
                    answers[transactionType.TransactionType.TransactionType.ToString()] += 1;
                else 
                    answers.Add(transactionType.TransactionType.TransactionType.ToString(), 1);
            }

            ViewBag.ChartData = answers;

            return View();
        }
    }
}
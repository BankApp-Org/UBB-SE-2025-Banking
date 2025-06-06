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

            TransactionHistoryChartViewModel viewModel = new()
            {
                TransactionTypeCounts = transactionTypesCount
            };
            return View(viewModel);
        }
    }
}
using BankAppWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BankApp.Service.Interfaces;

namespace BankAppWeb.Controllers
{
    [Authorize]
    public class TransactionHistoryChartController : Controller
    {
        private readonly ITransactionHistoryService _transactionHistoryService;

        public TransactionHistoryChartController(ITransactionHistoryService transactionHistoryService)
        {
            _transactionHistoryService = transactionHistoryService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var transactionTypesCount = await _transactionHistoryService.GetTransactionTypeCounts(userId);

            var viewModel = new TransactionHistoryChartViewModel
            {
                TransactionTypeCounts = transactionTypesCount
            };

            return View(viewModel);
        }
    }
} 
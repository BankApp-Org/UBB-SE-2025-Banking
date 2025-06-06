using BankAppWeb.ViewModels;
using Common.Models.Bank;
using Common.Services.Bank;
using Microsoft.AspNetCore.Mvc;

namespace LoanShark.MVC.Controllers
{
    public class SendMoneyController : Controller
    {
        private readonly IBankTransactionService transactionService;

        public SendMoneyController(IBankTransactionService transactionService)
        {
            this.transactionService = transactionService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new SendMoneyViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Send(SendMoneyViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ReceiverIban) || string.IsNullOrWhiteSpace(model.SumOfMoney))
            {
                model.ErrorMessage = "IBAN and amount are required.";
                return View("Index", model);
            }

            // Get sender IBAN from session (previously selected)
            if (string.IsNullOrEmpty(model.SenderIban))
            {
                model.ErrorMessage = "Sender account is not selected.";
                return View("Index", model);
            }

            // Convert amount safely
            if (!decimal.TryParse(model.SumOfMoney, out decimal amount))
            {
                model.ErrorMessage = "Invalid amount format.";
                return View("Index", model);
            }

            BankTransaction transaction = new()
            {
                SenderIban = model.SenderIban,
                ReceiverIban = model.ReceiverIban,
                SenderAmount = amount,
                ReceiverAmount = amount,
                TransactionDescription = model.Details ?? string.Empty,
                TransactionType = TransactionType.Transfer,
                TransactionDatetime = DateTime.UtcNow,
                ReceiverCurrency = Currency.RON,
                SenderCurrency = Currency.RON,
            };

            bool result = await transactionService.CreateTransaction(transaction);

            TempData["ResultMessage"] = result;
            return RedirectToAction("Result");
        }


        [HttpGet]
        public IActionResult Result()
        {
            ViewBag.Message = TempData["ResultMessage"];
            return View();
        }

        [HttpPost]
        public IActionResult Close()
        {
            return RedirectToAction("Index", "MainPage");
        }
    }
}
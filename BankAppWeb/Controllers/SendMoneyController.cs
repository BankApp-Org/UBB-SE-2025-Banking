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
        public IActionResult Index(string IBAN)
        {
            return View(new SendMoneyViewModel
            {
                SenderIban = IBAN,
            });
        }

        [HttpPost]
        public async Task<IActionResult> Send(SendMoneyViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ReceiverIban) || string.IsNullOrWhiteSpace(model.SumOfMoney))
            {
                model.ErrorMessage = "IBAN and amount are required.";
                return View("Index", model);
            }

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

            if (result)
            {
                TempData["SuccessMessage"] = "Transaction completed successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Transaction failed. Please try again.";
            }
            
            return RedirectToAction("Index", "MainPage", new { SelectedAccountIban = model.SenderIban });
        }

        [HttpPost]
        public IActionResult Close()
        {
            return RedirectToAction("Index", "MainPage");
        }
    }
}
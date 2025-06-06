using BankAppWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Common.Services.Bank;
using Common.Models.Bank;
using Common.Services;

namespace BankAppWeb.Controllers
{
    public class SendMoneyController : Controller
    {
        private readonly IBankTransactionService _transactionsService;
        private readonly IAuthenticationService _authenticationService;

        public SendMoneyController(
            IBankTransactionService transactionsService,
            IAuthenticationService authenticationService)
        {
            this._transactionsService = transactionsService ?? throw new ArgumentNullException(nameof(transactionsService));
            this._authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new SendMoneyViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Send(SendMoneyViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Iban) || string.IsNullOrWhiteSpace(model.SumOfMoney))
            {
                model.ErrorMessage = "IBAN and amount are required.";
                return View("Index", model);
            }

            // Get sender IBAN from authentication service (UserSession) instead of session
            var userSession = _authenticationService.GetCurrentUserSession();
            string? senderIban = userSession?.CurrentBankAccountIban;
            
            if (string.IsNullOrEmpty(senderIban))
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
            //TODO Somebody who is smarter and braver should look here
            //NOTE: BankTransaction is not equivalent with Transaction from prev project
            BankTransaction bankTransaction = new BankTransaction();
            // Call the transaction service
            string result = await this._transactionsService.CreateTransaction(bankTransaction);

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
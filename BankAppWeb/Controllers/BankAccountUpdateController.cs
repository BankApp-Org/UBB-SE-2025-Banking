using BankAppWeb.Models;
using Common.Models;
using Common.Services;
using Common.Services.Bank;
using Microsoft.AspNetCore.Mvc;

namespace BankAppWeb.Controllers
{
    public class BankAccountUpdateController : Controller
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly IAuthenticationService _authenticationService;
        private UserSession user;

        public BankAccountUpdateController(IBankAccountService bankAccountService, IAuthenticationService authenticationService)
        {
            _bankAccountService = bankAccountService;
            _authenticationService = authenticationService;
            user = _authenticationService.GetCurrentUserSession();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string iban)
        {
            if (string.IsNullOrEmpty(iban))
                return NotFound();

            var account = await _bankAccountService.FindBankAccount(iban);
            if (account == null)
                return NotFound();

            var model = new BankAccountEditModel
            {
                Iban = account.Iban,
                Name = account.Name,
                DailyLimit = account.DailyLimit,
                MaximumPerTransaction = account.MaximumPerTransaction,
                MaximumNrTransactions = account.MaximumNrTransactions,
                IsBlocked = account.Blocked
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(BankAccountEditModel model)
        {
            if (!ModelState.IsValid)
                return View(model);


            var bankAccount = new Common.Models.Bank.BankAccount
            {
                Iban = model.Iban,
                Name = model.Name,
                DailyLimit = (decimal)model.DailyLimit,
                MaximumPerTransaction = (decimal)model.MaximumPerTransaction,
                MaximumNrTransactions = model.MaximumNrTransactions,
                Blocked = model.IsBlocked,
                Currency = Common.Models.Bank.Currency.USD, // Assuming USD as default, adjust as needed
                Balance = 0.0m, // Assuming initial balance is 0, adjust as needed
                Transactions = new List<Common.Models.Bank.BankTransaction>(),
                UserId = int.Parse(user.UserId),
                User = new Common.Models.User { Id = int.Parse(user.UserId) }

            };
            var result = await _bankAccountService.UpdateBankAccount(bankAccount);

            if (result)
            {
                TempData["SuccessMessage"] = "Account updated successfully.";
                return RedirectToAction("Index", "BankAccountList");
            }

            ModelState.AddModelError("", "Failed to update the account.");
            return View(model);
        }
    }
}

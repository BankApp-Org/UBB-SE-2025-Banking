using BankAppWeb.Models;
using Common.Models.Bank;
using Common.Services.Bank;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankAppWeb.Controllers
{
    public class BankAccountListController : Controller
    {
        private readonly IBankAccountService _bankAccountService;

        public BankAccountListController(IBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
        }


        public async Task<IActionResult> Index()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("User not logged in"));
            var accounts = await _bankAccountService.GetUserBankAccounts(userId);
            return this.View(accounts);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
                return NotFound();

            var account = await _bankAccountService.FindBankAccount(iban);

            if (account == null)
                return NotFound();


            var model = new BankAccountListModel
            {
                BankAccount = account
            };

            return RedirectToAction("Edit", new { iban = model.BankAccount.Iban });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string iban)
        {
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
                Currency = account.Currency.ToString(),
                IsBlocked = account.Blocked
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(BankAccountEditModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var account = await _bankAccountService.FindBankAccount(model.Iban);
            if (account == null)
                return NotFound();

            // Update account with values from model
            account.Name = model.Name;
            account.DailyLimit = model.DailyLimit;
            account.MaximumPerTransaction = model.MaximumPerTransaction;
            account.MaximumNrTransactions = model.MaximumNrTransactions;

            // Parse currency if changed
            if (Enum.TryParse<Currency>(model.Currency, out var currency))
            {
                account.Currency = currency;
            }

            account.Blocked = model.IsBlocked;

            await _bankAccountService.UpdateBankAccount(account);
            TempData["SuccessMessage"] = "Account updated successfully.";
            return RedirectToAction("Details", new { iban = model.Iban });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string iban)
        {
            await _bankAccountService.RemoveBankAccount(iban);
            return RedirectToAction("Index");
            //return this.View(selectedAccount);

        }
    }
}
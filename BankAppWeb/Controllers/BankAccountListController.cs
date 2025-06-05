using Microsoft.AspNetCore.Mvc;
using Common.Models.Bank;
using Common.Services.Bank;
using Common.Services;
using BankAppWeb.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Common.Models;
using Common.Models;

namespace BankAppWeb.Controllers
{
    public class BankAccountListController : Controller
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly IAuthenticationService _authenticationService;

        public BankAccountListController(IBankAccountService bankAccountService, IAuthenticationService attservice)
        {
            _bankAccountService = bankAccountService;
            _authenticationService = attservice;
        }


        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();
            var accounts = await _bankAccountService.GetUserBankAccounts(userId);
            return this.View(accounts);
        }
        [HttpGet]
        public async Task<IActionResult> Details(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
                return NotFound();

            int userId = GetCurrentUserId();
            var account = await _bankAccountService.FindBankAccount(iban);

            if (account == null)
                return NotFound();

            ///modifica cand ii da accept lui ale !!!!!!!!!!!!!1
            var currentIban = HttpContext.Session.GetString("current_bank_account_iban");

            ///var currentUser = _authenticationService.GetCurrentUserSession();
            ///var currentIban = currentUseer.CurrentBankAccountIban;
            
            if (!string.IsNullOrEmpty(currentIban) && currentIban == iban)
            {
                var model = new BankAccountListModel
                {
                    BankAccount = account
                };

                return RedirectToAction("Edit", new { iban = model.BankAccount.Iban });

            }

            var readOnlyModel = new BankAccountListModel
            {
                BankAccount = account
            };

            return View("~/Views/BankAccountDetails/Index.cshtml", readOnlyModel);
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

        [HttpPost]
        public IActionResult SetCurrentBankAccount(string iban)
        {
            HttpContext.Session.SetString("current_bank_account_iban", iban);
            ///currentUseer.CurrentBankAccountIban =  iban;
            return RedirectToAction("Edit", "BankAccountUpdate");
        }


        private int GetCurrentUserId()
        {
            return int.TryParse(HttpContext.Session.GetString("userId"), out int id) ? id : 0;
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Common.Services.Bank;
using Common.Services;
using BankAppWeb.Models;
using Common.Models.Bank;

namespace BankAppWeb.Controllers
{
    public class MainPageController : Controller
    {
        private readonly IBankAccountService _mainPageService;
        private readonly IUserService _userService;

        public MainPageController(IBankAccountService mainPageService, IUserService userService)
        {
            _mainPageService = mainPageService;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult SelectBankAccount(string iban)
        {
            //if (!string.IsNullOrEmpty(iban))
            //{
            //    HttpContext.Session.SetString("current_bank_account_iban", iban);
            //}

            return RedirectToAction("Index", new { SelectedAccountIban = iban });
        }


        [HttpGet]
        public async Task<IActionResult> Index(string? SelectedAccountIban = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var accounts = await _mainPageService.GetUserBankAccounts(Int32.Parse(userId));

            //if (HttpContext.Session.GetString("current_bank_account_iban") == null && accounts.Count > 0)
            //    HttpContext.Session.SetString("current_bank_account_iban", accounts[0].ReceiverIban);

            if (string.IsNullOrEmpty(SelectedAccountIban) && accounts.Count > 0)
            {
                TempData["SelectedAccountIban"] = accounts[0].Iban;
            }
            else
            {
                TempData["SelectedAccountIban"] = SelectedAccountIban;
            }
            
            // Get user information including credit score
            var currentUser = await _userService.GetCurrentUserAsync();
            int creditScore = currentUser?.CreditScore ?? 300; // Default to minimum if not found
            string creditScoreDescription = creditScore switch
            {
                >= 750 => "Excellent Credit",
                >= 700 => "Very Good Credit",
                >= 650 => "Good Credit",
                >= 600 => "Fair Credit",
                _ => "Poor Credit"
            };

            var vm = new MainPageViewModel
                {
                    WelcomeText = $"Welcome, {currentUser.FirstName}!",
                    BankAccounts = new List<BankAccount>(accounts),
                    BalanceButtonContent = TempData["BalanceButtonContent"]?.ToString() ?? "Check Balance",
                    SelectedAccountIban = TempData["SelectedAccountIban"]?.ToString() ?? "INVALID IBAN",
                    CreditScore = creditScore,
                    CreditScoreDescription = creditScoreDescription
                };

            return View("Index", vm);
        }

        [HttpPost]
        public async Task<IActionResult> CheckBalance(string iban)
        {
            if (string.IsNullOrEmpty(iban))
            {
                TempData["BalanceMessage"] = "No account selected.";
                return RedirectToAction("Index");
            }

            // HttpContext.Session.SetString("current_bank_account_iban", iban);

            var result = await _mainPageService.FindBankAccount(iban);
            TempData["BalanceButtonContent"] = $"{result.Balance:N2} {result.Currency}";
            TempData["SelectedAccountIban"] = iban;


            return RedirectToAction("Index", new { SelectedAccountIban = iban });
        }

        [HttpPost]
        public IActionResult BankAccountDetails(string iban)
        {
            //var iban = HttpContext.Session.GetString("current_bank_account_iban");
            if (string.IsNullOrEmpty(iban))
            {
                TempData["BalanceMessage"] = "No bank account selected.";
                return RedirectToAction("Index");
            }

            // No route values passed
            return RedirectToAction("Index", "BankAccountDetails", new { iban });
        }


        [HttpPost]
        public IActionResult Transaction(string iban)
        {
            if (string.IsNullOrEmpty(iban))
                return RedirectToAction("Index");


            return RedirectToAction("Index", "BankTransactions", new { IBAN = iban}); 
        }

        [HttpPost]
        public IActionResult TransactionHistory(string iban)
        {
            //var iban = HttpContext.Session.GetString("current_bank_account_iban");
            if (string.IsNullOrEmpty(iban))
                return RedirectToAction("Index");

            return RedirectToAction("Index", "BankTransactionsHistory", new {Filter = "", IBAN = iban}); 
        }

        [HttpPost]
        public IActionResult BankAccountSettings(string iban)
        {
            //var iban = HttpContext.Session.GetString("current_bank_account_iban");

            if (string.IsNullOrEmpty(iban))
                return RedirectToAction("Index");

            return RedirectToAction("Details", "BankAccountList", new { iban });
        }

        [HttpPost]
        public IActionResult CreateBankAccount()
        {
            return RedirectToAction("Index", "BankAccountCreate");
        }

        [HttpPost]
        public IActionResult Loan()
        {
            //var iban = HttpContext.Session.GetString("current_bank_account_iban");
            //if (string.IsNullOrEmpty(iban))
            //    return RedirectToAction("Index");

            return RedirectToAction("Index", "Loans");
        }

        [HttpGet]
        public IActionResult GoToSocial()
        {
            return RedirectToAction("Index", "Social");
        }

        [HttpGet]
        public IActionResult AccountSettings()
        {
            return RedirectToAction("Index", "UserInformation");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Account");
        }
    }
}

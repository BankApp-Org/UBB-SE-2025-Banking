using Microsoft.AspNetCore.Mvc;
using Common.Models;
using BankAppWeb.Services;
using Common.Services.Bank;
using BankAppWeb.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System;
using Common.Models.Bank;
using System.Collections.Generic;

namespace BankAppWeb.Controllers
{
    [Authorize]
    public class BankAccountCreateController : Controller
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly WebAuthenticationService _authService;

        public BankAccountCreateController(IBankAccountService bankAccountService, WebAuthenticationService authService)
        {
            _bankAccountService = bankAccountService;
            _authService = authService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var model = new BankAccountCreateModel
            {
                AvailableCurrencies = Enum.GetNames(typeof(Currency))
                    .Select(currencyName => new CurrencyItemModel { Name = currencyName })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(BankAccountCreateModel model)
        {
            var userSession = _authService.GetCurrentUserSession();
            if (userSession == null || string.IsNullOrEmpty(userSession.UserId))
            {
                TempData["Error"] = "User session expired. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            if (Request.Form.TryGetValue("SelectedCurrencyIndex", out var selectedIndexStr)
                && int.TryParse(selectedIndexStr, out int selectedIndex)
                && selectedIndex >= 0 && selectedIndex < model.AvailableCurrencies.Count)
            {
                model.AvailableCurrencies[selectedIndex].IsChecked = true;
                model.SelectedCurrency = model.AvailableCurrencies[selectedIndex];
            }
            else
            {
                TempData["Error"] = "Please select a currency.";
                model.AvailableCurrencies = Enum.GetNames(typeof(Currency))
                    .Select(name => new CurrencyItemModel { Name = name })
                    .ToList();
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.CustomName))
            {
                ModelState.AddModelError(nameof(model.CustomName), "Please enter a custom name.");
                return View(model);
            }

            var iban = await _bankAccountService.GenerateIBAN();

            var bankAccount = new BankAccount
            {
                UserId = int.Parse(userSession.UserId),
                Name = model.CustomName,
                Currency = Enum.Parse<Currency>(model.SelectedCurrency.Name),
                Iban = iban,
                Blocked = false,
                Balance = 0,
                DailyLimit = 10000, // sensible default
                MaximumPerTransaction = 5000, // sensible default
                MaximumNrTransactions = 10, // sensible default
                Transactions = new List<BankTransaction>(),
                User = new User { Id = int.Parse(userSession.UserId) }
            };

            var success = await _bankAccountService.CreateBankAccount(bankAccount);

            if (!success)
            {
                TempData["Error"] = "Failed to create bank account.";
                return View(model);
            }

            TempData["Success"] = "Bank account created successfully!";
            return RedirectToAction("Index", "MainPage");
        }
    }
} 
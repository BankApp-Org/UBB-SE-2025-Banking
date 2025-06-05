using Microsoft.AspNetCore.Mvc;
using Common.Services.Bank;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using LoanShark.MVC.Models;

namespace BankAppWeb.Controllers
{
    public class CurrencyExchangeController : Controller
    {

        private readonly IBankAccountService bankaccountService;
        public async Task<IActionResult> Index()
        {
            var rawRates = await this.bankaccountService.GetExchangeRateAsync();

            var rates = rawRates.Select(rate =>
            {
                return new CurrencyExchangeRateDTO
                {
                    FromCurrency = rate.FromCurrency,
                    ToCurrency = rate.ToCurrency,
                    ExchangeRate = rate.ExchangeRate,
                };
                
            }).ToList();

            return View(new CurrencyExchangeViewModel
            {
                ExchangeRates = rates,
            });
        }

        public CurrencyExchangeController(IBankAccountService bankService)
        {
            this.bankaccountService = bankService;
        }
        public IActionResult Close()
        {
            return RedirectToAction("Index", "Transactions");
        }



    }
}

using BankAppWeb.Models;
using Common.Services.Bank;
using Microsoft.AspNetCore.Mvc;

namespace BankAppWeb.Controllers
{
    public class BankAccountDetailsController : Controller
    {
        private readonly IBankAccountService _bankAccountService;

        public BankAccountDetailsController(IBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string iban)
        {
            if (string.IsNullOrEmpty(iban))
                return RedirectToAction("Index", "HomePage");

            var bankAccount = await _bankAccountService.FindBankAccount(iban);

            if (bankAccount == null)
                return RedirectToAction("Index", "HomePage");

            var model = new BankAccountDetailsModel
            {
                BankAccount = bankAccount
            };

            return View(model);
        }
    }
}

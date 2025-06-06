using BankAppWeb.Models;
using Common.Services.Bank;
using Microsoft.AspNetCore.Mvc;

namespace BankAppWeb.Controllers
{
    public class BankAccountVerifyController : Controller
    {
        private readonly IBankAccountService _bankAccountService;

        public BankAccountVerifyController(IBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
        }

        [HttpGet]
        public IActionResult Verify(string iban)
        {
            return View(new BankAccountVerifyModel { Iban = iban });
        }

        [HttpPost]
        public async Task<IActionResult> Verify(BankAccountVerifyModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            TempData["VerifiedIBAN"] = model.Iban;
            return RedirectToAction("Confirm", "BankAccountDelete", new { iban = model.Iban });
        }
    }
}

using BankAppWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankAppWeb.Controllers
{
    [Authorize]
    public class BankTransactionsController : Controller
    {
        public IActionResult Index(string IBAN)
        {
            if(string.IsNullOrEmpty(IBAN))
            {
                return RedirectToAction("Index", "MainPage");
            }

            return View(new BankTransactionsModel
            {
                IBAN = IBAN
            });
        }
    }
}

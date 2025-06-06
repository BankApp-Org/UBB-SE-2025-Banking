using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankAppWeb.Controllers
{
    [Authorize]
    public class BankTransactionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

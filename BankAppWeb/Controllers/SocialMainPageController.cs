using Microsoft.AspNetCore.Mvc;

namespace BankAppWeb.Controllers
{
    public class SocialMainPageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

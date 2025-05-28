using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Common.Models;
using System.Diagnostics;
using BankAppWeb.Models;

namespace BankAppWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAuthenticationService _authService;

        public HomeController(ILogger<HomeController> logger, IAuthenticationService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        public IActionResult Index()
        {
            // Example of accessing authentication information
            ViewData["IsLoggedIn"] = _authService.IsUserLoggedIn();
            ViewData["IsAdmin"] = _authService.IsUserAdmin();

            if (_authService.IsUserLoggedIn())
            {
                ViewData["Username"] = _authService.GetCurrentUserSession()?.UserName;
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public IActionResult Protected()
        {
            // This action is only accessible to authenticated users
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnly()
        {
            // This action is only accessible to users with the Admin role
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

using Common.Services;
using Microsoft.AspNetCore.Mvc;
using StockAppWeb.Views.Users;

namespace StockAppWeb.Controllers
{
    public class UsersController(IUserService userService) : Controller
    {
        private readonly IUserService userService = userService;

        public async Task<IActionResult> Index()
        {
            IndexModel model = new()
            {
                UsersList = await userService.GetUsers()
            };
            return View(model);
        }
    }
}

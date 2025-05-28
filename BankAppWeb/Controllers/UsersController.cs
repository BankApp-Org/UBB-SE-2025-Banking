using BankAppWeb.Views.Users;
using Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankAppWeb.Controllers
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

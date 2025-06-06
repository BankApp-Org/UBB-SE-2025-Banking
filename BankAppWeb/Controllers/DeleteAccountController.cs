using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Common.Services;
using BankAppWeb.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BankAppWeb.Controllers
{
    [Authorize]
    public class DeleteAccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;

        public DeleteAccountController(
            IUserService userService,
            IAuthenticationService authenticationService)
        {
            _userService = userService;
            _authenticationService = authenticationService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new DeleteAccountModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(DeleteAccountModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                var result = await _userService.DeleteUser(model.Password);
                if (result == "User deleted")
                {
                    // Clear the JWT cookie
                    Response.Cookies.Delete("jwt_token", new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                    });
                    await _authenticationService.LogoutAsync();
                    TempData["SuccessMessage"] = "Your account has been deleted successfully.";
                    return RedirectToAction("Login", "Account");
                }

                model.ErrorMessage = result;
                return View("Index", model);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = $"An error occurred: {ex.Message}";
                return View("Index", model);
            }
        }
    }
}
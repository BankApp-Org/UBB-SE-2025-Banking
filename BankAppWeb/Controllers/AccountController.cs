using BankAppWeb.Models;
using Common.Models;
using Common.Services;
using Common.Services.Trading;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IAppAuthService = Common.Services.IAuthenticationService;

namespace BankAppWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAppAuthService _authenticationService;
        private readonly IConfiguration _configuration;
        private readonly IStockService _stockService; // Added IStockService

        public AccountController(
            IUserService userService,
            IAppAuthService authenticationService,
            IConfiguration configuration,
            IStockService stockService) // Added IStockService to constructor
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService)); // Initialize IStockService
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Homepage");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                try
                {
                    var session = await _authenticationService.LoginAsync(model.Username, model.Password);

                    // Store the JWT in an HttpOnly cookie
                    Response.Cookies.Append("jwt_token", session.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = session.ExpiryTimestamp
                    });

                    // Redirect to return URL or Homepage page
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction(nameof(HomeController.Index), "Homepage");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Invalid login attempt: {ex.Message}");
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Clear the JWT cookie
            Response.Cookies.Delete("jwt_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
            });
            await _authenticationService.LogoutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Homepage");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Homepage");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Username,
                    Email = model.Email,
                    CNP = model.CNP,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Birthday = model.Birthday,
                    PasswordHash = model.Password
                };

                try
                {
                    await _userService.CreateUser(user);

                    // After successful registration, redirect to login page.
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Registration failed: {ex.Message}");
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var user = await _userService.GetCurrentUserAsync();
            if (user == null)
            {
                return Challenge(JwtBearerDefaults.AuthenticationScheme);
            }

            var userStocks = await _stockService.UserStocksAsync();

            var model = new ManageAccountViewModel
            {
                UserId = user.Id.ToString(),
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Birthday = user.Birthday,
                ImageUrl = user.Image ?? string.Empty,
                ProfileInitial = string.IsNullOrEmpty(user.UserName) ? "U" : user.UserName[0].ToString().ToUpper(),
                Description = user.Description ?? string.Empty,
                IsAdmin = _authenticationService.IsUserAdmin(),
                IsHidden = user.IsHidden,
                UserStocks = userStocks.Select(s => new UserStockViewModel
                {
                    Name = s.Name,
                    Symbol = s.Symbol,
                    Quantity = s.Quantity,
                    Price = s.Price
                }).ToList(),
                IsAuthenticated = true,
                ErrorMessage = TempData["ErrorMessage"] as string,
                SuccessMessage = TempData["StatusMessage"] as string
            };

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userService.GetCurrentUserAsync();
            if (user == null)
            {
                return Challenge(JwtBearerDefaults.AuthenticationScheme);
            }

            var model = new EditProfileViewModel
            {
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Description = user.Description,
                Image = user.Image,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Birthday = user.Birthday
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                {
                    return Challenge(JwtBearerDefaults.AuthenticationScheme);
                }

                // Create a new User object with updated values
                var updatedUser = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Description = model.Description ?? string.Empty,
                    Image = model.Image ?? string.Empty,
                    IsHidden = user.IsHidden,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Birthday = model.Birthday
                };

                await _userService.UpdateUserAsync(updatedUser);

                TempData["StatusMessage"] = "Your profile has been updated successfully.";
                return RedirectToAction(nameof(Manage));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Profile update failed: {ex.Message}");
                return View(model);
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                TempData["StatusMessage"] = "Your password has been changed. Please log in with your new password.";

                Response.Cookies.Delete("jwt_token", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                });
                await _authenticationService.LogoutAsync();

                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Password change failed: {ex.Message}");
                return View(model);
            }
        }
    }
}
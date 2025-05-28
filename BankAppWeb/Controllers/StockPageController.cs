using BankAppWeb.Views.StockPage;
using Common.Models;
using Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankAppWeb.Controllers
{
    public class StockPageController : Controller
    {
        private readonly IStockPageService _stockPageService;
        private readonly IStockService _stockService;
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;

        public StockPageController(IStockPageService stockPageService, IStockService stockService,
                                  IUserService userService, IAuthenticationService authenticationService)
        {
            _stockPageService = stockPageService;
            _stockService = stockService;
            _userService = userService;
            _authenticationService = authenticationService;
        }

        public async Task<IActionResult> Index(string stockName)
        {
            if (string.IsNullOrEmpty(stockName))
            {
                TempData["ErrorMessage"] = "Stock name is required.";
                return RedirectToAction("Index", "Homepage");
            }

            var model = new IndexModel(_stockPageService, _stockService, _userService, _authenticationService);
            await model.OnGetAsync(stockName);
        
            if (model.SelectedStock == null)
            {
                TempData["ErrorMessage"] = "Stock not found.";
                return RedirectToAction("Index", "Homepage");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyStock(string stockName, IndexModel.StockTransactionDto transaction)
        {
            if (!_authenticationService.IsUserLoggedIn())
            {
                TempData["ErrorMessage"] = "You must be logged in to buy stocks.";
                return RedirectToAction("Index", new { stockName });
            }

            if (ModelState.IsValid)
            {
                var model = new IndexModel(_stockPageService, _stockService, _userService, _authenticationService);
                await model.OnGetAsync(stockName);

                try
                {
                    bool success = await model.BuyStockAsync(transaction.Quantity);
                    if (success)
                    {
                        TempData["SuccessMessage"] = $"Successfully bought {transaction.Quantity} shares of {stockName}!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to buy stock. You may not have enough gems.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error buying stock: {ex.Message}";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid transaction data.";
            }

            return RedirectToAction("Index", new { stockName });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SellStock(string stockName, IndexModel.StockTransactionDto transaction)
        {
            if (!_authenticationService.IsUserLoggedIn())
            {
                TempData["ErrorMessage"] = "You must be logged in to sell stocks.";
                return RedirectToAction("Index", new { stockName });
            }

            if (ModelState.IsValid)
            {
                var model = new IndexModel(_stockPageService, _stockService, _userService, _authenticationService);
                await model.OnGetAsync(stockName);

                try
                {
                    bool success = await model.SellStockAsync(transaction.Quantity);
                    if (success)
                    {
                        TempData["SuccessMessage"] = $"Successfully sold {transaction.Quantity} shares of {stockName}!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to sell stock. You may not have enough shares.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error selling stock: {ex.Message}";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid transaction data.";
            }

            return RedirectToAction("Index", new { stockName });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(string stockName)
        {
            if (!_authenticationService.IsUserLoggedIn())
            {
                TempData["ErrorMessage"] = "You must be logged in to manage favorites.";
                return RedirectToAction("Index", new { stockName });
            }

            try
            {
                var model = new IndexModel(_stockPageService, _stockService, _userService, _authenticationService);
                await model.OnGetAsync(stockName);
                await model.ToggleFavoriteAsync();
            
                TempData["SuccessMessage"] = model.IsFavorite 
                    ? "Added to favorites!" 
                    : "Removed from favorites!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating favorite: {ex.Message}";
            }

            return RedirectToAction("Index", new { stockName });
        }

        public IActionResult GoToAlerts(string stockName)
        {
            if (string.IsNullOrEmpty(stockName))
            {
                TempData["ErrorMessage"] = "Stock name is required.";
                return RedirectToAction("Index", "Homepage");
            }

            return RedirectToAction("Index", "Alerts", new { stockName });
        }
    }
}

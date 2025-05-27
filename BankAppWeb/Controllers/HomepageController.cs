using Common.Models;using Common.Services;using Microsoft.AspNetCore.Mvc;using StockAppWeb.Models;namespace StockAppWeb.Controllers
{
    public class HomepageController : Controller    {        private readonly IStockService _stockService;        private readonly IStockPageService _stockPageService;        private readonly IAuthenticationService _authenticationService;        public HomepageController(IStockService stockService, IStockPageService stockPageService, IAuthenticationService authenticationService)        {            _stockService = stockService;            _stockPageService = stockPageService;            _authenticationService = authenticationService;        }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchQuery = "", string? selectedSortOption = "")
    {
        var viewModel = new HomepageViewModel(_stockService, _authenticationService)
        {
            SearchQuery = searchQuery ?? string.Empty,
            SelectedSortOption = selectedSortOption ?? string.Empty
        };

        await viewModel.LoadStocksAsync();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(string symbol)
    {
        if (!_authenticationService.IsUserLoggedIn())
        {
            TempData["ErrorMessage"] = "You must be logged in to manage favorites.";
            return RedirectToAction("Index");
        }

        if (string.IsNullOrWhiteSpace(symbol))
            return RedirectToAction("Index");

        try
        {
            // Find the stock by symbol to get its name
            var allStocks = await _stockService.GetFilteredAndSortedStocksAsync("", "", false);
            var stock = allStocks.FirstOrDefault(s => s.StockDetails.Symbol == symbol);

            if (stock == null)
            {
                TempData["ErrorMessage"] = "Stock not found.";
                return RedirectToAction("Index");
            }

            // Get current favorite status and toggle it using StockPageService
            bool currentStatus = await _stockPageService.GetFavoriteAsync(stock.StockDetails.Name);
            await _stockPageService.ToggleFavoriteAsync(stock.StockDetails.Name, !currentStatus);

            string message = !currentStatus ? "Added to favorites!" : "Removed from favorites!";
            TempData["SuccessMessage"] = message;
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating favorite: {ex.Message}";
        }

        return RedirectToAction("Index");
    }
    }
}

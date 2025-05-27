using Common.Models;
using Common.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace StockAppWeb.Views.StockPage
{
    public class IndexModel : PageModel
    {
        private readonly IStockPageService _stockPageService;
        private readonly IStockService _stockService;
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;

        public IndexModel(IStockPageService stockPageService, IStockService stockService, 
                         IUserService userService, IAuthenticationService authenticationService)
        {
            _stockPageService = stockPageService;
            _stockService = stockService;
            _userService = userService;
            _authenticationService = authenticationService;
        }

        public Stock? SelectedStock { get; private set; }
        public UserStock? OwnedStocks { get; private set; }
        public List<int> StockHistory { get; private set; } = [];
        public bool IsFavorite { get; private set; }
        public int UserGems { get; private set; }
        public string? ErrorMessage { get; private set; }
        public string? SuccessMessage { get; private set; }
        public decimal PriceChangePercentage { get; private set; }
        public bool IsAuthenticated => _authenticationService.IsUserLoggedIn();
        public bool CanBuy => IsAuthenticated && UserGems > 0 && SelectedStock != null;
        public bool CanSell => IsAuthenticated && OwnedStocks?.Quantity > 0;

        public async Task OnGetAsync(string stockName)
        {
            try
            {
                if (string.IsNullOrEmpty(stockName))
                {
                    ErrorMessage = "Stock name is required.";
                    return;
                }

                SelectedStock = await _stockService.GetStockByNameAsync(stockName);
                if (SelectedStock == null)
                {
                    ErrorMessage = "Stock not found.";
                    return;
                }

                await LoadStockDataAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading stock data: {ex.Message}";
            }
        }

        private async Task LoadStockDataAsync()
        {
            if (SelectedStock == null) return;

            try
            {
                // Load stock history
                StockHistory = await _stockPageService.GetStockHistoryAsync(SelectedStock.Name);
            
                // Calculate price change percentage
                if (StockHistory.Count > 1)
                {
                    var currentPrice = StockHistory.Last();
                    var previousPrice = StockHistory[^2];
                    PriceChangePercentage = previousPrice != 0 
                        ? (decimal)(currentPrice - previousPrice) * 100 / previousPrice 
                        : 0;
                }

                // Load user-specific data if authenticated
                if (IsAuthenticated)
                {
                    UserGems = await _userService.GetCurrentUserGemsAsync();
                    OwnedStocks = await _stockPageService.GetUserStockAsync(SelectedStock.Name);
                    IsFavorite = await _stockPageService.GetFavoriteAsync(SelectedStock.Name);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading stock details: {ex.Message}";
            }
        }

        public async Task<bool> BuyStockAsync(int quantity)
        {
            if (SelectedStock == null)
                throw new InvalidOperationException("Selected stock is not set");

            try
            {
                bool result = await _stockPageService.BuyStockAsync(SelectedStock.Name, quantity);
                if (result)
                {
                    await LoadStockDataAsync(); // Refresh data
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error buying stock: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> SellStockAsync(int quantity)
        {
            if (SelectedStock == null)
                throw new InvalidOperationException("Selected stock is not set");

            try
            {
                bool result = await _stockPageService.SellStockAsync(SelectedStock.Name, quantity);
                if (result)
                {
                    await LoadStockDataAsync(); // Refresh data
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error selling stock: {ex.Message}";
                return false;
            }
        }

        public async Task ToggleFavoriteAsync()
        {
            if (SelectedStock == null)
                throw new InvalidOperationException("Selected stock is not set");

            try
            {
                await _stockPageService.ToggleFavoriteAsync(SelectedStock.Name, !IsFavorite);
                IsFavorite = !IsFavorite;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error toggling favorite: {ex.Message}";
            }
        }

        public class StockTransactionDto
        {
            [Required]
            [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
            public int Quantity { get; set; } = 1;
        }
    }
}

using Common.Models;
using Common.Services;

namespace BankAppWeb.Models
{
    /// <summary>
    /// ViewModel for the homepage, tailored for ASP.NET Core MVC.
    /// Contains stocks to display, filtering and sorting options, and whether the user is a guest.
    /// </summary>
    public class HomepageViewModel
    {
        private readonly IStockService stockService;
        private readonly IAuthenticationService authenticationService;

        public List<HomepageStock> FilteredStocks { get; private set; } = [];
        public List<HomepageStock> FilteredFavoriteStocks { get; private set; } = [];

        public string SearchQuery { get; set; } = string.Empty;
        public string SelectedSortOption { get; set; } = string.Empty;
        public bool IsGuestUser { get; private set; }

        public HomepageViewModel(IStockService stockService, IAuthenticationService authenticationService)
        {
            this.stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        public async Task LoadStocksAsync()
        {
            IsGuestUser = !authenticationService.IsUserLoggedIn();
            if (IsGuestUser)
            {
                return;
            }
            var stocks = await stockService.GetFilteredAndSortedStocksAsync(SearchQuery, SelectedSortOption, false);
            FilteredStocks = stocks;

            var favoriteStocks = await stockService.GetFilteredAndSortedStocksAsync(SearchQuery, SelectedSortOption, true);
            FilteredFavoriteStocks = favoriteStocks;
        }
    }
}

namespace BankAppDesktop.Views.Pages
{
    using Common.Models;
    using Common.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using BankAppDesktop.ViewModels;
    using System;
    using System.Threading.Tasks;
    using Common.Services.Trading;

    public sealed partial class HomePage : Page
    {
        private readonly IStockService stockService;
        private readonly IAuthenticationService authenticationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomePage"/> class.
        /// </summary>
        public HomePage(HomepageViewModel homepageViewModel, IStockService stockService, IAuthenticationService authenticationService)
        {
            this.ViewModel = homepageViewModel ?? throw new ArgumentNullException(nameof(homepageViewModel));
            this.stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));

            this.InitializeComponent();
            this.DataContext = homepageViewModel;

            // Initialize guest user status
            this.ViewModel.IsGuestUser = !this.authenticationService.IsUserLoggedIn();

            // Load stocks if user is logged in
            if (!this.ViewModel.IsGuestUser)
            {
                _ = this.LoadStocksAsync();
            }
        }

        /// <summary>
        /// Gets the view model for the homepage view.
        /// </summary>
        public HomepageViewModel ViewModel { get; }

        /// <summary>
        /// Gets whether the user can modify favorites.
        /// </summary>
        public bool CanModifyFavorites => !this.authenticationService.IsUserLoggedIn();

        /// <summary>
        /// Loads stocks data into the ViewModel.
        /// </summary>
        public async Task LoadStocksAsync()
        {
            var stocks = await this.stockService.GetFilteredAndSortedStocksAsync(this.ViewModel.SearchQuery, this.ViewModel.SelectedSortOption, false);
            this.ViewModel.FilteredStocks.Clear();
            foreach (var stock in stocks)
            {
                this.ViewModel.FilteredStocks.Add(stock);
            }

            var favoriteStocks = await this.stockService.GetFilteredAndSortedStocksAsync(this.ViewModel.SearchQuery, this.ViewModel.SelectedSortOption, true);
            this.ViewModel.FilteredFavoriteStocks.Clear();
            foreach (var stock in favoriteStocks)
            {
                this.ViewModel.FilteredFavoriteStocks.Add(stock);
            }
        }

        /// <summary>
        /// Applies filter and sort to the stock collections.
        /// </summary>
        private async Task ApplyFilterAndSortAsync()
        {
            var stocks = await this.stockService.GetFilteredAndSortedStocksAsync(this.ViewModel.SearchQuery, this.ViewModel.SelectedSortOption, false);
            this.ViewModel.FilteredStocks.Clear();
            foreach (var stock in stocks)
            {
                this.ViewModel.FilteredStocks.Add(stock);
            }

            var favoriteStocks = await this.stockService.GetFilteredAndSortedStocksAsync(this.ViewModel.SearchQuery, this.ViewModel.SelectedSortOption, true);
            this.ViewModel.FilteredFavoriteStocks.Clear();
            foreach (var stock in favoriteStocks)
            {
                this.ViewModel.FilteredFavoriteStocks.Add(stock);
            }
        }

        /// <summary>
        /// Toggles the favorite status of a stock.
        /// </summary>
        private async Task ToggleFavoriteAsync(HomepageStock? stock)
        {
            if (stock == null)
            {
                return;
            }

            if (stock.IsFavorite)
            {
                await this.stockService.RemoveFromFavoritesAsync(stock);
            }
            else
            {
                await this.stockService.AddToFavoritesAsync(stock);
            }

            await this.ApplyFilterAndSortAsync();
        }

        /// <summary>
        /// Handles the click event for the stock item in the homepage.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void GoToStock(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is not HomepageStock selectedStock)
            {
                throw new InvalidOperationException("Clicked item is not a valid stock");
            }

            // Navigate to the stock page using the selected stock's name
            if (App.MainAppWindow != null && App.MainAppWindow.MainAppFrame.Content is Page currentPage)
            {
                App.MainAppWindow.MainAppFrame.Content = App.Host.Services.GetService<StockPage>()
                    ?? throw new InvalidOperationException("StockPage is not registered in the service provider");
                if (App.MainAppWindow.MainAppFrame.Content is StockPage stockPage)
                {
                    stockPage.ViewModel.SelectedStock = selectedStock.StockDetails;
                    stockPage.PreviousPage = currentPage;
                }
                else
                {
                    throw new InvalidOperationException("Failed to navigate to StockPage");
                }
            }
        }

        /// <summary>
        /// Handles the search button click event.
        /// </summary>
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await ApplyFilterAndSortAsync();
        }

        /// <summary>
        /// Handles the favorite button click event.
        /// </summary>
        private async void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is HomepageStock stock)
            {
                await ToggleFavoriteAsync(stock);
            }
        }

        /// <summary>
        /// Handles the sort option selection changed event.
        /// </summary>
        private async void SortOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await ApplyFilterAndSortAsync();
        }
    }
}
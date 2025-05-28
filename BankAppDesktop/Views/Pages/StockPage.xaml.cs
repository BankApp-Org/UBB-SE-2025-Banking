namespace BankAppDesktop.Views.Pages
{
    using Common.Models;
    using Common.Services;
    using LiveChartsCore.SkiaSharpView;
    using LiveChartsCore.SkiaSharpView.Painting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using SkiaSharp;
    using BankAppDesktop.ViewModels;
    using BankAppDesktop.Views.Components;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StockPage : Page
    {
        private readonly IStockPageService stockPageService;
        private readonly IUserService userService;
        private readonly IAuthenticationService authenticationService;

        public Page? PreviousPage { get; set; }

        public StockPageViewModel ViewModel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StockPage"/> class.
        /// </summary>
        /// <param name="stockPageViewModel">The ViewModel for this Page.</param>
        public StockPage(StockPageViewModel stockPageViewModel, IStockPageService stockPageService, IUserService userService, IAuthenticationService authenticationService)
        {
            this.ViewModel = stockPageViewModel ?? throw new ArgumentNullException(nameof(stockPageViewModel));
            this.stockPageService = stockPageService ?? throw new ArgumentNullException(nameof(stockPageService));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));

            this.DataContext = this.ViewModel;
            this.InitializeComponent();

            // Subscribe to authentication events
            this.authenticationService.UserLoggedIn += (_, _) => UpdateAuthenticationStatus();
            this.authenticationService.UserLoggedOut += (_, _) => UpdateAuthenticationStatus();

            UpdateAuthenticationStatus();
        }

        private void UpdateAuthenticationStatus()
        {
            this.ViewModel.IsAuthenticated = this.authenticationService.IsUserLoggedIn();
            UpdateComputedProperties();
        }

        private void UpdateComputedProperties()
        {
            this.ViewModel.CanSell = this.ViewModel.IsAuthenticated && this.ViewModel.OwnedStocks?.Quantity > 0;
            this.ViewModel.CanBuy = this.ViewModel.IsAuthenticated && this.ViewModel.UserGems > 0;
        }

        /// <summary>
        /// Updates all displayed stock values, including price, change percentage, owned count, and chart.
        /// </summary>
        public async Task UpdateStockValue()
        {
            if (this.ViewModel.SelectedStock == null)
            {
                throw new InvalidOperationException("Selected stock is not set");
            }

            if (this.authenticationService.IsUserLoggedIn())
            {
                this.ViewModel.UserGems = await this.userService.GetCurrentUserGemsAsync();
                this.ViewModel.OwnedStocks = await this.stockPageService.GetUserStockAsync(this.ViewModel.SelectedStock.Name);
                this.ViewModel.IsFavorite = await this.stockPageService.GetFavoriteAsync(this.ViewModel.SelectedStock.Name);
            }
            List<decimal> stockHistory = await this.stockPageService.GetStockHistoryAsync(this.ViewModel.SelectedStock.Name);
            if (stockHistory.Count > 1)
            {
                decimal increasePerc = (stockHistory.Last() - stockHistory[^2]) * 100 / stockHistory[^2];
                // Handle percentage display logic here if needed
            }
            this.ViewModel.Series.Clear();
            this.ViewModel.Series.Add(new LineSeries<decimal>
            {
                Values = [.. stockHistory.TakeLast(30)],
                Fill = null,
                Stroke = new SolidColorPaint(SKColor.Parse("#4169E1"), 5),
                GeometryStroke = new SolidColorPaint(SKColor.Parse("#4169E1"), 5),
            });

            UpdateComputedProperties();
        }

        /// <summary>
        /// Toggles the favorite state of the stock.
        /// </summary>
        public async Task ToggleFavorite()
        {
            if (this.ViewModel.SelectedStock == null)
            {
                throw new InvalidOperationException("Selected stock is not set");
            }

            bool isFavorite = await this.stockPageService.GetFavoriteAsync(this.ViewModel.SelectedStock.Name);
            await this.stockPageService.ToggleFavoriteAsync(this.ViewModel.SelectedStock.Name, !isFavorite);
            this.ViewModel.IsFavorite = !isFavorite;
        }

        /// <summary>
        /// Attempts to buy a specified quantity of the stock.
        /// </summary>
        /// <param name="quantity">The number of shares to buy.</param>
        /// <returns><c>true</c> if the purchase succeeded; otherwise, <c>false</c>.</returns>
        public async Task<bool> BuyStock(int quantity)
        {
            if (this.ViewModel.SelectedStock == null)
            {
                throw new InvalidOperationException("Selected stock is not set");
            }
            bool res = await this.stockPageService.BuyStockAsync(this.ViewModel.SelectedStock.Name, quantity);
            await this.UpdateStockValue();
            return res;
        }

        /// <summary>
        /// Attempts to sell a specified quantity of the stock.
        /// </summary>
        /// <param name="quantity">The number of shares to sell.</param>
        /// <returns><c>true</c> if the sale succeeded; otherwise, <c>false</c>.</returns>
        public async Task<bool> SellStock(int quantity)
        {
            if (this.ViewModel.SelectedStock == null)
            {
                throw new InvalidOperationException("Selected stock is not set");
            }
            bool res = await this.stockPageService.SellStockAsync(this.ViewModel.SelectedStock.Name, quantity);
            await this.UpdateStockValue();
            return res;
        }

        public async Task ShowProfileDialog()
        {
            if (this.ViewModel.SelectedStock == null)
            {
                throw new InvalidOperationException("Selected stock is not set");
            }

            ContentDialog dialog = new()
            {
                Title = "Author",
                CloseButtonText = "OK",
                XamlRoot = App.MainAppWindow!.MainAppFrame.XamlRoot,
            };
            UserProfileComponent userProfile = App.Host.Services.GetService<UserProfileComponent>() ?? throw new InvalidOperationException("UserProfileComponent is not available");
            userProfile.ViewModel.User = await this.userService.GetUserByCnpAsync(this.ViewModel.SelectedStock.AuthorCNP);
            dialog.Content = userProfile;
            await dialog.ShowAsync();
        }

        public void OpenAlertsView()
        {
            if (this.ViewModel.SelectedStock == null)
            {
                throw new InvalidOperationException("Selected stock is not set");
            }

            AlertsPage alertsPage = App.Host.Services.GetService<AlertsPage>() ?? throw new InvalidOperationException("AlertsView is not available");
            alertsPage.ViewModel.SelectedStockName = this.ViewModel.SelectedStock.Name;
            if (App.MainAppWindow!.MainAppFrame.Content is Page currentPage)
            {
                alertsPage.PreviousPage = currentPage;
            }
            App.MainAppWindow.MainAppFrame.Content = alertsPage;
        }

        /// <summary>
        /// Handles the click event for the return button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void ReturnButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.PreviousPage is HomePage homepage)
            {
                await homepage.LoadStocksAsync();
            }

            App.MainAppWindow!.MainAppFrame.Content = this.PreviousPage ?? throw new InvalidOperationException("Previous page is not set");
        }

        /// <summary>
        /// Handles the click event for the favorite button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void FavoriteButtonClick(object sender, RoutedEventArgs e)
        {
            await this.ToggleFavorite();
        }

        /// <summary>
        /// Handles the click event for the alerts button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AlertsButtonClick(object sender, RoutedEventArgs e)
        {
            this.OpenAlertsView();
        }

        /// <summary>
        /// Handles the click event for the author button.
        /// </summary>
        public async void AuthorButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.authenticationService.IsUserAdmin())
            {
                await this.ShowProfileDialog();
            }
        }

        /// <summary>
        /// Handles the click event for the buy button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void BuyButtonClick(object sender, RoutedEventArgs e)
        {
            if (!this.ViewModel?.IsAuthenticated ?? true)
            {
                await this.ShowDialogAsync("Login Required", "You need to be logged in to buy stocks.");
                return;
            }

            int quantity = (int)this.QuantityInput.Value;
            bool success = await this.BuyStock(quantity);
            this.QuantityInput.Value = 1;

            if (!success)
            {
                await this.ShowDialogAsync("Not enough gems", "You don't have enough gems to buy this stock.");
            }
        }

        /// <summary>
        /// Handles the click event for the sell button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void SellButtonClick(object sender, RoutedEventArgs e)
        {
            if (!this.ViewModel?.IsAuthenticated ?? true)
            {
                await this.ShowDialogAsync("Login Required", "You need to be logged in to sell stocks.");
                return;
            }

            int quantity = (int)this.QuantityInput.Value;
            if (quantity <= 0)
            {
                await this.ShowDialogAsync("Invalid Quantity", "You must sell at least one stock.");
                return;
            }

            bool success = await this.SellStock(quantity);
            this.QuantityInput.Value = 1;

            if (!success)
            {
                await this.ShowDialogAsync("Not enough stocks", "You don't have enough stocks to sell.");
            }
        }

        /// <summary>
        /// Handles the stock selection change to update values.
        /// </summary>
        public async void OnStockChanged()
        {
            if (this.ViewModel.SelectedStock != null)
            {
                await this.UpdateStockValue();
            }
        }

        private async Task ShowDialogAsync(string title, string content)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot,
            };
            await dialog.ShowAsync();
        }
    }
}

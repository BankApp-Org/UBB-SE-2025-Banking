namespace StockApp.ViewModels
{
    using Common.Models;
    using Common.Services;
    using LiveChartsCore;
    using LiveChartsCore.SkiaSharpView;
    using LiveChartsCore.SkiaSharpView.Painting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml.Controls;
    using SkiaSharp;
    using StockApp.Commands;
    using StockApp.Views.Components;
    using StockApp.Views.Pages;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// ViewModel managing data and operations for the stock detail page.
    /// </summary>
    public partial class StockPageViewModel : INotifyPropertyChanged
    {
        private readonly IStockPageService stockPageService;
        private readonly IUserService userService;
        private readonly IAuthenticationService authenticationService;
        private int userGems = 0;
        private Stock? selectedStock;
        private UserStock? userStock;
        private bool isFavorite;

        public UserStock? OwnedStocks
        {
            get => this.userStock;
            set
            {
                this.userStock = value;
                this.UpdateComputedProperties();
                this.OnPropertyChanged(nameof(this.OwnedStocks));
            }
        }

        public bool IsFavorite
        {
            get => this.isFavorite;
            set
            {
                this.isFavorite = value;
                this.OnPropertyChanged(nameof(this.IsFavorite));
            }
        }

        public ObservableCollection<ISeries> Series { get; set; } = [];

        public ICommand AuthorCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StockPageViewModel"/> class with dependencies.
        /// </summary>
        /// <param name="stockPageService">The stock page service.</param>
        /// <param name="userService">The user service.</param>
        public StockPageViewModel(IUserService userService, IStockPageService stockPageService, IAuthenticationService authenticationService)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.authenticationService.UserLoggedIn += (_, _) => this.OnPropertyChanged(nameof(this.IsAuthenticated));
            this.authenticationService.UserLoggedOut += (_, _) => this.OnPropertyChanged(nameof(this.IsAuthenticated));
            this.stockPageService = stockPageService ?? throw new ArgumentNullException(nameof(stockPageService));
            this.AuthorCommand = new RelayCommand(async o => await this.ShowProfileDialog(), o => this.authenticationService.IsUserAdmin());
        }

        /// <summary>
        /// Updates all displayed stock values, including price, change percentage, owned count, and chart.
        /// </summary>
        public async Task UpdateStockValue()
        {
            if (this.selectedStock == null)
            {
                throw new InvalidOperationException("Selected stock is not set");
            }
            if (this.authenticationService.IsUserLoggedIn())
            {
                this.UserGems = await this.userService.GetCurrentUserGemsAsync();
                this.OwnedStocks = await this.stockPageService.GetUserStockAsync(this.selectedStock.Name);
                this.IsFavorite = await this.stockPageService.GetFavoriteAsync(this.selectedStock.Name);
            }

            List<int> stockHistory = await this.stockPageService.GetStockHistoryAsync(this.selectedStock.Name);
            if (stockHistory.Count > 1)
            {
                int increasePerc = (stockHistory.Last() - stockHistory[^2]) * 100 / stockHistory[^2];
                // this.increaseLabel.Text = increasePerc + "%";
                if (increasePerc > 0)
                {
                    // this.increaseLabel.Foreground = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    // this.increaseLabel.Foreground = new SolidColorBrush(Colors.IndianRed);
                }
            }

            this.Series.Clear();
            this.Series.Add(new LineSeries<int>
            {
                Values = [.. stockHistory.TakeLast(30)],
                Fill = null,
                Stroke = new SolidColorPaint(SKColor.Parse("#4169E1"), 5), // FIXME: make stroke color configurable
                GeometryStroke = new SolidColorPaint(SKColor.Parse("#4169E1"), 5),
            });

            // TODO: handle case where stockHistory is empty to prevent exceptions
        }

        /// <summary>
        /// Gets or sets the selected stock.
        /// </summary>
        public Stock? SelectedStock
        {
            get => this.selectedStock;
            set
            {
                if ((value != this.selectedStock) && (value != null))
                {
                    this.selectedStock = value;
                    _ = this.UpdateStockValue();
                    this.OnPropertyChanged(nameof(this.SelectedStock));
                }
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Toggles the favorite state of the stock.
        /// </summary>
        public async Task ToggleFavorite()
        {
            if (this.selectedStock == null)
            {
                throw new InvalidOperationException("Selected stock is not set");
            }

            bool isFavorite = await this.stockPageService.GetFavoriteAsync(this.selectedStock.Name);
            await this.stockPageService.ToggleFavoriteAsync(this.selectedStock.Name, !isFavorite);
            this.IsFavorite = !isFavorite;
            this.OnPropertyChanged(nameof(this.SelectedStock));
        }

        /// <summary>
        /// Gets a value indicating whether the user is a guest.
        /// </summary>
        public bool IsAuthenticated => this.authenticationService.IsUserLoggedIn();

        private bool canSell;
        private bool canBuy;

        public bool CanSell
        {
            get => this.canSell;
            private set
            {
                if (this.canSell != value)
                {
                    this.canSell = value;
                    this.OnPropertyChanged(nameof(this.CanSell));
                }
            }
        }

        public bool CanBuy
        {
            get => this.canBuy;
            private set
            {
                if (this.canBuy != value)
                {
                    this.canBuy = value;
                    this.OnPropertyChanged(nameof(this.CanBuy));
                }
            }
        }

        private void UpdateComputedProperties()
        {
            this.CanSell = this.IsAuthenticated && this.OwnedStocks?.Quantity > 0;
            this.CanBuy = this.IsAuthenticated && this.UserGems > 0;
        }

        /// <summary>
        /// Gets or sets the user's current gem balance.
        /// </summary>
        public int UserGems
        {
            get => this.userGems;
            set
            {
                this.userGems = value;
                this.UpdateComputedProperties();
                this.OnPropertyChanged(nameof(this.UserGems));
            }
        }

        /// <summary>
        /// Attempts to buy a specified quantity of the stock.
        /// </summary>
        /// <param name="quantity">The number of shares to buy.</param>
        /// <returns><c>true</c> if the purchase succeeded; otherwise, <c>false</c>.</returns>
        public async Task<bool> BuyStock(int quantity)
        {
            if (this.selectedStock == null)
            {
                throw new InvalidOperationException("Selected stock is not set");
            }
            bool res = await this.stockPageService.BuyStockAsync(this.selectedStock.Name, quantity);
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
            if (this.selectedStock == null)
            {
                throw new InvalidOperationException("Selected stock is not set");
            }
            bool res = await this.stockPageService.SellStockAsync(this.selectedStock.Name, quantity);
            await this.UpdateStockValue();
            return res;
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task ShowProfileDialog()
        {
            if (this.selectedStock == null)
            {
                throw new InvalidOperationException("Selected stock is not set");
            }
            if (this.stockPageService == null)
            {
                throw new InvalidOperationException("StockPageService is not initialized");
            }

            ContentDialog dialog = new()
            {
                Title = "Author",
                CloseButtonText = "OK",
                XamlRoot = App.MainAppWindow!.MainAppFrame.XamlRoot,
            };
            UserProfileComponent userProfile = App.Host.Services.GetService<UserProfileComponent>() ?? throw new InvalidOperationException("UserProfileComponent is not available");
            userProfile.ViewModel.User = await this.userService.GetUserByCnpAsync(this.selectedStock.AuthorCNP);
            dialog.Content = userProfile;
            await dialog.ShowAsync();
        }

        public void OpenAlertsView()
        {
            if (this.selectedStock == null)
            {
                throw new InvalidOperationException("Selected stock is not set");
            }

            AlertsPage alertsPage = App.Host.Services.GetService<AlertsPage>() ?? throw new InvalidOperationException("AlertsView is not available");
            alertsPage.ViewModel.SelectedStockName = this.selectedStock.Name;
            if (App.MainAppWindow!.MainAppFrame.Content is Page currentPage)
            {
                alertsPage.ViewModel.PreviousPage = currentPage;
            }
            App.MainAppWindow.MainAppFrame.Content = alertsPage;
        }
    }
}

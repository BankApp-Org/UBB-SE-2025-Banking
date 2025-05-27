namespace StockApp.Views.Pages
{
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.ViewModels;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StockPage : Page
    {
        public Page? PreviousPage { get; set; }

        public StockPageViewModel ViewModel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StockPage"/> class.
        /// </summary>
        /// <param name="stockPageViewModel">The ViewModel for this Page.</param>
        public StockPage(StockPageViewModel stockPageViewModel)
        {
            this.ViewModel = stockPageViewModel;
            this.DataContext = this.ViewModel;
            this.InitializeComponent();
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
                await homepage.ViewModel.LoadStocksAsync();
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
            await this.ViewModel.ToggleFavorite();
        }

        /// <summary>
        /// Handles the click event for the alerts button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AlertsButtonClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.OpenAlertsView();
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
            bool success = await this.ViewModel!.BuyStock(quantity);
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

            bool success = await this.ViewModel!.SellStock(quantity);
            this.QuantityInput.Value = 1;

            if (!success)
            {
                await this.ShowDialogAsync("Not enough stocks", "You don't have enough stocks to sell.");
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

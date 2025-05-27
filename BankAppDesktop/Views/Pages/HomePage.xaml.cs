namespace StockApp.Views.Pages
{
    using Common.Models;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.ViewModels;
    using System;

    public sealed partial class HomePage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HomePage"/> class.
        /// </summary>
        public HomePage(HomepageViewModel homepageViewModel)
        {
            this.ViewModel = homepageViewModel ?? throw new ArgumentNullException(nameof(homepageViewModel));
            this.InitializeComponent();
            this.DataContext = homepageViewModel;
            this.ViewModel = homepageViewModel;
        }

        /// <summary>
        /// Gets the view model for the homepage view.
        /// </summary>
        public HomepageViewModel ViewModel { get; }

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
    }
}
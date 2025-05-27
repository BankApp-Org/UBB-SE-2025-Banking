namespace StockApp.Pages
{
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Input;
    using StockApp.ViewModels;

    public sealed partial class NewsListPage : Page
    {
        /// <summary>
        /// The view model for the NewsListPage.
        /// </summary>
        public NewsListViewModel ViewModel { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewsListPage"/> class.
        /// </summary>
        public NewsListPage(NewsListViewModel newsListViewModel)
        {
            this.ViewModel = newsListViewModel;
            this.InitializeComponent();
        }

        private void RefreshContainerRefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            this.ViewModel.RefreshCommand.Execute(null);
        }

        private void EscapeKeyInvoked(KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            this.ViewModel.ClearSearchCommand.Execute(null);
            args.Handled = true;
        }

        private void CategoryFilterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ViewModel.SelectedCategory != null)
            {
                this.ViewModel.RefreshCommand.Execute(null);
            }
        }
    }
}
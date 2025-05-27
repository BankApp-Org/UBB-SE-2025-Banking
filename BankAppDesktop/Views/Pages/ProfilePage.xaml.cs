namespace StockApp.Views.Pages
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.Commands;
    using StockApp.ViewModels;
    using System;
    using System.Windows.Input;
    using Windows.UI.Popups;

    /// <summary>
    /// Represents the Profile Page of the application.
    /// </summary>
    public sealed partial class ProfilePage : Page
    {
        private readonly ProfilePageViewModel viewModel;

        public ProfilePageViewModel ViewModel => this.viewModel;

        /// <summary>
        /// Gets the command for updating the profile.
        /// </summary>
        private ICommand UpdateProfileButton { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfilePage"/> class.
        /// </summary>
        /// <param name="viewModel">The view model for the profile page.</param>
        public ProfilePage(ProfilePageViewModel viewModel)
        {
            this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            this.DataContext = this.viewModel;
            this.InitializeComponent();
            this.UpdateProfileButton = new StockNewsRelayCommand(() => this.GoToUpdatePage());
            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs ea)
        {
            if (this.viewModel.IsGuest)
            {
                ShowNoUserMessage();
                return;
            }
        }

        private static void ShowNoUserMessage()
        {
            MessageDialog dialog = new("No user profile available", "Error");
            dialog.Commands.Add(new UICommand("OK", null));
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 0;
            _ = dialog.ShowAsync();
        }

        private static void ShowErrorMessage(string message)
        {
            MessageDialog dialog = new(message, "Error");
            dialog.Commands.Add(new UICommand("OK", null));
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 0;
            _ = dialog.ShowAsync();
        }

        /// <summary>
        /// Navigates to the update profile page.
        /// </summary>
        private void GoToUpdatePage()
        {
            if (this.viewModel == null)
            {
                ShowErrorMessage("No user profile available");
                return;
            }

            UpdateProfilePage updateProfilePage = App.Host.Services.GetService<UpdateProfilePage>() ?? throw new InvalidOperationException("UpdateProfilePage is not available");
            updateProfilePage.PreviousPage = this;
            App.MainAppWindow!.MainAppFrame.Content = updateProfilePage;
        }

        /// <summary>
        /// Handles the click event for the "Go To Stock" button.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        /// <exception cref="InvalidOperationException">Thrown when no stock is selected.</exception>
        public void GoToStockButton(object sender, RoutedEventArgs e)
        {
            if (this.viewModel.SelectedStock == null)
            {
                ShowErrorMessage("No stock selected.");
                return;
            }

            StockPage stockPage = App.Host.Services.GetService<StockPage>() ?? throw new InvalidOperationException("StockPage is not available");
            stockPage.PreviousPage = this;
            stockPage.ViewModel.SelectedStock = this.viewModel.SelectedStock;
            App.MainAppWindow!.MainAppFrame.Content = stockPage;
        }
    }
}

namespace StockApp.Views.Pages
{
    using Common.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.ViewModels;
    using System;
    using Windows.UI.Popups;

    /// <summary>
    /// Represents the Profile Page of the application.
    /// </summary>
    public sealed partial class ProfilePage : Page
    {
        private readonly ProfilePageViewModel viewModel;
        private readonly IAuthenticationService authenticationService;

        public ProfilePageViewModel ViewModel => this.viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfilePage"/> class.
        /// </summary>
        /// <param name="viewModel">The view model for the profile page.</param>
        /// <param name="authenticationService">The authentication service.</param>
        public ProfilePage(ProfilePageViewModel viewModel, IAuthenticationService authenticationService)
        {
            this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.DataContext = this.viewModel;
            this.InitializeComponent();
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
        /// Handles the update profile button click event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void UpdateProfileButton_Click(object sender, RoutedEventArgs e)
        {
            GoToUpdatePage();
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

        /// <summary>
        /// Handles the logout button click event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private async void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await this.authenticationService.LogoutAsync();

                // Navigate to login page
                var loginPage = App.Host.Services.GetService<LoginPage>() ?? throw new InvalidOperationException("LoginPage is not available");
                App.MainAppWindow!.MainAppFrame.Content = loginPage;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error during logout: {ex.Message}");
            }
        }
    }
}

using Common.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace StockApp.Views.Components
{
    public sealed partial class UserAuthStatusComponent : UserControl
    {
        private readonly IAuthenticationService _authService;

        public UserAuthStatusComponent(IAuthenticationService authService)
        {
            this.InitializeComponent();
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            Loaded += UserAuthStatusComponent_Loaded;
        }

        private void UserAuthStatusComponent_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        public void UpdateUI()
        {
            var isLoggedIn = _authService.IsUserLoggedIn();
            var isAdmin = _authService.IsUserAdmin();

            if (isLoggedIn)
            {
                var user = _authService.GetCurrentUserSession();
                UserNameTextBlock.Text = user?.UserName ?? "User";
                LoginStatusTextBlock.Text = "Logged In";
                UserRoleTextBlock.Text = isAdmin ? "Admin" : "User";

                LogoutButton.Visibility = Visibility.Visible;
                LoginPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                UserNameTextBlock.Text = "Guest";
                LoginStatusTextBlock.Text = "Not Logged In";
                UserRoleTextBlock.Text = "Guest";

                LogoutButton.Visibility = Visibility.Collapsed;
                LoginPanel.Visibility = Visibility.Visible;
            }
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            await _authService.LogoutAsync();
            UpdateUI();
        }

        private void LoginComponent_LoginSuccessful(object sender, EventArgs e)
        {
            UpdateUI();
        }
    }
}
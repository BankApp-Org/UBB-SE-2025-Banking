using Common.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace StockApp.Views.Components
{
    public sealed partial class LoginComponent : UserControl
    {
        private readonly IAuthenticationService _authService;

        public LoginComponent(IAuthenticationService authService)
        {
            this.InitializeComponent();
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable the button to prevent multiple clicks
            LoginButton.IsEnabled = false;
            ErrorTextBlock.Text = string.Empty;

            try
            {
                // Get the username and password from the UI
                var username = UsernameTextBox.Text;
                var password = PasswordBox.Password;

                // Validate input
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    ErrorTextBlock.Text = "UserName and password are required.";
                    return;
                }

                // Attempt to login
                var session = await _authService.LoginAsync(username, password);
                if (session.IsLoggedIn)
                {
                    // Login successful - you might want to navigate to another page or update UI
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorTextBlock.Text = "Login failed. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorTextBlock.Text = $"Error: {ex.Message}";
            }
            finally
            {
                // Re-enable the button
                LoginButton.IsEnabled = true;
            }
        }

        // Event that can be subscribed to by parent components
        public event EventHandler? LoginSuccessful;
    }
}
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Common.Services;
using StockApp.ViewModels;
using System;

namespace StockApp.Views.Pages
{
    public sealed partial class LoginPage : Page
    {
        private readonly AuthenticationViewModel _viewModel;

        public LoginPage(IAuthenticationService authService)
        {
            this.InitializeComponent();
            _viewModel = new AuthenticationViewModel(authService);
            DataContext = _viewModel;
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Focus on username by default
            UsernameTextBox.Focus(FocusState.Programmatic);
        }

        private void LoginSuccessful(object sender, EventArgs e)
        {
            // This could navigate to another page or update UI
            // For example: Frame.Navigate(typeof(HomePage));
        }
    }
}
using Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BankAppDesktop.ViewModels;
using System;
using System.Threading.Tasks;

namespace BankAppDesktop.Views.Pages
{
    public sealed partial class LoginPage : Page
    {
        private readonly AuthenticationViewModel _viewModel;
        private readonly IAuthenticationService _authService;

        public LoginPage(IAuthenticationService authService)
        {
            this.InitializeComponent();
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _viewModel = new AuthenticationViewModel();
            DataContext = _viewModel;

            RefreshUserState();
        }

        public void RefreshUserState()
        {
            var currentUser = _authService.GetCurrentUserSession();
            _viewModel.IsLoggedIn = currentUser?.IsLoggedIn ?? false;
            _viewModel.IsAdmin = currentUser?.IsAdmin ?? false;
            _viewModel.CurrentUser = currentUser;
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(_viewModel.Username) &&
                   !string.IsNullOrWhiteSpace(_viewModel.Password) &&
                   !_viewModel.IsLoading;
        }

        private async Task LoginAsync()
        {
            if (!CanLogin())
            {
                return;
            }

            _viewModel.ErrorMessage = string.Empty;
            _viewModel.IsLoading = true;

            try
            {
                var currentUser = await _authService.LoginAsync(_viewModel.Username, _viewModel.Password);
                _viewModel.CurrentUser = currentUser;
                _viewModel.IsLoggedIn = currentUser.IsLoggedIn;
                _viewModel.IsAdmin = currentUser.IsAdmin;

                if (_viewModel.IsLoggedIn)
                {
                    // Clear credentials after successful login
                    _viewModel.Username = string.Empty;
                    _viewModel.Password = string.Empty;
                }
                else
                {
                    _viewModel.ErrorMessage = "Login failed. Please check your credentials.";
                }
            }
            catch (Exception ex)
            {
                _viewModel.ErrorMessage = $"Login error: {ex.Message}";
            }
            finally
            {
                _viewModel.IsLoading = false;
            }
        }

        private async Task LogoutAsync()
        {
            _viewModel.IsLoading = true;

            try
            {
                await _authService.LogoutAsync();
                RefreshUserState();
            }
            catch (Exception ex)
            {
                _viewModel.ErrorMessage = $"Logout error: {ex.Message}";
            }
            finally
            {
                _viewModel.IsLoading = false;
            }
        }

        private void NavigateToCreateProfile()
        {
            App.MainAppWindow!.MainAppFrame.Content = App.Host.Services.GetService<CreateProfilePage>();
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Focus on username by default
            UsernameTextBox.Focus(FocusState.Programmatic);
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            await LoginAsync();
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            await LogoutAsync();
        }

        private void CreateProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.IsLoggedIn)
            {
                NavigateToCreateProfile();
            }
        }

        private void LoginSuccessful(object sender, EventArgs e)
        {
            // This could navigate to another page or update UI
            // For example: Frame.Navigate(typeof(HomePage));
        }
    }
}
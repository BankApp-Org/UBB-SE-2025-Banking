using Common.Models;
using Common.Services;
using Microsoft.Extensions.DependencyInjection;
using StockApp.Commands;
using StockApp.Views.Pages;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StockApp.ViewModels
{
    public class AuthenticationViewModel : INotifyPropertyChanged
    {
        private readonly IAuthenticationService _authService;
        private UserSession? _currentUser;
        private bool _isLoggedIn;
        private bool _isAdmin;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        public event PropertyChangedEventHandler? PropertyChanged;

        public AuthenticationViewModel(IAuthenticationService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            LoginCommand = new RelayCommand(async o => await LoginAsync(), CanLogin);
            LogoutCommand = new RelayCommand(async o => await LogoutAsync(), o => IsLoggedIn);
            CreateProfileCommand = new RelayCommand(o => NavigateToCreateProfile(), o => !IsLoggedIn);

            RefreshUserState();
        }

        private static void NavigateToCreateProfile()
        {
            App.MainAppWindow!.MainAppFrame.Content = App.Host.Services.GetService<CreateProfilePage>();
        }

        public void RefreshUserState()
        {
            _currentUser = _authService.GetCurrentUserSession();
            IsLoggedIn = _currentUser?.IsLoggedIn ?? false;
            IsAdmin = _currentUser?.IsAdmin ?? false;
            OnPropertyChanged(nameof(CurrentUser));
        }

        public UserSession? CurrentUser
        {
            get => _currentUser;
            private set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            private set
            {
                if (_isLoggedIn != value)
                {
                    _isLoggedIn = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsAdmin
        {
            get => _isAdmin;
            private set
            {
                if (_isAdmin != value)
                {
                    _isAdmin = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                    RelayCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
                    RelayCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand LoginCommand { get; }

        public ICommand LogoutCommand { get; }

        public ICommand CreateProfileCommand { get; }

        private bool CanLogin(object o)
        {
            return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password) && !IsLoading;
        }

        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;
            IsLoading = true;

            try
            {
                CurrentUser = await _authService.LoginAsync(Username, Password);
                IsLoggedIn = CurrentUser.IsLoggedIn;
                IsAdmin = CurrentUser.IsAdmin;

                if (IsLoggedIn)
                {
                    // Clear credentials after successful login
                    Username = string.Empty;
                    Password = string.Empty;
                }
                else
                {
                    ErrorMessage = "Login failed. Please check your credentials.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LogoutAsync()
        {
            IsLoading = true;

            try
            {
                await _authService.LogoutAsync();
                RefreshUserState();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Logout error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
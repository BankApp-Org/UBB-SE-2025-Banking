using System;
using System.Threading.Tasks;
using System.Windows;
using Common.Services;
using Common.Models;
using Common.Services.Bank;

namespace BankAppDesktop.ViewModels
{
    public class DeleteAccountViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authService;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        public Action? OnClose { get; set; }

        public DeleteAccountViewModel(IUserService userService, IAuthenticationService authService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public async Task<bool> DeleteAccount(string password)
        {
            try
            {
                IsLoading = true;
                var currentUser = _authService.GetCurrentUserSession();
                if (currentUser == null)
                {
                    ErrorMessage = "You must be logged in to delete your account.";
                    return false;
                }

                var result = await _userService.DeleteUser(password);
                if (result == "User deleted")
                {
                    _authService.LogoutAsync();
                    // Do not close the window here; let the View handle it after showing dialogs
                    return true;
                }
                else
                {
                    ErrorMessage = result;
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 
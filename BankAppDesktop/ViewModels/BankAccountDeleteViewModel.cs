using BankAppDesktop.Commands;
using Common.Models;
using Common.Services;
using Common.Services.Bank;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BankAppDesktop.ViewModels
{
    /// <summary>
    /// ViewModel for the bank account deletion confirmation view
    /// </summary>
    public partial class BankAccountDeleteViewModel : ViewModelBase
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly IAuthenticationService _authService;
        private string _iban = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;
        private UserSession? _currentUser;

        public ICommand NoCommand { get; }
        public ICommand YesCommand { get; }

        public Action? OnClose { get; set; }

        public BankAccountDeleteViewModel(IBankAccountService bankAccountService, IAuthenticationService authService)
        {
            _bankAccountService = bankAccountService ?? throw new ArgumentNullException(nameof(bankAccountService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _currentUser = authService.GetCurrentUserSession();
            _iban = _currentUser?.CurrentBankAccountIban ?? string.Empty;
            NoCommand = new RelayCommand(_ => OnNoButtonClicked());
            YesCommand = new RelayCommand(async _ => await OnYesButtonClickedAsync());
        }

        public UserSession? CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
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

        private void OnNoButtonClicked()
        {
            Debug.WriteLine("Cancel delete");
            OnClose?.Invoke();
        }

        private async Task OnYesButtonClickedAsync()
        {
            Debug.WriteLine("Confirm delete");
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(_iban))
                {
                    ErrorMessage = "Invalid account IBAN";
                    return;
                }

                // Call service to delete the account
                bool result = await _bankAccountService.RemoveBankAccount(_iban);

                if (result)
                {
                    // Navigate back to account list or main page
                    OnClose?.Invoke();
                }
                else
                {
                    ErrorMessage = "Failed to delete account. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting account: {ex.Message}";
                Debug.WriteLine($"Error deleting account: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}

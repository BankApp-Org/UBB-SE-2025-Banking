using System;
using System.Diagnostics;
using System.Windows.Input;
using BankAppDesktop.Commands;
using Common.Models;
using Common.Services.Bank;
using Common.Services;
using BankAppDesktop.Views.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace BankAppDesktop.ViewModels
{
    /// <summary>
    /// ViewModel for the bank account deletion confirmation view
    /// </summary>
    public class BankAccountDeleteViewModel : ViewModelBase
    {
        private readonly IBankAccountService _bankAccountService;
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
            _currentUser = authService.GetCurrentUserSession();
            _iban = _currentUser?.CurrentBankAccountIban ?? string.Empty;
            NoCommand = new RelayCommand(_ => OnNoButtonClicked());
            YesCommand = new RelayCommand(_ => OnYesButtonClicked());
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

        private void OnYesButtonClicked()
        {
            Debug.WriteLine("Confirm delete");
            // UNCOMMENT THIS AFTER VERIFY VIEW IS ADDED TO THE PROJECT
            // var verifyPage = App.Host.Services.GetService<BankAccountVerifyView>();
            // if (verifyPage != null)
            // {
            //    App.MainAppWindow!.MainAppFrame.Content = verifyPage;
            // }
            OnClose?.Invoke();
        }
    }
}

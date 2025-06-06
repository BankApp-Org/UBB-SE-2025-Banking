using BankAppDesktop.Commands;
using BankAppDesktop.Views.Pages;
using Common.Models;
using Common.Models.Bank;
using Common.Services;
using Common.Services.Bank;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BankAppDesktop.ViewModels
{
    /// <summary>
    /// ViewModel for displaying and managing a list of bank accounts
    /// </summary>
    public class BankAccountVerifyViewModel : INotifyPropertyChanged
    {
        public Action OnSuccess { get; set; }
        public Action OnFailure { get; set; }

        /// <summary>
        /// Command for the back button to return to the previous view
        /// </summary>
        public ICommand BackCommand { get; }

        /// <summary>
        /// Command for the confirm button to verify credentials and delete the account
        /// </summary>
        public ICommand ConfirmCommand { get; }

        /// <summary>
        /// Action to be invoked when the view should be closed
        /// </summary>
        public Action? OnClose { get; set; }

        private IBankAccountService service;
        private IAuthenticationService authenticationService;
        private UserSession user;
        private string iban;
        private string? passwordInput;
        private string email;

        /// <summary>
        /// The password entered by the user for verification
        /// </summary>
        public string Password
        {
            get
            {
                return passwordInput ?? string.Empty;
            }
            set
            {
                passwordInput = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        /// <summary>
        /// Initializes a new instance of the BankAccountVerifyViewModel class
        /// </summary>
        public BankAccountVerifyViewModel(IBankAccountService s, IAuthenticationService a)
        {
            this.service = s;
            this.authenticationService = a;
            user = authenticationService.GetCurrentUserSession();
            email = string.Empty;
            iban = user.CurrentBankAccountIban ?? string.Empty;
        }

        /// <summary>
        /// Event that is raised when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">The name of the property that changed</param>
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

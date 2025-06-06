using BankAppDesktop.Commands;
using Catel.Data;
using System;
using System.Windows;
using System.Windows.Input;

namespace BankAppDesktop.ViewModels
{
    public partial class BankTransactionsViewModel : ObservableObject
    {
        public ICommand CloseCommand { get; }
        public ICommand SendMoneyCommand { get; }
        public ICommand PayLoanCommand { get; }
        public ICommand CurrencyExchangeCommand { get; }

        public Action CloseAction { get; set; }

        public BankTransactionsViewModel()
        {
            CloseCommand = new RelayCommand(_ => CloseWindow());
            SendMoneyCommand = new RelayCommand(_ => OpenSendMoneyWindow());
            PayLoanCommand = new RelayCommand(_ => OpenPayLoanWindow());
            CurrencyExchangeCommand = new RelayCommand(_ => OpenCurrencyExchangeWindow());
        }

        private void OpenSendMoneyWindow()
        {
            throw new Exception("Send Money Not Migrated!");
            // OpenChildWindow(new SendMoneyView());
        }

        private void OpenPayLoanWindow()
        {
            throw new Exception("Loan View Not Migrated!");
            // OpenChildWindow(new Views.Windows.LoanView());
        }

        private void OpenCurrencyExchangeWindow()
        {
            throw new Exception("CurrencyExchangeTableView Not Migrated!");
            // OpenChildWindow(new CurrencyExchangeTableView());
        }

        private void OpenChildWindow(Window childWindow)
        {
            CloseWindow();
            childWindow.Activate();
        }

        private void CloseWindow()
        {
            CloseAction?.Invoke();
        }
    }
}

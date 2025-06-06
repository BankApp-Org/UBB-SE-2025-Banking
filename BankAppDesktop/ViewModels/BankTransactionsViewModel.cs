using BankApi.Repositories.Impl.Bank;
using BankAppDesktop.Commands;
using BankAppDesktop.Views.Pages;
using Catel.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Input;
using Windows.Media.Devices;

namespace BankAppDesktop.ViewModels
{
    public partial class BankTransactionsViewModel : ObservableObject
    {
        public ICommand CloseCommand { get; }
        public ICommand SendMoneyCommand { get; }
        public ICommand PayLoanCommand { get; }
        public ICommand CurrencyExchangeCommand { get; }

        public Action CloseAction { get; set; }

        public string Iban { get; set; }

        public BankTransactionsViewModel(string iban)
        {
            this.Iban = iban;
            CloseCommand = new RelayCommand(_ => CloseWindow());
            SendMoneyCommand = new RelayCommand(_ => OpenSendMoneyWindow());
            PayLoanCommand = new RelayCommand(_ => OpenPayLoanWindow());
            CurrencyExchangeCommand = new RelayCommand(_ => OpenCurrencyExchangeWindow());
        }

        private void OpenSendMoneyWindow()
        {
            SendMoneyViewModel sendViewModel = App.Services.GetRequiredService<SendMoneyViewModel>();
            sendViewModel.SelectedBankAccountIban = Iban;
            App.MainAppWindow.MainAppFrame.Content = new SendMoneyView(sendViewModel);
        }

        private void OpenPayLoanWindow()
        {
            throw new Exception("Pay loan not implemented!");
        }

        private void OpenCurrencyExchangeWindow()
        {
            App.MainAppWindow.MainAppFrame.Content = new CurrencyExchangeTable(Iban);
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

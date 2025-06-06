using BankAppDesktop.Commands;
using Common.Models.Bank;
using Common.Services.Bank;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BankAppDesktop.ViewModels
{
    public class CurrencyExchangeViewModel
    {
        private readonly IBankAccountService transactionService;

        public ObservableCollection<CurrencyExchange> ExchangeRates { get; } = new ObservableCollection<CurrencyExchange>();

        public ICommand CloseCommand { get; }

        public Action CloseAction { get; set; }

        public CurrencyExchangeViewModel(IBankAccountService transactionService)
        {
            this.transactionService = transactionService;

            LoadExchangeRatesAsync();

            CloseCommand = new RelayCommand(_ => CloseWindow());
        }

        private async void LoadExchangeRatesAsync()
        {
            var rates = await this.transactionService.GetAllExchangeRatesAsync();

            ExchangeRates.Clear();
            foreach (var rate in rates)
            {
                ExchangeRates.Add(rate);
            }
        }

        private void CloseWindow()
        {
            CloseAction?.Invoke();
        }
    }
}

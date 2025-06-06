using BankApi.Services.Trading;
using Common.Models.Bank;
using Common.Services;
using Common.Services.Bank;
using Common.Services.Proxy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace BankAppDesktop.ViewModels
{
    public class BankTransactionsHistoryViewModel
    {
        private readonly IBankTransactionService _bankTranscationHistoryService;
        private readonly IAuthenticationService authService;
        public BankTransactionsHistoryViewModel(IBankTransactionService s, IAuthenticationService authService)
        {
            this._bankTranscationHistoryService = s;
            this.authService = authService;
        }

        // retrieveForMenu() returns a list of transactions formatted for the menu
        public async Task<ObservableCollection<string>> RetrieveForMenu()
        {
            var results = await GetAllTransactions(null);

            List<string> answer = new List<string>();

            foreach (var transaction in results)
            {
                answer.Add(
                    transaction.SenderIban + "\n" +
                    transaction.ReceiverIban + "\n" +
                    transaction.SenderAmount.ToString() + "\n" +
                    transaction.ReceiverAmount.ToString() + "\n" +
                    transaction.TransactionDatetime.ToString() + "\n" +
                    transaction.TransactionType.ToString() + "\n");
            }

            return new ObservableCollection<string>(answer);
        }

        // FilterByTypeForMenu() returns a list of transactions formatted for the menu filtered by the transaction type
        public async Task<ObservableCollection<string>> FilterByTypeForMenu(string filter)
        {
            var results = await GetAllTransactions(filter);

            List<string> answer = new List<string>();

            foreach (var transaction in results)
            {
                answer.Add(
                    transaction.SenderIban + "\n" +
                    transaction.ReceiverIban + "\n" +
                    transaction.SenderAmount.ToString() + "\n" +
                    transaction.ReceiverAmount.ToString() + "\n" +
                    transaction.TransactionDatetime.ToString() + "\n" +
                    transaction.TransactionType.ToString() + "\n");
            }

            return new ObservableCollection<string>(answer);
        }

        // CreateCSV() creates a CSV file with the transactions
        public async void CreateCSV()
        {
            string iban = authService.GetCurrentUserSession()?.CurrentBankAccountIban ?? string.Empty;

            bool answer = await ((BankTransactionProxyService)_bankTranscationHistoryService).CreateCSV(iban);
        }

        // GetTransactionTypeCounts() returns a dictionary with the transaction type counts
        public async Task<Dictionary<string, int>> GetTransactionTypeCounts()
        {
            List<BankTransaction> transactions = await this.GetAllTransactions(null);

            return transactions
                .GroupBy(t => t.TransactionType)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());
        }

        private async Task<List<BankTransaction>> GetAllTransactions(string? filter)
        {
            string iban = authService.GetCurrentUserSession()?.CurrentBankAccountIban ?? string.Empty;

            if (string.IsNullOrEmpty(iban))
            {
                throw new Exception("Empty IBAN");
            }

            // Normalize the filter
            string normalizedFilter = filter?.ToLowerInvariant() ?? string.Empty;

            // Get all matching enum values based on string match
            var matchingTypes = Enum.GetValues(typeof(Common.Models.Bank.TransactionType))
                .Cast<Common.Models.Bank.TransactionType>()
                .Where(t => t.ToString().ToLowerInvariant().Contains(normalizedFilter) || string.IsNullOrEmpty(normalizedFilter))
                .ToList();

            List<BankTransaction> results = new List<BankTransaction>();

            foreach (var type in matchingTypes)
            {
                TransactionFilters filters = new TransactionFilters
                {
                    Type = type,
                    SenderIban = iban,
                    StartDate = new DateTime(2000, 1, 1),
                    EndDate = new DateTime(3000, 1, 1),
                };

                var transactions1 = await _bankTranscationHistoryService.GetTransactions(filters);

                filters.SenderIban = string.Empty;
                filters.ReceiverIban = iban;

                var transactions2 = await _bankTranscationHistoryService.GetTransactions(filters);

                results.AddRange(transactions1);
                results.AddRange(transactions2);
            }

            return results;
        }
    }
}

using BankAppDesktop.Commands;
using Common.Services;
using Common.Services.Bank;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace BankAppDesktop.ViewModels
{
    public partial class TransactionHistoryChartViewModel : INotifyPropertyChanged
    {
        private readonly IBankTransactionService _transactionHistoryService;
        private readonly IAuthenticationService _authenticationService;
        private IEnumerable<ISeries> _transactionTypeCounts;
        private ICommand _backCommand;

        public TransactionHistoryChartViewModel(IBankTransactionService transactionHistoryService, IAuthenticationService authenticationService)
        {
            _transactionHistoryService = transactionHistoryService;
            _authenticationService = authenticationService;
            _backCommand = new RelayCommand(ExecuteBack);
            LoadData();
        }

        public IEnumerable<ISeries> TransactionTypeCounts
        {
            get => _transactionTypeCounts;
            set
            {
                _transactionTypeCounts = value;
                OnPropertyChanged();
            }
        }

        public ICommand BackCommand => _backCommand;

        private async void LoadData()
        {
            int currentUserId = int.Parse(_authenticationService.GetCurrentUserSession()?.UserId ?? throw new Exception("User not authenticated"));
            var transactionTypeCounts = await _transactionHistoryService.GetTransactionTypeCounts(currentUserId);

            TransactionTypeCounts = [.. transactionTypeCounts.Select(tc =>
                new PieSeries<int>
                {
                    Name = tc.TransactionType.ToString(),
                    Values = [tc.Count],
                    DataLabelsSize = 16,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle
                })];
        }

        private void ExecuteBack(object parameter)
        {
            // Navigare înapoi
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
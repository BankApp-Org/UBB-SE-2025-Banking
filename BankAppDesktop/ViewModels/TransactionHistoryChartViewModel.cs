using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BankAppDesktop.Commands;
using BankAppDesktop.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Linq;

namespace BankAppDesktop.ViewModels
{
    public class TransactionHistoryChartViewModel : INotifyPropertyChanged
    {
        private readonly ITransactionHistoryService _transactionHistoryService;
        private IEnumerable<ISeries> _transactionTypeCounts;
        private ICommand _backCommand;

        public TransactionHistoryChartViewModel(ITransactionHistoryService transactionHistoryService)
        {
            _transactionHistoryService = transactionHistoryService;
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
            var userId = "current-user-id"; // Înlocuiește cu userId real
            var transactionTypeCounts = await _transactionHistoryService.GetTransactionTypeCounts(userId);

            TransactionTypeCounts = transactionTypeCounts.Select(tc =>
                new PieSeries<int>
                {
                    Name = tc.TransactionType.Name,
                    Values = new[] { tc.Count },
                    DataLabelsSize = 16,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle
                }
            ).ToArray();
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
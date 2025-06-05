using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BankAppDesktop.Commands;
using BankAppDesktop.Services;
using LiveCharts;
using LiveCharts.Wpf;
using System.Linq;

namespace BankAppDesktop.ViewModels
{
    public class TransactionHistoryChartViewModel : INotifyPropertyChanged
    {
        private readonly ITransactionHistoryService _transactionHistoryService;
        private SeriesCollection _transactionTypeCounts;
        private ICommand _backCommand;

        public TransactionHistoryChartViewModel(ITransactionHistoryService transactionHistoryService)
        {
            _transactionHistoryService = transactionHistoryService;
            _backCommand = new RelayCommand(ExecuteBack);
            LoadData();
        }

        public SeriesCollection TransactionTypeCounts
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

            TransactionTypeCounts = new SeriesCollection();
            foreach (var tc in transactionTypeCounts)
            {
                TransactionTypeCounts.Add(new PieSeries
                {
                    Title = tc.TransactionType.Name,
                    Values = new ChartValues<int> { tc.Count },
                    DataLabels = true
                });
            }
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
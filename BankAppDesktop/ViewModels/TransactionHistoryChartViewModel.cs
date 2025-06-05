using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BankApp.Service.Interfaces;
using System.Windows.Input;
using BankAppDesktop.Commands;

namespace BankAppDesktop.ViewModels
{
    public class TransactionHistoryChartViewModel : INotifyPropertyChanged
    {
        private readonly ITransactionHistoryService _transactionHistoryService;
        private Dictionary<string, int> _transactionTypeCounts;
        private ICommand _backCommand;

        public TransactionHistoryChartViewModel(ITransactionHistoryService transactionHistoryService)
        {
            _transactionHistoryService = transactionHistoryService;
            _backCommand = new RelayCommand(ExecuteBack);
            LoadData();
        }

        public Dictionary<string, int> TransactionTypeCounts
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
            // Note: In desktop app, we'll need to get the current user ID from the authentication service
            var userId = "current-user-id"; // Replace with actual user ID from auth service
            TransactionTypeCounts = await _transactionHistoryService.GetTransactionTypeCounts(userId);
        }

        private void ExecuteBack(object parameter)
        {
            // Navigate back to transaction history
            // Implementation depends on your navigation system
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 
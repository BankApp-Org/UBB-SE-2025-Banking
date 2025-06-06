using BankAppDesktop.Commands;
using Common.Models.Bank;
using Common.Services.Bank;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace BankAppDesktop.ViewModels
{
    public partial class BankAccountCreateViewModel : INotifyPropertyChanged
    {
        public Action? OnClose { get; set; }
        public Action? OnSuccess { get; set; }
        public ObservableCollection<CurrencyItem> Currencies { get; set; } = new ObservableCollection<CurrencyItem>();
        public ICommand CancelCommand { get; }
        public ICommand ConfirmCommand { get; }
        public int UserID { get; set; }
        private CurrencyItem? selectedItem;
        public CurrencyItem? SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem != value)
                {
                    if (selectedItem != null)
                    {
                        selectedItem.IsChecked = false;
                    }
                    selectedItem = value;
                    if (selectedItem != null)
                    {
                        selectedItem.IsChecked = true;
                    }
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }
        private string? customName;
        public string? CustomName
        {
            get
            {
                return customName ?? string.Empty;
            }

            set
            {
                customName = value;
                OnPropertyChanged(nameof(CustomName));
            }
        }
        private IBankAccountService service;
        public BankAccountCreateViewModel(IBankAccountService s)
        {
            service = s;
            LoadData();
            ConfirmCommand = new RelayCommand(_ => OnConfirmButtonClicked());
            CancelCommand = new RelayCommand(_ => OnCancelButtonClicked());
        }
        public async void OnConfirmButtonClicked()
        {
            Debug.WriteLine($"Pressed create confirm bank account: {SelectedItem?.Name}");
            if (SelectedItem != null)
            {
                string iban = await service.GenerateIBAN();
                var bankAccount = new BankAccount
                {
                    Iban = iban,
                    UserId = UserID,
                    Name = CustomName ?? string.Empty,
                    Currency = Enum.TryParse<Currency>(SelectedItem.Name, out var currency) ? currency : Currency.USD,
                    Balance = 0,
                    Blocked = false,
                    DailyLimit = 1000.0m,
                    MaximumPerTransaction = 200.0m,
                    MaximumNrTransactions = 10,
                    Transactions = new List<BankTransaction>(),
                    User = null!
                };
                await service.CreateBankAccount(bankAccount);
                // WindowManager.ShouldReloadBankAccounts = true; // Uncomment if WindowManager is implemented
                OnSuccess?.Invoke();
            }
            else
            {
                Debug.WriteLine("Bank account creation failed because no currency was selected");
            }
        }
        public void OnCancelButtonClicked()
        {
            Debug.WriteLine("Pressed cancel create bank account");
            // WindowManager.ShouldReloadBankAccounts = false; // Uncomment if WindowManager is implemented
            OnClose?.Invoke();
        }
        public void LoadData()
        {
            var currencyList = Enum.GetNames(typeof(Currency));
            foreach (var c in currencyList)
            {
                Currencies.Add(new CurrencyItem { Name = c });
            }
            if (Currencies.Count > 0)
            {
                SelectedItem = Currencies.FirstOrDefault(c => c.Name == "RON") ?? Currencies[0];
                OnPropertyChanged(nameof(SelectedItem));
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public partial class CurrencyItem : INotifyPropertyChanged
    {
        public string Name { get; set; } = string.Empty;
        private bool isChecked;
        public bool IsChecked
        {
            get => isChecked;
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
                }
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
namespace StockApp.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using StockApp.Commands;
    using Common.Models;
    using Common.Services;

    public class LoansViewModel
    {
        private readonly ILoanService loansService;

        public event EventHandler? LoansUpdated;
        public event EventHandler? ShowCreateLoanDialog;

        public ObservableCollection<Loan> Loans { get; set; } = [];

        public ICommand LoadLoansCommand { get; private set; }
        public ICommand OnLoansUpdatedCommand { get; private set; }
        public ICommand ShowCreateLoanDialogCommand { get; private set; }

        public bool IsLoading { get; private set; } = false;

        public LoansViewModel(ILoanService loansService)
        {
            this.loansService = loansService;
            this.LoadLoansCommand = new RelayCommand(async sender => await this.LoadLoans(), sender => !this.IsLoading);
            this.OnLoansUpdatedCommand = new RelayCommand(async sender => await this.LoadLoans(), sender => !this.IsLoading);
            this.ShowCreateLoanDialogCommand = new RelayCommand(sender => this.OnShowCreateLoanDialog());
        }

        private void OnShowCreateLoanDialog()
        {
            this.ShowCreateLoanDialog?.Invoke(this, EventArgs.Empty);
        }

        private async Task LoadLoans()
        {
            this.IsLoading = true;
            this.Loans.Clear();
            try
            {
                List<Loan> loans = await this.loansService.GetLoansAsync();
                loans.ForEach(this.Loans.Add);
                this.LoansUpdated?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error - LoadLoans: {exception.Message}");
                throw;
            }
            finally
            {
                this.IsLoading = false;
            }
        }
    }
}

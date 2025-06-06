namespace BankAppDesktop.ViewModels
{
    using Common.Models.Bank;
    using System.Collections.ObjectModel;

    /// <summary>
    /// ViewModel for loans, containing only data properties.
    /// </summary>
    public partial class LoansViewModel : ViewModelBase
    {
        private ObservableCollection<Loan> loans = [];
        private bool isLoading = false;

        public ObservableCollection<Loan> Loans
        {
            get => this.loans;
            set => this.SetProperty(ref this.loans, value);
        }

        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetProperty(ref this.isLoading, value);
        }

        public LoansViewModel()
        {
        }
    }
}

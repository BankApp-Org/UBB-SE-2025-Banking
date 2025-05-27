namespace StockApp.ViewModels
{
    using System.Collections.ObjectModel;
    using Common.Models;

    /// <summary>
    /// ViewModel for loans, containing only data properties.
    /// </summary>
    public class LoansViewModel : ViewModelBase
    {
        private ObservableCollection<Loan> loans = [];
        private bool isLoading = false;

        public event PropertyChangedEventHandler? PropertyChanged;

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

        /// <summary>
        /// Sets the property value and raises the PropertyChanged event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">Reference to the backing field.</param>
        /// <param name="value">The new value to set.</param>
        /// <param name="propertyName">The name of the property (automatically provided by the compiler).</param>
        /// <returns>True if the property value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

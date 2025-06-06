namespace BankAppDesktop.ViewModels
{
    using Common.Models.Bank;
    using Common.Models.Trading;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// ViewModel for the store page, managing gem deals and user gem balance UI state.
    /// Contains only data properties and UI state management - business logic handled in code-behind.
    /// </summary>
    public partial class StoreViewModel : ViewModelBase
    {
        private int userGems;
        private ObservableCollection<GemDeal> availableDeals = [];
        private List<GemDeal> possibleDeals = [];
        private bool isLoading = false;
        private string errorMessage = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreViewModel"/> class.
        /// </summary>
        public StoreViewModel()
        {
        }

        /// <summary>
        /// Gets or sets the user's current gem balance.
        /// </summary>
        public int UserGems
        {
            get => this.userGems;
            set => this.SetProperty(ref this.userGems, value);
        }

        /// <summary>
        /// Gets or sets the collection of available gem deals.
        /// </summary>
        public ObservableCollection<GemDeal> AvailableDeals
        {
            get => this.availableDeals;
            set => this.SetProperty(ref this.availableDeals, value);
        }

        /// <summary>
        /// Gets or sets the collection of possible gem deals that can be generated.
        /// </summary>
        public List<GemDeal> PossibleDeals
        {
            get => this.possibleDeals;
            set => this.SetProperty(ref this.possibleDeals, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the view model is currently loading data.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetProperty(ref this.isLoading, value);
        }

        /// <summary>
        /// Gets or sets the current error message to display to the user.
        /// </summary>
        public string ErrorMessage
        {
            get => this.errorMessage;
            set => this.SetProperty(ref this.errorMessage, value);
        }

        /// <summary>
        /// Gets a list of the user's linked bank accounts for UI binding.
        /// </summary>
        public List<BankAccount> UserBankAccounts;

        /// <summary>
        /// Sets the property and raises the PropertyChanged event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="field">The field to set.</param>
        /// <param name="value">The new value.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>True if the property was set; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}

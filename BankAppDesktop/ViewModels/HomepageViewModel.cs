namespace BankAppDesktop.ViewModels
{
    using Common.Models;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// ViewModel for the homepage, containing only data for stock display.
    /// </summary>
    public partial class HomepageViewModel : ViewModelBase
    {
        private ObservableCollection<HomepageStock> filteredStocks = [];
        private ObservableCollection<HomepageStock> filteredFavoriteStocks = [];
        private string searchQuery = string.Empty;
        private string selectedSortOption = string.Empty;
        private bool isGuestUser;
        private bool isLoading;
        private string errorMessage = string.Empty;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomepageViewModel"/> class.
        /// </summary>
        public HomepageViewModel()
        {
        }

        /// <summary>
        /// Gets or sets the collection of filtered stocks.
        /// </summary>
        public ObservableCollection<HomepageStock> FilteredStocks
        {
            get => this.filteredStocks;
            set => this.SetProperty(ref this.filteredStocks, value);
        }

        /// <summary>
        /// Gets or sets the collection of filtered favorite stocks.
        /// </summary>
        public ObservableCollection<HomepageStock> FilteredFavoriteStocks
        {
            get => this.filteredFavoriteStocks;
            set => this.SetProperty(ref this.filteredFavoriteStocks, value);
        }

        /// <summary>
        /// Gets or sets the search query for filtering stocks.
        /// </summary>
        public string SearchQuery
        {
            get => this.searchQuery;
            set => this.SetProperty(ref this.searchQuery, value);
        }

        /// <summary>
        /// Gets or sets the selected sort option.
        /// </summary>
        public string SelectedSortOption
        {
            get => this.selectedSortOption;
            set => this.SetProperty(ref this.selectedSortOption, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current user is a guest.
        /// </summary>
        public bool IsGuestUser
        {
            get => this.isGuestUser;
            set => this.SetProperty(ref this.isGuestUser, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether data is currently being loaded.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetProperty(ref this.isLoading, value);
        }

        /// <summary>
        /// Gets or sets the error message to display to the user.
        /// </summary>
        public string ErrorMessage
        {
            get => this.errorMessage;
            set => this.SetProperty(ref this.errorMessage, value);
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

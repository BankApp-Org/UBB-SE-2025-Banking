namespace BankAppDesktop.ViewModels
{
    using Common.Models.Trading;
    using Microsoft.UI.Xaml.Media.Imaging;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// ViewModel for the profile page, managing user profile UI state and data properties.
    /// Contains only data properties and UI state management - business logic handled in code-behind.
    /// </summary>
    public partial class ProfilePageViewModel : ViewModelBase
    {
        private BitmapImage? imageSource;
        private string username = string.Empty;
        private string description = string.Empty;
        private List<Stock> userStocks = [];
        private Stock? selectedStock;
        private bool isAdmin = false;
        private bool isHidden = false;
        private bool isLoading = false;
        private string errorMessage = string.Empty;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfilePageViewModel"/> class.
        /// </summary>
        public ProfilePageViewModel()
        {
        }

        /// <summary>
        /// Gets or sets the profile image source.
        /// </summary>
        public BitmapImage? ImageSource
        {
            get => this.imageSource;
            set => this.SetProperty(ref this.imageSource, value);
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string UserName
        {
            get => this.username;
            set => this.SetProperty(ref this.username, value);
        }

        /// <summary>
        /// Gets or sets the user description.
        /// </summary>
        public string Description
        {
            get => this.description;
            set => this.SetProperty(ref this.description, value);
        }

        /// <summary>
        /// Gets or sets the user's stock portfolio.
        /// </summary>
        public List<Stock> UserStocks
        {
            get => this.userStocks;
            set => this.SetProperty(ref this.userStocks, value);
        }

        /// <summary>
        /// Gets or sets the currently selected stock.
        /// </summary>
        public Stock? SelectedStock
        {
            get => this.selectedStock;
            set => this.SetProperty(ref this.selectedStock, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user has admin privileges.
        /// </summary>
        public bool IsAdmin
        {
            get => this.isAdmin;
            set => this.SetProperty(ref this.isAdmin, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the profile is hidden.
        /// </summary>
        public bool IsHidden
        {
            get => this.isHidden;
            set => this.SetProperty(ref this.isHidden, value);
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
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

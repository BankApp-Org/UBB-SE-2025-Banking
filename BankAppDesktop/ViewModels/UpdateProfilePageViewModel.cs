namespace BankAppDesktop.ViewModels
{
    using Common.Models.Trading;
    using Microsoft.UI.Xaml.Media.Imaging;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// ViewModel for updating user profile details including username, image, description, and visibility.
    /// Contains only data properties and UI state management - business logic handled in code-behind.
    /// </summary>
    public class UpdateProfilePageViewModel : INotifyPropertyChanged
    {
        private string username = string.Empty;
        private string image = string.Empty;
        private string description = string.Empty;
        private bool isHidden = false;
        private bool isAdmin = false;
        private List<Stock> userStocks = [];
        private BitmapImage? imageSource;
        private bool isLoading = false;
        private string errorMessage = string.Empty;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateProfilePageViewModel"/> class.
        /// </summary>
        public UpdateProfilePageViewModel()
        {
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string Username
        {
            get => this.username;
            set => this.SetProperty(ref this.username, value);
        }

        /// <summary>
        /// Gets or sets the profile image URL.
        /// </summary>
        public string Image
        {
            get => this.image;
            set => this.SetProperty(ref this.image, value);
        }

        /// <summary>
        /// Gets or sets the profile image source for UI binding.
        /// </summary>
        public BitmapImage? ImageSource
        {
            get => this.imageSource;
            set => this.SetProperty(ref this.imageSource, value);
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
        /// Gets or sets a value indicating whether the profile is hidden.
        /// </summary>
        public bool IsHidden
        {
            get => this.isHidden;
            set => this.SetProperty(ref this.isHidden, value);
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
        /// Gets or sets the user's stock portfolio.
        /// </summary>
        public List<Stock> UserStocks
        {
            get => this.userStocks;
            set => this.SetProperty(ref this.userStocks, value);
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

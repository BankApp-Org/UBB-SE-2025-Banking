﻿namespace BankAppDesktop.ViewModels
{
    using Common.Models;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// ViewModel for the User Profile Component, containing only data properties.
    /// </summary>
    public partial class UserProfileComponentViewModel : INotifyPropertyChanged
    {
        private User? user;
        private bool isLoading;
        private string errorMessage = string.Empty;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileComponentViewModel"/> class.
        /// </summary>
        public UserProfileComponentViewModel()
        {
        }

        /// <summary>
        /// Gets or sets the user associated with the profile.
        /// </summary>
        public User? User
        {
            get => this.user;
            set => this.SetProperty(ref this.user, value);
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

namespace StockApp.ViewModels
{
    using Common.Models;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// ViewModel for managing user activities, containing only data properties.
    /// </summary>
    public partial class ActivityViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ActivityLog> activities = [];
        private string userCnp = string.Empty;
        private bool isLoading;
        private string errorMessage = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the collection of activities.
        /// </summary>
        public ObservableCollection<ActivityLog> Activities
        {
            get => activities;
            set => SetProperty(ref activities, value);
        }

        /// <summary>
        /// Gets or sets the user's CNP identifier.
        /// </summary>
        public string UserCnp
        {
            get => userCnp;
            set => SetProperty(ref userCnp, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether activities are being loaded.
        /// </summary>
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
        }

        /// <summary>
        /// Gets or sets the current error message.
        /// </summary>
        public string ErrorMessage
        {
            get => errorMessage;
            set => SetProperty(ref errorMessage, value);
        }

        public ActivityViewModel()
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

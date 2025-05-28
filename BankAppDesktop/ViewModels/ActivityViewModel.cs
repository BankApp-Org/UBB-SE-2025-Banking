namespace BankAppDesktop.ViewModels
{
    using Common.Models;
    using Common.Models.Bank;
    using Microsoft.AspNetCore.Mvc.Rendering;
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
        private bool isAdmin;
        private ObservableCollection<SelectListItem> userList = [];
        private string selectedUserCnp = string.Empty;
        private User? currentUser;

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

        /// <summary>
        /// Gets or sets a value indicating whether the current user is an admin.
        /// </summary>
        public bool IsAdmin
        {
            get => isAdmin;
            set => SetProperty(ref isAdmin, value);
        }

        /// <summary>
        /// Gets or sets the list of users for selection.
        /// </summary>
        public ObservableCollection<SelectListItem> UserList
        {
            get => userList;
            set => SetProperty(ref userList, value);
        }

        /// <summary>
        /// Gets or sets the currently selected user's CNP.
        /// </summary>
        public string SelectedUserCnp
        {
            get => selectedUserCnp;
            set => SetProperty(ref selectedUserCnp, value);
        }

        /// <summary>
        /// Gets or sets the current user.
        /// </summary>
        public User? CurrentUser
        {
            get => currentUser;
            set => SetProperty(ref currentUser, value);
        }

        /// <summary>
        /// Gets a value indicating whether there is an error.
        /// </summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// Gets a value indicating whether the activities list is empty.
        /// </summary>
        public bool IsEmptyState => !IsLoading && Activities.Count == 0;

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

            // Trigger notifications for computed properties
            if (propertyName == nameof(ErrorMessage))
            {
                this.OnPropertyChanged(nameof(HasError));
            }
            if (propertyName == nameof(Activities) || propertyName == nameof(IsLoading))
            {
                this.OnPropertyChanged(nameof(IsEmptyState));
            }

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

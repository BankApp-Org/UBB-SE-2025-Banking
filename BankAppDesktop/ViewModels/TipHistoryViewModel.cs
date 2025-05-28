using Common.Models;
using Common.Models.Social;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace BankAppDesktop.ViewModels
{
    /// <summary>
    /// ViewModel for tip history display, containing only data properties.
    /// </summary>
    public class TipHistoryViewModel : ViewModelBase
    {
        private User? selectedUser;
        private ObservableCollection<Message> messageHistory = [];
        private ObservableCollection<Tip> tipHistory = []; private bool isLoading;

        /// <summary>
        /// Gets or sets the collection of message history.
        /// </summary>
        public ObservableCollection<Message> MessageHistory
        {
            get => this.messageHistory;
            set => this.SetProperty(ref this.messageHistory, value);
        }

        /// <summary>
        /// Gets or sets the collection of tip history.
        /// </summary>
        public ObservableCollection<Tip> TipHistory
        {
            get => this.tipHistory;
            set => this.SetProperty(ref this.tipHistory, value);
        }

        /// <summary>
        /// Gets or sets the currently selected user.
        /// </summary>
        public User? SelectedUser
        {
            get => this.selectedUser;
            set => this.SetProperty(ref this.selectedUser, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether data is being loaded.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetProperty(ref this.isLoading, value);
        }

        public TipHistoryViewModel()
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
    }
}

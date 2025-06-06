namespace BankAppDesktop.ViewModels
{
    using Common.Models.Social;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// ViewModel for chat reports display, containing only data properties.
    /// Business logic for report management is handled in code-behind.
    /// </summary>
    public partial class ChatReportsViewModel : INotifyPropertyChanged
    {
        private bool isLoading;
        private string errorMessage = string.Empty;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatReportsViewModel"/> class.
        /// </summary>
        public ChatReportsViewModel()
        {
            ChatReports = [];
        }

        /// <summary>
        /// Gets or sets the collection of chat reports.
        /// </summary>
        public ObservableCollection<ChatReport> ChatReports { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether reports are currently being loaded.
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

namespace StockApp.ViewModels
{
    using System.ComponentModel;
    using Common.Models;

    /// <summary>
    /// ViewModel for the User Profile Component.
    /// </summary>
    public partial class UserProfileComponentViewModel : INotifyPropertyChanged
    {
        private User? user;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the user associated with the profile.
        /// </summary>
        public User? User
        {
            get => this.user;
            set
            {
                this.user = value;
                this.OnPropertyChanged(nameof(this.User));
            }
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        public void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

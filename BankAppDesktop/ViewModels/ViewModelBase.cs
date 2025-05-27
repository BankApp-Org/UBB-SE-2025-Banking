namespace StockApp.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Base class for ViewModels, providing property change notification and helper methods to simplify property setters.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property that changed. Automatically supplied by the compiler if omitted.
        /// </param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Determines whether a property value has changed, sets the backing field, and raises <see cref="OnPropertyChanged"/> if it has.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">Reference to the field storing the property's value.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="propertyName">
        /// The name of the property. Automatically supplied by the compiler if omitted.
        /// </param>
        /// <returns>
        /// <c>true</c> if the value was different and the property was updated; otherwise, <c>false</c>.
        /// </returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            // Inline: compare existing value to new value and update if different
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}

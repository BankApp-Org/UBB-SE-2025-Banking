using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StockApp.ViewModels
{
    using Common.Models;

    /// <summary>
    /// ViewModel for creating stocks, containing only data properties.
    /// </summary>
    public partial class CreateStockViewModel : INotifyPropertyChanged
    {
        private string stockName = string.Empty;
        private string stockSymbol = string.Empty;
        private int price = 0;
        private int quantity = 0;
        private string authorCnp = string.Empty;
        private string message = string.Empty;
        private bool isAdmin;
        private bool isInputValid;
        private string priceText = string.Empty;
        private string quantityText = string.Empty;
        private bool isLoading;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        public CreateStockViewModel()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current user is an administrator.
        /// </summary>
        public bool IsAdmin
        {
            get => this.isAdmin;
            set => this.SetProperty(ref this.isAdmin, value);
        }

        /// <summary>
        /// Gets or sets the name of the stock to create.
        /// </summary>
        public string StockName
        {
            get => this.stockName;
            set => this.SetProperty(ref this.stockName, value);
        }

        /// <summary>
        /// Gets or sets the symbol of the stock to create.
        /// </summary>
        public string StockSymbol
        {
            get => this.stockSymbol;
            set => this.SetProperty(ref this.stockSymbol, value);
        }

        /// <summary>
        /// Gets or sets the price of the stock.
        /// </summary>
        public int Price
        {
            get => this.price;
            set => this.SetProperty(ref this.price, value);
        }

        /// <summary>
        /// Gets or sets the quantity of the stock.
        /// </summary>
        public int Quantity
        {
            get => this.quantity;
            set => this.SetProperty(ref this.quantity, value);
        }

        /// <summary>
        /// Gets or sets the price as text for input binding.
        /// </summary>
        public string PriceText
        {
            get => this.priceText;
            set => this.SetProperty(ref this.priceText, value);
        }

        /// <summary>
        /// Gets or sets the quantity as text for input binding.
        /// </summary>
        public string QuantityText
        {
            get => this.quantityText;
            set => this.SetProperty(ref this.quantityText, value);
        }

        /// <summary>
        /// Gets or sets the CNP identifier of the author.
        /// </summary>
        public string AuthorCnp
        {
            get => this.authorCnp;
            set => this.SetProperty(ref this.authorCnp, value);
        }

        /// <summary>
        /// Gets or sets the validation or service message to display.
        /// </summary>
        public string Message
        {
            get => this.message;
            set => this.SetProperty(ref this.message, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the input is valid.
        /// </summary>
        public bool IsInputValid
        {
            get => this.isInputValid;
            set => this.SetProperty(ref this.isInputValid, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether an operation is in progress.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetProperty(ref this.isLoading, value);
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

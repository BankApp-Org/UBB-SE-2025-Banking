using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("StockApp.ViewModels.Tests")]

namespace StockApp.ViewModels
{
    using Common.Models;
    using Common.Services;
    using StockApp.Commands;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Windows.Input;

    public partial class CreateStockViewModel : INotifyPropertyChanged
    {
        private readonly IStockService stockService;
        private readonly IUserService userService;
        private readonly IAuthenticationService authenticationService;
        private string stockName = null!;
        private string stockSymbol = null!;
        private int price = 0;
        private int quantity = 0;
        private string authorCnp = null!;
        private string message = null!;
        private readonly bool suppressValidation;
        private bool isAdmin;
        private bool isInputValid;
        private string priceText = string.Empty;
        private string quantityText = string.Empty;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the command to create a new stock entry.
        /// </summary>
        public ICommand CreateStockCommand { get; }

        public CreateStockViewModel(IStockService stockService, IUserService userService,
            IAuthenticationService authenticationService)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.CreateStockCommand = new RelayCommand(this.CreateStock, this.CanCreateStock);
            this.suppressValidation = true;
            this.StockName = string.Empty;
            this.StockSymbol = string.Empty;
            this.price = 0;
            this.quantity = 0;
            this.AuthorCnp = string.Empty;
            this.Message = string.Empty;
            this.suppressValidation = false;
            this.IsAdmin = this.CheckIfUserIsAdmin();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current user is an administrator.
        /// </summary>
        public bool IsAdmin
        {
            get => this.isAdmin;
            set
            {
                if (this.isAdmin != value)
                {
                    this.isAdmin = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the stock to create.
        /// </summary>
        public string StockName
        {
            get => this.stockName;
            set
            {
                if (this.stockName != value)
                {
                    this.stockName = value;
                    this.ValidateInputs();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the symbol of the stock to create.
        /// </summary>
        public string StockSymbol
        {
            get => this.stockSymbol;
            set
            {
                if (this.stockSymbol != value)
                {
                    this.stockSymbol = value;
                    this.ValidateInputs();
                    this.OnPropertyChanged();
                }
            }
        }

        public int Price
        {
            get => this.price;
            set
            {
                if(this.price != value)
                {
                    this.price = value;
                    this.ValidateInputs();
                    this.OnPropertyChanged();
                }
            }
        }

        public int Quantity
        {
            get => this.quantity;
            set
            {
                if (this.quantity != value)
                {
                    this.quantity = value;
                    this.ValidateInputs();
                    this.OnPropertyChanged();
                }
            }
        }

        public string PriceText
        {
            get => this.priceText;
            set
            {
                this.priceText = value;
                this.OnPropertyChanged();

                if (int.TryParse(value, out int parsed))
                {
                    this.Price = parsed;
                }
                this.ValidateInputs();
            }
        }

        public string QuantityText
        {
            get => this.quantityText;
            set
            {
                this.quantityText = value;
                this.OnPropertyChanged();

                if (int.TryParse(value, out int parsed))
                {
                    this.Quantity = parsed;
                }
                this.ValidateInputs();
            }
        }

        /// <summary>
        /// Gets or sets the CNP identifier of the author.
        /// </summary>
        public string AuthorCnp
        {
            get => this.authorCnp;
            set
            {
                if (this.authorCnp != value)
                {
                    this.authorCnp = value;
                    this.ValidateInputs();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the validation or stockService message to display.
        /// </summary>
        public string Message
        {
            get => this.message;
            set
            {
                if (this.message != value)
                {
                    this.message = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current inputs are valid for creating a stock.
        /// </summary>
        public bool IsInputValid
        {
            get => this.isInputValid;
            private set
            {
                if (this.isInputValid != value)
                {
                    this.isInputValid = value;
                    this.OnPropertyChanged();

                    // Notify the command that it can execute or not
                    RelayCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Validates all input fields and sets <see cref="IsInputValid"/> and <see cref="Message"/> accordingly.
        /// </summary>
        private void ValidateInputs()
        {
            if (this.suppressValidation)
            {
                return;
            }

            // Validate stock name presence and format
            if (string.IsNullOrWhiteSpace(this.StockName))
            {
                this.Message = "Stock Name is required!";
                this.IsInputValid = false;
                return;
            }

            if (!StockNameRegex.IsMatch(this.StockName))
            {
                // Only letters and spaces, up to 20 characters
                this.Message = "Stock Name must be max 20 characters and contain only letters & spaces!";
                this.IsInputValid = false;
                return;
            }

            // Validate stock symbol presence and format
            if (string.IsNullOrWhiteSpace(this.StockSymbol))
            {
                this.Message = "Stock Symbol is required!";
                this.IsInputValid = false;
                return;
            }

            if (!StockSymbolRegex.IsMatch(this.StockSymbol))
            {
                // Alphanumeric only, up to 5 characters
                this.Message = "Stock Symbol must be alphanumeric and max 5 characters!";
                this.IsInputValid = false;
                return;
            }

            // Validate stock price
            if (!int.TryParse(this.priceText, out int parsedPrice))
            {
                this.Message = "Price must be a valid number!";
                this.IsInputValid = false;
                return;
            }
            if (parsedPrice < 0)
            {
                this.Message = "Price cannot be negative!";
                this.IsInputValid = false;
                return;
            }

            // Validate Stock quantity
            if (!int.TryParse(this.quantityText, out int parsedQuantity))
            {
                this.Message = "Quantity must be a valid number!";
                this.IsInputValid = false;
                return;
            }
            if (parsedQuantity < 0)
            {
                this.Message = "Quantity cannot be negative!";
                this.IsInputValid = false;
                return;
            }

            // Validate CNP presence and format
            if (string.IsNullOrWhiteSpace(this.AuthorCnp))
            {
                this.Message = "Author CNP is required!";
                this.IsInputValid = false;
                return;
            }

            if (!CNPRegex.IsMatch(this.AuthorCnp))
            {
                // Exactly 13 digits required
                this.Message = "Author CNP must be exactly 13 digits!";
                this.IsInputValid = false;
                return;
            }

            this.Message = string.Empty;
            this.IsInputValid = true;
        }

        private bool CanCreateStock(object? obj) => this.isAdmin && this.IsInputValid;

        private async void CreateStock(object obj)
        {
            if (!this.CanCreateStock(null))
            {
                return;
            }

            var existingStock = await this.stockService.GetStockByNameAsync(this.StockName);
            if (existingStock != null)
            {
                this.Message = $"A stock named '{this.StockName}' already exists!";
                return;
            }

            await this.stockService.CreateStockAsync(new Stock()
            {
                Name = this.StockName,
                Symbol = this.StockSymbol,
                AuthorCNP = this.AuthorCnp,
                NewsArticles = [],
                Price = this.Price,
                Quantity = this.Quantity,
            });

            this.Message = $"Stock '{this.StockName}' was successfully created.";
        }

        protected bool CheckIfUserIsAdmin()
        {
            return this.authenticationService.IsUserLoggedIn() &&
                this.authenticationService.IsUserAdmin();
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed. Auto-supplied if omitted.</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [GeneratedRegex(@"^[A-Za-z ]{1,20}$")]
        private static partial Regex StockNameRegex { get; }

        [GeneratedRegex(@"^[A-Za-z0-9]{1,5}$")]
        private static partial Regex StockSymbolRegex { get; }

        [GeneratedRegex(@"^\d{13}$")]
        private static partial Regex CNPRegex { get; }
    }
}

namespace BankAppDesktop.Pages
{
    using BankAppDesktop.ViewModels;
    using Common.Models.Trading;
    using Common.Services;
    using Common.Services.Stock;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateStockPage : Page
    {
        private readonly CreateStockViewModel _viewModel;
        private readonly IStockService _stockService;
        private readonly IAuthenticationService _authService;

        public CreateStockPage(CreateStockViewModel viewModel, IStockService stockService, IAuthenticationService authService)
        {
            this.InitializeComponent();
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            this.DataContext = _viewModel;

            // Initialize admin status
            RefreshAdminStatus();
        }

        private void RefreshAdminStatus()
        {
            _viewModel.IsAdmin = _authService.IsUserAdmin();
        }

        private async void CreateStockButton_Click(object sender, RoutedEventArgs e)
        {
            await CreateStockAsync();
        }

        private async Task CreateStockAsync()
        {
            try
            {
                // Clear previous messages
                _viewModel.Message = string.Empty;

                // Validate admin permissions
                if (!_authService.IsUserLoggedIn())
                {
                    _viewModel.Message = "You must be logged in to create stocks.";
                    return;
                }

                if (!_authService.IsUserAdmin())
                {
                    _viewModel.Message = "Only administrators can create stocks.";
                    return;
                }

                // Validate input data
                if (!ValidateStockData())
                {
                    return;
                }

                // Parse numeric values
                if (!int.TryParse(_viewModel.PriceText, out int price))
                {
                    _viewModel.Message = "Invalid price format. Please enter a valid number.";
                    return;
                }

                if (!int.TryParse(_viewModel.QuantityText, out int quantity))
                {
                    _viewModel.Message = "Invalid quantity format. Please enter a valid number.";
                    return;
                }

                // Validate ranges
                if (price <= 0)
                {
                    _viewModel.Message = "Price must be greater than 0.";
                    return;
                }

                if (quantity <= 0)
                {
                    _viewModel.Message = "Quantity must be greater than 0.";
                    return;
                }

                // Get current user CNP
                string userCnp = _authService.GetUserCNP();

                // Create the stock object
                var stock = new Stock
                {
                    Name = _viewModel.StockName,
                    Symbol = _viewModel.StockSymbol,
                    Price = price,
                    Quantity = quantity,
                    AuthorCNP = userCnp
                };

                // Call the service to create the stock
                var createdStock = await _stockService.CreateStockAsync(stock);

                if (createdStock != null)
                {
                    _viewModel.Message = $"Stock '{createdStock.Name}' created successfully!";

                    // Clear the form
                    ClearForm();
                }
                else
                {
                    _viewModel.Message = "Failed to create stock. Please try again.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                _viewModel.Message = "You are not authorized to create stocks.";
            }
            catch (Exception ex)
            {
                _viewModel.Message = $"Error creating stock: {ex.Message}";
            }
        }

        private bool ValidateStockData()
        {
            if (string.IsNullOrWhiteSpace(_viewModel.StockName))
            {
                _viewModel.Message = "Stock name is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(_viewModel.StockSymbol))
            {
                _viewModel.Message = "Stock symbol is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(_viewModel.PriceText))
            {
                _viewModel.Message = "Price is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(_viewModel.QuantityText))
            {
                _viewModel.Message = "Quantity is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(_viewModel.AuthorCnp))
            {
                _viewModel.Message = "Author CNP is required.";
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            _viewModel.StockName = string.Empty;
            _viewModel.StockSymbol = string.Empty;
            _viewModel.PriceText = string.Empty;
            _viewModel.QuantityText = string.Empty;
            _viewModel.AuthorCnp = string.Empty;
        }
    }
}

namespace BankAppDesktop.Views.Pages
{
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using BankAppDesktop.ViewModels;
    using System;
    using System.Threading.Tasks; // Added for Task
    using Common.Services.Bank;

    public sealed partial class InvestmentsPage : Page
    {
        private readonly InvestmentsViewModel _viewModel;
        private readonly IInvestmentsService _investmentsService; // Added

        public InvestmentsPage(InvestmentsViewModel viewModel, IInvestmentsService investmentsService) // Modified constructor
        {
            InitializeComponent();
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _investmentsService = investmentsService ?? throw new ArgumentNullException(nameof(investmentsService)); // Added
            DataContext = _viewModel;
            Loaded += async (s, e) => await LoadInvestmentPortfolioAsync(); // Load data on page load
        }

        private async void UpdateCreditScoreCommand(object sender, RoutedEventArgs e)
        {
            _viewModel.IsLoading = true;
            _viewModel.ErrorMessage = string.Empty;
            try
            {
                await _investmentsService.CreditScoreUpdateInvestmentsBasedAsync(); // Changed
                await LoadInvestmentPortfolioAsync();
                ShowSuccessDialog("Credit score based investment adjustments completed.");
            }
            catch (Exception ex)
            {
                _viewModel.ErrorMessage = $"Error updating credit score related investments: {ex.Message}";
                ShowErrorDialog(_viewModel.ErrorMessage);
            }
            finally
            {
                _viewModel.IsLoading = false;
            }
        }

        private async void CalculateROICommand(object sender, RoutedEventArgs e)
        {
            _viewModel.IsLoading = true;
            _viewModel.ErrorMessage = string.Empty;
            try
            {
                await _investmentsService.CalculateAndUpdateROIAsync(); // Changed
                // Optionally, refresh portfolio data
                // await LoadInvestmentPortfolioAsync();
                ShowSuccessDialog("ROI calculation and update completed.");
            }
            catch (Exception ex)
            {
                _viewModel.ErrorMessage = $"Error calculating ROI: {ex.Message}";
                ShowErrorDialog(_viewModel.ErrorMessage);
            }
            finally
            {
                _viewModel.IsLoading = false;
            }
        }

        private async void CalculateRiskScoreCommand(object sender, RoutedEventArgs e)
        {
            _viewModel.IsLoading = true;
            _viewModel.ErrorMessage = string.Empty;
            try
            {
                await _investmentsService.CalculateAndUpdateRiskScoreAsync(); // Changed
                await LoadInvestmentPortfolioAsync();
                ShowSuccessDialog("Risk score calculation and update completed.");
            }
            catch (Exception ex)
            {
                _viewModel.ErrorMessage = $"Error calculating risk score: {ex.Message}";
                ShowErrorDialog(_viewModel.ErrorMessage);
            }
            finally
            {
                _viewModel.IsLoading = false;
            }
        }

        // Renamed from LoadInvestmentPortfolio and made private Task
        private async Task LoadInvestmentPortfolioAsync()
        {
            _viewModel.IsLoading = true;
            _viewModel.ErrorMessage = string.Empty;
            try
            {
                var portfolioItems = await _investmentsService.GetPortfolioSummaryAsync(); // Changed
                _viewModel.UsersPortofolio.Clear();
                if (portfolioItems != null)
                {
                    foreach (var item in portfolioItems)
                    {
                        _viewModel.UsersPortofolio.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                _viewModel.ErrorMessage = $"Error loading investment portfolio: {ex.Message}";
                ShowErrorDialog(_viewModel.ErrorMessage);
            }
            finally
            {
                _viewModel.IsLoading = false;
            }
        }

        // Helper methods for dialogs (can be moved to a base page or utility class)
        private async void ShowErrorDialog(string message)
        {
            ContentDialog errorDialog = new()
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await errorDialog.ShowAsync();
        }

        private async void ShowSuccessDialog(string message)
        {
            ContentDialog successDialog = new()
            {
                Title = "Success",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await successDialog.ShowAsync();
        }
    }
}

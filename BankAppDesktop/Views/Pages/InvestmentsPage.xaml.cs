namespace StockApp.Views.Pages
{
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.ViewModels;
    using System;

    public sealed partial class InvestmentsPage : Page
    {
        private readonly InvestmentsViewModel _viewModel;

        public InvestmentsPage(InvestmentsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = _viewModel;
        }

        private async void UpdateCreditScoreCommand(object sender, RoutedEventArgs e)
        {
            try
            {
                await _viewModel.CreditScoreUpdateInvestmentsBasedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating credit score: {ex.Message}");
            }
        }

        private async void CalculateROICommand(object sender, RoutedEventArgs e)
        {
            try
            {
                await _viewModel.CalculateAndUpdateROIAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating ROI: {ex.Message}");
            }
        }

        private async void CalculateRiskScoreCommand(object sender, RoutedEventArgs e)
        {
            try
            {
                await _viewModel.CalculateAndUpdateRiskScoreAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating risk score: {ex.Message}");
            }
        }

        private async void LoadInvestmentPortfolio(object sender, RoutedEventArgs e)
        {
            try
            {
                await _viewModel.LoadPortfolioSummaryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading investment portfolio: {ex.Message}");
            }
        }
    }
}

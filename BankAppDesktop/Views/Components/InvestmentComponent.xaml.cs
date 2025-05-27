namespace StockApp.Views.Components
{
    using Common.Models;
    using Microsoft.UI.Xaml.Controls;

    public sealed partial class InvestmentComponent : Page
    {
        public string FirstName { get; set; } = string.Empty;

        public string SecondName { get; set; } = string.Empty;

        public decimal TotalInvested { get; set; }

        public decimal TotalReturns { get; set; }

        public decimal AverageROI { get; set; }

        public int NumberOfInvestments { get; set; }

        public int RiskFactor { get; set; }

        public InvestmentComponent()
        {
            this.InitializeComponent();
        }

        public void SetPortfolioSummary(InvestmentPortfolio userPortfolio)
        {
            this.FirstName = userPortfolio.FirstName;
            this.SecondName = userPortfolio.SecondName;
            this.TotalInvested = userPortfolio.TotalAmountInvested;
            this.TotalReturns = userPortfolio.TotalAmountReturned;
            this.AverageROI = userPortfolio.AverageROI;
            this.NumberOfInvestments = userPortfolio.NumberOfInvestments;
            this.RiskFactor = userPortfolio.RiskFactor;

            this.UserFirstNameTextBlock.Text = $"First Name: {this.FirstName}";
            this.UserSecondNameTextBlock.Text = $"Second Name: {this.SecondName}";
            this.TotalInvestedTextBlock.Text = $"Total Invested: {this.TotalInvested}";
            this.TotalReturnsTextBlock.Text = $"Total Returns: {this.TotalReturns}";
            this.AverageROITextBlock.Text = $"Average ROI: {this.AverageROI}";
            this.NumberOfInvestmentsTextBlock.Text = $"Number of Investments: {this.NumberOfInvestments}";
            this.RiskFactorTextBlock.Text = $"Risk Score: {this.RiskFactor}";
        }
    }
}

namespace Common.Models
{
    public class InvestmentPortfolio(string firstName, string secondName, decimal totalAmountInvested, decimal totalAmountReturned, decimal averageROI, int numberOfInvestments, int riskFactor)
    {
        public string FirstName { get; set; } = firstName;

        public string SecondName { get; set; } = secondName;

        public decimal TotalAmountInvested { get; set; } = totalAmountInvested;

        public decimal TotalAmountReturned { get; set; } = totalAmountReturned;

        public decimal AverageROI { get; set; } = averageROI;

        public int NumberOfInvestments { get; set; } = numberOfInvestments;

        public int RiskFactor { get; set; } = riskFactor;
    }
}

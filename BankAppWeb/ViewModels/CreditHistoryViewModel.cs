using Common.Models;

namespace BankAppWeb.ViewModels
{
    public class CreditHistoryViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public int CurrentCreditScore { get; set; }
        public List<CreditScoreHistory> WeeklyHistory { get; set; } = new();
        public List<CreditScoreHistory> MonthlyHistory { get; set; } = new();
        public List<CreditScoreHistory> YearlyHistory { get; set; } = new();

        public string GetCreditScoreClass()
        {
            return CurrentCreditScore switch
            {
                >= 750 => "excellent",
                >= 700 => "very-good",
                >= 650 => "good",
                >= 600 => "fair",
                _ => "poor"
            };
        }

        public string GetCreditScoreDescription()
        {
            return CurrentCreditScore switch
            {
                >= 750 => "Excellent Credit",
                >= 700 => "Very Good Credit",
                >= 650 => "Good Credit",
                >= 600 => "Fair Credit",
                _ => "Poor Credit"
            };
        }
    }
}
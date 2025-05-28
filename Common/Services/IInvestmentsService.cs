namespace Common.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Models.Bank;

    public interface IInvestmentsService
    {
        Task CalculateAndUpdateRiskScoreAsync();

        Task CalculateAndUpdateROIAsync();

        Task CreditScoreUpdateInvestmentsBasedAsync();

        Task<List<InvestmentPortfolio>> GetPortfolioSummaryAsync();

        Task<List<Investment>> GetInvestmentsHistoryAsync();

        Task AddInvestmentAsync(Investment investment);

        Task UpdateInvestmentAsync(int investmentId, string investorCNP, decimal amountReturned);
    }
}

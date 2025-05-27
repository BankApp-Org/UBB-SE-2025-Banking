using Common.Models;

namespace BankApi.Repositories
{
    public interface IInvestmentsRepository
    {
        Task<List<Investment>> GetInvestmentsHistory();
        Task AddInvestment(Investment investment);
        Task UpdateInvestment(int investmentId, string investorCNP, decimal amountReturned);
    }
}
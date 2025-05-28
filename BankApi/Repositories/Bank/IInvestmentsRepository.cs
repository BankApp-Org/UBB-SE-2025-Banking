using Common.Models.Bank;

namespace BankApi.Repositories.Bank
{
    public interface IInvestmentsRepository
    {
        Task<List<Investment>> GetInvestmentsHistory();
        Task AddInvestment(Investment investment);
        Task UpdateInvestment(int investmentId, string investorCNP, decimal amountReturned);
    }
}
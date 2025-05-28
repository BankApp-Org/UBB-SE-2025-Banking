using Common.Models.Bank;

namespace BankApi.Repositories.Bank
{
    public interface ILoanRequestRepository
    {
        Task<List<LoanRequest>> GetLoanRequestsAsync();

        Task<List<LoanRequest>> GetUnsolvedLoanRequestsAsync();

        Task SolveLoanRequestAsync(int loanRequestId);

        Task DeleteLoanRequestAsync(int loanRequestId);

        Task<LoanRequest> CreateLoanRequestAsync(LoanRequest loanRequest);
    }
}

namespace Common.Services
{
    using Common.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ILoanRequestService
    {
        Task<string> GiveSuggestion(LoanRequest loanRequest);

        Task SolveLoanRequest(int loanRequestId);

        Task DeleteLoanRequest(int loanRequestId);

        Task<List<LoanRequest>> GetLoanRequests();

        Task<List<LoanRequest>> GetUnsolvedLoanRequests();

        Task<LoanRequest> CreateLoanRequest(LoanRequest loanRequest);
    }
}
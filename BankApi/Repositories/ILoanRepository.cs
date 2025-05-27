namespace BankApi.Repositories
{
    using System.Collections.Generic;
    using Common.Models;

    public interface ILoanRepository
    {
        Task<List<Loan>> GetLoansAsync();

        Task<List<Loan>> GetUserLoansAsync(string userCNP);

        Task AddLoanAsync(Loan loan);

        Task UpdateLoanAsync(Loan loan);

        Task DeleteLoanAsync(int loanID);

        Task<Loan> GetLoanByIdAsync(int loanID);

        Task UpdateCreditScoreHistoryForUserAsync(string userCNP, int newScore);
    }
}

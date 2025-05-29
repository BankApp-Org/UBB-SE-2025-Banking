namespace Common.Services.Bank
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Models;
    using Common.Models.Bank;

    public interface ILoanService
    {
        Task<List<Loan>> GetLoansAsync();

        Task<List<Loan>> GetUserLoansAsync(string userCNP);

        Task AddLoanAsync(LoanRequest loanRequest);

        Task CheckLoansAsync();

        static int ComputeNewCreditScore(User user, Loan loan)
        {
            int totalDaysInAdvance = (loan.RepaymentDate - DateTime.Today).Days;
            if (totalDaysInAdvance > 30)
            {
                totalDaysInAdvance = 30;
            }
            else if (totalDaysInAdvance < -100)
            {
                totalDaysInAdvance = -100;
            }

            int newUserCreditScore = user.CreditScore + (int)loan.LoanAmount * 10 / user.Income + totalDaysInAdvance;
            newUserCreditScore = Math.Min(newUserCreditScore, 700);
            newUserCreditScore = Math.Max(newUserCreditScore, 100);

            return newUserCreditScore;
        }

        Task UpdateHistoryForUserAsync(string userCNP, int newScore);

        Task IncrementMonthlyPaymentsCompletedAsync(int loanID, decimal penalty);
    }
}

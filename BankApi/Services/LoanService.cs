namespace BankApi.Services
{
    using BankApi.Repositories;
    using Common.Models;
    using Common.Services;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class LoanService(ILoanRepository loanRepository, IUserRepository userRepository) : ILoanService
    {
        private readonly ILoanRepository loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        private readonly IUserRepository userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

        public async Task<List<Loan>> GetLoansAsync()
        {
            return await loanRepository.GetLoansAsync();
        }

        public async Task<List<Loan>> GetUserLoansAsync(string userCNP)
        {
            return await loanRepository.GetUserLoansAsync(userCNP);
        }

        public async Task AddLoanAsync(LoanRequest loanRequest)
        {
            User user = await userRepository.GetByCnpAsync(loanRequest.UserCnp) ?? throw new Exception("User not found");
            Loan loanToProcess = loanRequest.Loan; // Use the Loan object from the input LoanRequest

            if (loanToProcess == null)
            {
                // This case should ideally not happen if the controller always sets loanRequest.Loan
                throw new ArgumentException("Loan details are missing in the loan request.", nameof(loanRequest));
            }

            // Calculate interest rate
            // Ensure CreditScore is not zero to avoid division by zero error.
            // Assign a high default interest component if CreditScore is 0 or RiskScore is high relative to CreditScore.
            decimal interestRate = (user.CreditScore > 0) ? ((decimal)user.RiskScore / user.CreditScore * 100) : 100m; // Default to 100% if credit score is 0
            interestRate = Math.Max(0, interestRate); // Ensure interest rate is not negative

            // Calculate number of months
            int noMonths = (loanToProcess.RepaymentDate.Year - loanToProcess.ApplicationDate.Year) * 12 + loanToProcess.RepaymentDate.Month - loanToProcess.ApplicationDate.Month;
            if (noMonths <= 0)
            {
                // Default to 1 month if date range is invalid or too short. Business logic might require an error here.
                noMonths = 1;
            }

            // Calculate monthly payment amount
            decimal monthlyPaymentAmount;
            if (noMonths > 0)
            {
                monthlyPaymentAmount = loanToProcess.LoanAmount * (1 + interestRate / 100) / noMonths;
            }
            else
            {
                monthlyPaymentAmount = loanToProcess.LoanAmount; // Fallback, though noMonths is ensured to be > 0 above.
            }
            monthlyPaymentAmount = Math.Max(0, monthlyPaymentAmount); // Ensure non-negative

            // Update the properties of the loanToProcess object (which is loanRequest.Loan)
            loanToProcess.InterestRate = interestRate;
            loanToProcess.NumberOfMonths = noMonths;
            loanToProcess.MonthlyPaymentAmount = monthlyPaymentAmount;

            await loanRepository.AddLoanAsync(loanToProcess);
        }

        public async Task CheckLoansAsync()
        {
            List<Loan> loanList = await loanRepository.GetLoansAsync();
            foreach (Loan loan in loanList)
            {
                int numberOfMonthsPassed = (DateTime.Today.Year - loan.ApplicationDate.Year) * 12 + DateTime.Today.Month - loan.ApplicationDate.Month;
                User user = await userRepository.GetByCnpAsync(loan.UserCnp) ?? throw new Exception("User not found");
                if (loan.MonthlyPaymentsCompleted >= loan.NumberOfMonths)
                {
                    loan.Status = "completed";
                    int newUserCreditScore = ILoanService.ComputeNewCreditScore(user, loan);

                    user.CreditScore = newUserCreditScore;
                    await userRepository.UpdateAsync(user);
                }

                if (numberOfMonthsPassed > loan.MonthlyPaymentsCompleted)
                {
                    int numberOfOverdueDays = (DateTime.Today - loan.ApplicationDate.AddMonths(loan.MonthlyPaymentsCompleted)).Days;
                    decimal penalty = (decimal)(0.1 * numberOfOverdueDays);
                    loan.Penalty = Math.Max(0, penalty); // Ensure penalty is not negative
                }
                else
                {
                    loan.Penalty = 0;
                }

                if (DateTime.Today > loan.RepaymentDate && loan.Status == "active")
                {
                    loan.Status = "overdue";
                    int newUserCreditScore = ILoanService.ComputeNewCreditScore(user, loan);

                    user.CreditScore = newUserCreditScore;
                    await userRepository.UpdateAsync(user);
                    await UpdateHistoryForUserAsync(loan.UserCnp, newUserCreditScore);
                }
                else if (loan.Status == "overdue")
                {
                    if (loan.MonthlyPaymentsCompleted >= loan.NumberOfMonths)
                    {
                        loan.Status = "completed";
                        int newUserCreditScore = ILoanService.ComputeNewCreditScore(user, loan);

                        user.CreditScore = newUserCreditScore;
                        await userRepository.UpdateAsync(user);
                        await UpdateHistoryForUserAsync(loan.UserCnp, newUserCreditScore);
                    }
                }

                if (loan.Status == "completed")
                {
                    await loanRepository.DeleteLoanAsync(loan.Id);
                }
                else
                {
                    await loanRepository.UpdateLoanAsync(loan);
                }
            }
        }

        public async Task UpdateHistoryForUserAsync(string userCNP, int newScore)
        {
            await loanRepository.UpdateCreditScoreHistoryForUserAsync(userCNP, newScore);
        }

        public async Task IncrementMonthlyPaymentsCompletedAsync(int loanID, decimal penalty)
        {
            Loan loan = await loanRepository.GetLoanByIdAsync(loanID);
            loan.MonthlyPaymentsCompleted++;
            loan.RepaidAmount += loan.MonthlyPaymentAmount + penalty; // Assuming penalty is part of the repayment
            if (loan.MonthlyPaymentsCompleted >= loan.NumberOfMonths)
            {
                loan.Status = "completed";
                User user = await userRepository.GetByCnpAsync(loan.UserCnp) ?? throw new Exception("User not found");
                int newUserCreditScore = ILoanService.ComputeNewCreditScore(user, loan);

                user.CreditScore = newUserCreditScore;
                await userRepository.UpdateAsync(user);
                await UpdateHistoryForUserAsync(loan.UserCnp, newUserCreditScore);
            }
            else if (loan.Status == "overdue" && loan.MonthlyPaymentsCompleted < loan.NumberOfMonths)
            {
                loan.Status = "active"; // Reset status to active if overdue but payments are still ongoing
            }
            await loanRepository.UpdateLoanAsync(loan);
        }
    }
}

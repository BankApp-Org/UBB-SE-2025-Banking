namespace BankApi.Services.Bank
{
    using BankApi.Repositories;
    using BankApi.Repositories.Bank;
    using Common.Models;
    using Common.Models.Bank;
    using Common.Services.Bank;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class LoanService(ILoanRepository loanRepository, IUserRepository userRepository, ICreditScoringService creditScoringService, IBankAccountService bankAccountService) : ILoanService
    {
        private readonly ILoanRepository loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        private readonly IUserRepository userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly ICreditScoringService creditScoringService = creditScoringService ?? throw new ArgumentNullException(nameof(creditScoringService));
        private readonly IBankAccountService bankAccountService = bankAccountService ?? throw new ArgumentNullException(nameof(bankAccountService));

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
            decimal interestRate = user.CreditScore > 0 ? (decimal)user.RiskScore / user.CreditScore * 100 : 100m; // Default to 100% if credit score is 0
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

            // Apply credit score impact for taking out a loan
            await ApplyLoanApplicationImpactAsync(user.CNP, loanToProcess);
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

                    // Apply credit score impact for completing loan
                    await ApplyLoanCompletionImpactAsync(user.CNP, loan);
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

                    // Apply negative credit score impact for overdue loan
                    await ApplyLoanOverdueImpactAsync(user.CNP, loan);
                }
                else if (loan.Status == "overdue")
                {
                    if (loan.MonthlyPaymentsCompleted >= loan.NumberOfMonths)
                    {
                        loan.Status = "completed";

                        // Apply credit score impact for completing overdue loan
                        await ApplyLoanCompletionImpactAsync(user.CNP, loan);
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

            bool isOnTime = penalty == 0; // No penalty means on-time payment

            // Apply credit score impact for loan payment
            await ApplyLoanPaymentImpactAsync(loan.UserCnp, loan, loan.MonthlyPaymentAmount + penalty, isOnTime);

            if (loan.MonthlyPaymentsCompleted >= loan.NumberOfMonths)
            {
                
                loan.Status = "completed";

                // Apply additional credit score impact for completing loan
                await ApplyLoanCompletionImpactAsync(loan.UserCnp, loan);
            }
            else if (loan.Status == "overdue" && loan.MonthlyPaymentsCompleted < loan.NumberOfMonths)
            {
                loan.Status = "active"; // Reset status to active if overdue but payments are still ongoing
            }
            await loanRepository.UpdateLoanAsync(loan);
        }

        public async Task PayLoanAsync(int loanId, decimal amount, string userCNP, string iban)
        {
            try
            {
                Loan loan = await loanRepository.GetLoanByIdAsync(loanId);
                if (loan == null)
                {
                    throw new Exception("Loan not found");
                }

                decimal previousRepaidAmount = loan.RepaidAmount;
                loan.RepaidAmount += amount;

                bool isOnTime = loan.Status != "overdue";

                // Apply credit score impact for loan payment
                await ApplyLoanPaymentImpactAsync(loan.UserCnp, loan, amount, isOnTime);

                if (loan.RepaidAmount >= loan.LoanAmount + loan.Penalty)
                {
                    var totalToPay = loan.LoanAmount + loan.Penalty;
                    var user = await userRepository.GetByCnpAsync(loan.UserCnp);

                      if( await bankAccountService.CheckSufficientFunds(user.Id, iban, totalToPay, loan.Currency))
                        {
                            var bankAccount = await bankAccountService.FindBankAccount(iban);
                            bankAccount.Balance -= totalToPay;
                           await  bankAccountService.UpdateBankAccount(bankAccount);
                           loan.Status = "completed";
                           await ApplyLoanCompletionImpactAsync(loan.UserCnp, loan);
                        }
                      else
                    {
                        throw new Exception("Insufficient Funds! Please select another account.");
                    }
                  
                }
                else
                {
                    loan.Status = "active"; // Still active if not fully repaid
                }

                await loanRepository.UpdateLoanAsync(loan);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // New methods for credit score impact
        private async Task ApplyLoanApplicationImpactAsync(string userCnp, Loan loan)
        {
            try
            {
                // Taking out a loan has a small negative impact initially
                int currentScore = await creditScoringService.GetCurrentCreditScoreAsync(userCnp);
                int newScore = currentScore - 5; // Small penalty for new debt

                await creditScoringService.UpdateCreditScoreAsync(userCnp, newScore,
                    $"New loan application: {loan.LoanAmount:C}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying loan application impact: {ex.Message}");
            }
        }

        private async Task ApplyLoanPaymentImpactAsync(string userCnp, Loan loan, decimal paymentAmount, bool isOnTime)
        {
            try
            {
                int impact = await creditScoringService.CalculateLoanPaymentImpactAsync(userCnp, loan, paymentAmount, isOnTime);

                if (impact != 0)
                {
                    int currentScore = await creditScoringService.GetCurrentCreditScoreAsync(userCnp);
                    int newScore = currentScore + impact;

                    string reason = isOnTime
                        ? $"On-time loan payment: {paymentAmount:C}"
                        : $"Late loan payment: {paymentAmount:C}";

                    await creditScoringService.UpdateCreditScoreAsync(userCnp, newScore, reason);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying loan payment impact: {ex.Message}");
                throw;
            }
        }

        private async Task ApplyLoanCompletionImpactAsync(string userCnp, Loan loan)
        {
            try
            {
                // Completing a loan has a positive impact
                int currentScore = await creditScoringService.GetCurrentCreditScoreAsync(userCnp);
                int bonusPoints = loan.Status == "overdue" ? 10 : 20; // Less bonus if it was overdue
                int newScore = currentScore + bonusPoints;

                await creditScoringService.UpdateCreditScoreAsync(userCnp, newScore,
                    $"Loan completion: {loan.LoanAmount:C}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying loan completion impact: {ex.Message}");
                throw;
            }
        }

        private async Task ApplyLoanOverdueImpactAsync(string userCnp, Loan loan)
        {
            try
            {
                // Overdue loans have significant negative impact
                int currentScore = await creditScoringService.GetCurrentCreditScoreAsync(userCnp);
                int penalty = -30; // Significant penalty for overdue
                int newScore = currentScore + penalty;

                await creditScoringService.UpdateCreditScoreAsync(userCnp, newScore,
                    $"Loan overdue: {loan.LoanAmount:C}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying loan overdue impact: {ex.Message}");
            }
        }
    }
}

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

    public class LoanRequestService(ILoanRequestRepository loanRequestRepository, IUserRepository userRepository) : ILoanRequestService
    {
        private readonly ILoanRequestRepository loanRequestRepository = loanRequestRepository;
        private readonly IUserRepository userRepository = userRepository;

        public async Task<string> GiveSuggestion(LoanRequest loanRequest)
        {

            User user = await userRepository.GetByCnpAsync(loanRequest.UserCnp) ?? throw new Exception("User not found");

            string givenSuggestion = string.Empty;

            if (loanRequest.Loan.LoanAmount > user.Income * 10)
            {
                givenSuggestion = "Amount requested is too high for user income";
            }

            if (user.CreditScore < 300)
            {
                if (givenSuggestion.Length > 0)
                {
                    givenSuggestion += ", ";
                }

                givenSuggestion += "Credit score is too low";
            }

            if (user.RiskScore > 70)
            {
                if (givenSuggestion.Length > 0)
                {
                    givenSuggestion += ", ";
                }

                givenSuggestion += "User risk score is too high";
            }

            if (givenSuggestion.Length > 0)
            {
                givenSuggestion = "User does not qualify for loan: " + givenSuggestion;
            }

            return givenSuggestion;
        }

        public async Task SolveLoanRequest(int loanRequestId)
        {
            await loanRequestRepository.SolveLoanRequestAsync(loanRequestId);
        }

        public async Task DeleteLoanRequest(int loanRequestId)
        {
            await loanRequestRepository.DeleteLoanRequestAsync(loanRequestId);
        }

        public async Task<List<LoanRequest>> GetLoanRequests()
        {
            return await loanRequestRepository.GetLoanRequestsAsync();
        }

        public async Task<List<LoanRequest>> GetUnsolvedLoanRequests()
        {
            return await loanRequestRepository.GetUnsolvedLoanRequestsAsync();
        }

        public async Task<LoanRequest> CreateLoanRequest(LoanRequest loanRequest)
        {
            return await loanRequestRepository.CreateLoanRequestAsync(loanRequest);
        }
    }
}
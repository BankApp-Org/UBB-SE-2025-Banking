﻿namespace Common.Services.Bank
{
    using Common.Models.Bank;
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
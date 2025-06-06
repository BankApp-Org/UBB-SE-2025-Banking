using Common.Models;
using Common.Models.Bank;
using Common.Models.Trading;

namespace Common.Services.Bank
{
    public interface ICreditScoringService
    {
        /// <summary>
        /// Calculates credit score impact for bank transactions (transfers, withdrawals, deposits)
        /// </summary>
        Task<int> CalculateBankTransactionImpactAsync(string userCnp, BankTransaction transaction);

        /// <summary>
        /// Calculates credit score impact for loan payments
        /// </summary>
        Task<int> CalculateLoanPaymentImpactAsync(string userCnp, Loan loan, decimal paymentAmount, bool isOnTime);

        /// <summary>
        /// Calculates credit score impact for gem transactions
        /// </summary>
        Task<int> CalculateGemTransactionImpactAsync(string userCnp, bool isBuying, int gemAmount, double transactionValue);

        /// <summary>
        /// Calculates credit score impact for stock transactions
        /// </summary>
        Task<int> CalculateStockTransactionImpactAsync(string userCnp, StockTransaction stockTransaction);

        /// <summary>
        /// Updates user's credit score and records history
        /// </summary>
        Task UpdateCreditScoreAsync(string userCnp, int newScore, string reason);

        /// <summary>
        /// Gets current credit score for a user
        /// </summary>
        Task<int> GetCurrentCreditScoreAsync(string userCnp);

        /// <summary>
        /// Calculates comprehensive credit score based on user's financial behavior
        /// </summary>
        Task<int> CalculateComprehensiveCreditScoreAsync(string userCnp);
    }
}
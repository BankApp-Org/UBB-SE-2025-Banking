using BankApi.Repositories.Trading;
using Common.Models.Trading;
using Common.Services.Trading;
using Common.Services.Bank;

namespace BankApi.Services.Trading
{
    public class TransactionService(IStockTransactionRepository transactionRepository, ICreditScoringService creditScoringService) : ITransactionService
    {
        private readonly IStockTransactionRepository _transactionRepository = transactionRepository;
        private readonly ICreditScoringService _creditScoringService = creditScoringService;

        public async Task AddTransactionAsync(StockTransaction transaction)
        {
            await _transactionRepository.AddTransactionAsync(transaction);

            // Apply credit score impact for stock transaction
            await ApplyStockTransactionImpactAsync(transaction);
        }

        public async Task<List<StockTransaction>> GetAllTransactionsAsync()
        {
            return await _transactionRepository.getAllTransactions();
        }

        public async Task<List<StockTransaction>> GetByFilterCriteriaAsync(StockTransactionFilterCriteria criteria)
        {
            return await _transactionRepository.GetByFilterCriteriaAsync(criteria);
        }

        /// <summary>
        /// Applies credit score impact for stock transactions.
        /// </summary>
        private async Task ApplyStockTransactionImpactAsync(StockTransaction transaction)
        {
            try
            {
                if (string.IsNullOrEmpty(transaction.AuthorCNP))
                    return;

                int impact = await _creditScoringService.CalculateStockTransactionImpactAsync(transaction.AuthorCNP, transaction);

                if (impact != 0)
                {
                    int currentScore = await _creditScoringService.GetCurrentCreditScoreAsync(transaction.AuthorCNP);
                    int newScore = currentScore + impact;

                    string reason = $"Stock {transaction.Type}: {transaction.StockSymbol} - {Math.Abs(transaction.TotalValue):C}";

                    await _creditScoringService.UpdateCreditScoreAsync(transaction.AuthorCNP, newScore, reason);
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the transaction
                Console.WriteLine($"Error applying stock transaction credit impact: {ex.Message}");
            }
        }
    }
}

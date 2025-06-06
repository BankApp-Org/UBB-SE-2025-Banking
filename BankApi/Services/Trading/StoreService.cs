namespace BankApi.Services.Trading
{
    using BankApi.Repositories;
    using BankApi.Repositories.Trading;
    using Common.Exceptions;
    using Common.Models.Trading;
    using Common.Services.Trading;
    using Common.Services.Bank;
    using System;
    using System.Threading.Tasks;

    public class StoreService : IStoreService
    {
        private readonly IGemStoreRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly ICreditScoringService _creditScoringService;

        public StoreService(IGemStoreRepository repository, IUserRepository userRepository, ICreditScoringService creditScoringService)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _creditScoringService = creditScoringService ?? throw new ArgumentNullException(nameof(creditScoringService));
        }

        /// <summary>
        /// Retrieves the current gem balance for the specified user.
        /// </summary>
        public async Task<int> GetUserGemBalanceAsync(string userCNP)
        {
            return await _repository.GetUserGemBalanceAsync(userCNP);
        }

        /// <summary>
        /// Updates the gem balance for a given user.
        /// </summary>
        public async Task UpdateUserGemBalanceAsync(int newBalance, string userCNP)
        {
            await _repository.UpdateUserGemBalanceAsync(userCNP, newBalance);
        }

        /// <summary>
        /// Processes a bank transaction for buying gems.
        /// </summary>
        public async Task<string> BuyGems(GemDeal deal, string selectedAccountId, string userCNP)
        {
            bool transactionSuccess = await ProcessBankTransaction(selectedAccountId, -deal.Price);
            if (!transactionSuccess)
            {
                throw new GemTransactionFailedException("Transaction failed. Please check your bank account balance.");
            }

            int currentBalance = await GetUserGemBalanceAsync(userCNP);
            await UpdateUserGemBalanceAsync(currentBalance + deal.GemAmount, userCNP);

            // Apply credit score impact for gem purchase
            await ApplyGemTransactionImpactAsync(userCNP, true, deal.GemAmount, deal.Price);

            return $"Successfully purchased {deal.GemAmount} gems for {deal.Price}€";
        }

        /// <summary>
        /// Processes a bank transaction for selling gems.
        /// </summary>
        public async Task<string> SellGems(int gemAmount, string selectedAccountId, string userCNP)
        {
            int currentBalance = await GetUserGemBalanceAsync(userCNP);
            if (gemAmount > currentBalance)
            {
                throw new InsufficientGemsException($"Not enough gems to sell. You have {currentBalance}, attempted to sell {gemAmount}.");
            }

            double moneyEarned = gemAmount / 100.0;
            bool transactionSuccess = await ProcessBankTransaction(selectedAccountId, moneyEarned);
            if (!transactionSuccess)
            {
                throw new GemTransactionFailedException("Transaction failed. Unable to deposit funds.");
            }

            await UpdateUserGemBalanceAsync(currentBalance - gemAmount, userCNP);

            // Apply credit score impact for gem sale
            await ApplyGemTransactionImpactAsync(userCNP, false, gemAmount, moneyEarned);

            return $"Successfully sold {gemAmount} gems for {moneyEarned}€";
        }

        /// <summary>
        /// Protected virtual method so unit tests can override transaction behavior.
        /// </summary>
        protected virtual Task<bool> ProcessBankTransaction(string accountId, double amount)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Applies credit score impact for gem transactions.
        /// </summary>
        private async Task ApplyGemTransactionImpactAsync(string userCnp, bool isBuying, int gemAmount, double transactionValue)
        {
            try
            {
                int impact = await _creditScoringService.CalculateGemTransactionImpactAsync(userCnp, isBuying, gemAmount, transactionValue);

                if (impact != 0)
                {
                    int currentScore = await _creditScoringService.GetCurrentCreditScoreAsync(userCnp);
                    int newScore = currentScore + impact;

                    string action = isBuying ? "Purchased" : "Sold";
                    string reason = $"{action} {gemAmount} gems for {transactionValue:C}";

                    await _creditScoringService.UpdateCreditScoreAsync(userCnp, newScore, reason);
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the transaction
                Console.WriteLine($"Error applying gem transaction credit impact: {ex.Message}");
            }
        }
    }
}

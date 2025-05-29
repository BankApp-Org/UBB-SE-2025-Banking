namespace BankApi.Services.Stock
{
    using BankApi.Repositories;
    using BankApi.Repositories.Stock;
    using Common.Exceptions;
    using Common.Models.Trading;
    using Common.Services.Stock;
    using System;
    using System.Threading.Tasks;

    public class StoreService : IStoreService
    {
        private readonly IGemStoreRepository _repository;
        private readonly IUserRepository _userRepository;

        public StoreService(IGemStoreRepository repository, IUserRepository userRepository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
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
            return $"Successfully sold {gemAmount} gems for {moneyEarned}€";
        }

        /// <summary>
        /// Protected virtual method so unit tests can override transaction behavior.
        /// </summary>
        protected virtual Task<bool> ProcessBankTransaction(string accountId, double amount)
        {
            return Task.FromResult(true);
        }
    }
}

using BankApi.Repositories.Impl.Bank;
using Common.Models;
using Common.Models.Bank;
using Common.Services.Bank;

namespace BankApi.Services.Bank
{
    public class BankTransactionService : IBankTransactionService
    {
        private readonly IBankTransactionRepository _transactionRepository;
        private readonly IBankTransactionHistoryRepository _historyRepository;
        private readonly ICreditScoringService _creditScoringService;

        public BankTransactionService(
            IBankTransactionRepository transactionRepository,
            IBankTransactionHistoryRepository historyRepository,
            ICreditScoringService creditScoringService)
        {
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
            _creditScoringService = creditScoringService ?? throw new ArgumentNullException(nameof(creditScoringService));
        }

        public async Task<List<BankTransaction>> GetTransactions(TransactionFilters filters)
        {
            if (filters == null)
            {
                throw new ArgumentNullException(nameof(filters), "Transaction filters cannot be null");
            }

            try
            {
                // Get all transactions first
                var transactions = await _historyRepository.GetAllTransactionsAsync();

                // Apply filters
                var filteredTransactions = transactions
                    .Where(t => t.TransactionType == filters.Type)
                    .Where(t => t.TransactionDatetime >= filters.StartDate && t.TransactionDatetime <= filters.EndDate);

                // Apply sender IBAN filter if provided
                if (!string.IsNullOrEmpty(filters.SenderIban))
                {
                    filteredTransactions = filteredTransactions.Where(t => t.SenderIban == filters.SenderIban);
                }

                // Apply receiver IBAN filter if provided
                if (!string.IsNullOrEmpty(filters.ReceiverIban))
                {
                    filteredTransactions = filteredTransactions.Where(t => t.ReceiverIban == filters.ReceiverIban);
                }

                // Apply amount filters if provided
                if (filters.MinAmount.HasValue)
                {
                    filteredTransactions = filteredTransactions.Where(t => t.SenderAmount >= filters.MinAmount.Value);
                }

                if (filters.MaxAmount.HasValue)
                {
                    filteredTransactions = filteredTransactions.Where(t => t.SenderAmount <= filters.MaxAmount.Value);
                }

                // Apply ordering
                var orderedTransactions = filters.Ordering == Ordering.Ascending
                    ? filteredTransactions.OrderBy(t => t.TransactionDatetime)
                    : filteredTransactions.OrderByDescending(t => t.TransactionDatetime);

                return orderedTransactions.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving transactions: {ex.Message}", ex);
            }
        }

        public async Task<BankTransaction> GetTransactionById(int transactionId)
        {
            if (transactionId <= 0)
            {
                throw new ArgumentException("Transaction ID must be greater than zero.", nameof(transactionId));
            }

            try
            {
                return await _historyRepository.GetTransactionByIdAsync(transactionId);
            }
            catch (KeyNotFoundException)
            {
                // Return null if transaction not found instead of throwing exception
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving transaction with ID {transactionId}: {ex.Message}", ex);
            }
        }

        public async Task<bool> CreateTransaction(BankTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction), "Transaction cannot be null");
            }

            try
            {
                // Set transaction datetime if not already set
                if (transaction.TransactionDatetime == default)
                {
                    transaction.TransactionDatetime = DateTime.UtcNow;
                }

                await _historyRepository.CreateTransactionAsync(transaction);

                // Calculate and apply credit score impact for sender
                await ApplyCreditScoreImpactAsync(transaction);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating transaction: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateTransaction(BankTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction), "Transaction cannot be null");
            }

            try
            {
                await _historyRepository.UpdateTransactionAsync(transaction);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating transaction: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteTransaction(int transactionId)
        {
            if (transactionId <= 0)
            {
                throw new ArgumentException("Transaction ID must be greater than zero.", nameof(transactionId));
            }

            try
            {
                await _historyRepository.DeleteTransactionAsync(transactionId);
                return true;
            }
            catch (KeyNotFoundException)
            {
                // Return false if transaction not found
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting transaction with ID {transactionId}: {ex.Message}", ex);
            }
        }

        public async Task<List<TransactionTypeCountDTO>> GetTransactionTypeCounts(int userId)
        {
            try
            {
                var counts = await _historyRepository.GetTransactionTypeCountsAsync(userId);
                return [.. counts.Select(c => new TransactionTypeCountDTO
                {
                    TransactionType = c.TransactionType,
                    Count = c.Count
                })];
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving transaction type counts for user {userId}: {ex.Message}", ex);
            }
        }

        private async Task ApplyCreditScoreImpactAsync(BankTransaction transaction)
        {
            try
            {
                // Extract user CNP from IBAN (assuming IBAN contains CNP or user identifier)
                string senderCnp = ExtractUserCnpFromIban(transaction.SenderIban);
                string receiverCnp = ExtractUserCnpFromIban(transaction.ReceiverIban);

                // Apply credit score impact for sender
                if (!string.IsNullOrEmpty(senderCnp))
                {
                    int senderImpact = await _creditScoringService.CalculateBankTransactionImpactAsync(senderCnp, transaction);
                    if (senderImpact != 0)
                    {
                        int currentScore = await _creditScoringService.GetCurrentCreditScoreAsync(senderCnp);
                        int newScore = currentScore + senderImpact;
                        await _creditScoringService.UpdateCreditScoreAsync(senderCnp, newScore,
                            $"Bank transaction: {transaction.TransactionType} of {transaction.SenderAmount:C}");
                    }
                }

                // Apply credit score impact for receiver (for deposits)
                if (!string.IsNullOrEmpty(receiverCnp) && transaction.TransactionType == TransactionType.Deposit)
                {
                    // Create a receiver transaction for credit calculation
                    var receiverTransaction = new BankTransaction
                    {
                        TransactionType = TransactionType.Deposit,
                        SenderAmount = transaction.ReceiverAmount,
                        SenderIban = transaction.ReceiverIban,
                        ReceiverIban = transaction.SenderIban,
                        TransactionDatetime = transaction.TransactionDatetime,
                        SenderCurrency = transaction.ReceiverCurrency,
                        ReceiverCurrency = transaction.SenderCurrency,
                        TransactionDescription = "Credit score calculation transaction"
                    };

                    int receiverImpact = await _creditScoringService.CalculateBankTransactionImpactAsync(receiverCnp, receiverTransaction);
                    if (receiverImpact != 0)
                    {
                        int currentScore = await _creditScoringService.GetCurrentCreditScoreAsync(receiverCnp);
                        int newScore = currentScore + receiverImpact;
                        await _creditScoringService.UpdateCreditScoreAsync(receiverCnp, newScore,
                            $"Received transfer: {transaction.ReceiverAmount:C}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the transaction
                Console.WriteLine($"Error applying credit score impact: {ex.Message}");
            }
        }

        private string ExtractUserCnpFromIban(string iban)
        {
            if (string.IsNullOrEmpty(iban)) return null;

            // This is a simplified implementation - in a real system, you'd need to
            // look up the account owner by IBAN in the database
            // For now, assume the IBAN contains or can be mapped to a CNP

            // If IBAN is in format "RO49AAAA1B31007593840000" and contains CNP
            // This would need to be implemented based on your actual IBAN structure

            // Placeholder implementation - you'll need to implement actual IBAN to CNP mapping
            return iban.Length >= 13 ? iban.Substring(iban.Length - 13) : null;
        }
    }
}

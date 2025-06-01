using BankApi.Data;
using Common.Models.Bank;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Repositories.Impl.Bank
{
    public class BankTransactionHistoryRepository : IBankTransactionHistoryRepository
    {
        private readonly ApiDbContext _context;

        public BankTransactionHistoryRepository(ApiDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<BankTransaction>> GetTransactionHistoryByIbanAsync(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
            {
                throw new ArgumentException("IBAN cannot be null or empty.", nameof(iban));
            }
            try
            {
                return await _context.BankTransactions
                    .Include(t => t.SenderAccount)
                    .Include(t => t.ReceiverAccount)
                    .Where(t => t.ReceiverIban == iban || t.SenderIban == iban)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving transaction history for IBAN {iban}: {ex.Message}", ex);
            }
        }

        public async Task UpdateTransactionAsync(BankTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction), "Transaction cannot be null");
            }
            try
            {
                _context.BankTransactions.Update(transaction);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Error updating transaction: {ex.Message}", ex);
            }
        }

        public async Task DeleteTransactionAsync(int transactionId)
        {
            try
            {
                var transaction = await _context.BankTransactions.FindAsync(transactionId);
                if (transaction == null)
                {
                    throw new KeyNotFoundException($"Transaction with ID {transactionId} not found.");
                }
                _context.BankTransactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Error deleting transaction: {ex.Message}", ex);
            }
        }
        public async Task<BankTransaction> GetTransactionByIdAsync(int transactionId)
        {
            try
            {
                var transaction = await _context.BankTransactions
                    .Include(t => t.SenderAccount)
                    .Include(t => t.ReceiverAccount)
                    .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

                if (transaction == null)
                {
                    throw new KeyNotFoundException($"Transaction with ID {transactionId} not found.");
                }

                return transaction;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving transaction by ID {transactionId}: {ex.Message}", ex);
            }
        }

        public async Task<List<BankTransaction>> GetAllTransactionsAsync()
        {
            try
            {
                return await _context.BankTransactions
                    .Include(t => t.SenderAccount)
                    .Include(t => t.ReceiverAccount)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving all transactions: {ex.Message}", ex);
            }
        }

        public async Task CreateTransactionAsync(BankTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction), "Transaction cannot be null");
            }
            try
            {
                _context.BankTransactions.Add(transaction);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Error creating transaction: {ex.Message}", ex);
            }
        }
    }
}

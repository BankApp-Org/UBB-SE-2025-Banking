using BankApi.Data;
using Common.Models.Bank;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Repositories.Impl.Bank
{
    public class BankTransactionRepository
    {
        private readonly ApiDbContext _dbContext;

        public BankTransactionRepository(ApiDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<bool> AddTransaction(BankTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction), "Transaction cannot be null");
            }
            try
            {
                _dbContext.BankTransactions.Add(transaction);
                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error in AddTransaction: {ex.Message}", ex);
            }
        }

        public async Task<List<BankAccount>> GetAllBankAccounts()
        {
            try
            {
                List<BankAccount> bankAccounts = await _dbContext.BankAccounts.ToListAsync();
                return bankAccounts;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error in GetAllBankAccounts: {ex.Message}", ex);
            }
        }

        public async Task<List<CurrencyExchange>> GetAllCurrencyExchangeRates()
        {
            try
            {
                List<CurrencyExchange> exchangeRates = await _dbContext.CurrencyExchanges
                    .ToListAsync();
                return exchangeRates;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error in GetAllCurrencyExchangeRates: {ex.Message}", ex);
            }
        }

        public async Task<BankAccount> GetBankAccountByIBAN(string iban)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(iban))
                {
                    throw new ArgumentException("IBAN cannot be empty.", nameof(iban));
                }

                BankAccount account = await _dbContext.BankAccounts
                    .FirstOrDefaultAsync(acc => acc.Iban == iban) ?? throw new Exception($"Bank account with IBAN {iban} not found.");

                return account;
            }
            catch (Exception ex)
            {
                throw new Exception($"ORM error in GetBankAccountByIBAN: {ex.Message}", ex);
            }
        }

        public async Task<List<BankTransaction>> GetBankAccountTransactions(string iban)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(iban))
                {
                    throw new ArgumentException("IBAN cannot be empty.", nameof(iban));
                }

                var transactions = await _dbContext.BankTransactions
                .Where(t => t.SenderIban == iban || t.ReceiverIban == iban)
                .ToListAsync();

                return transactions;
            }
            catch (Exception ex)
            {
                throw new Exception($"ORM error in GetBankAccountTransactions: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetExchangeRate(Currency fromCurrency, Currency toCurrency)
        {
            try
            {
                var exchangeRate = await _dbContext.CurrencyExchanges
                    .Where(c => c.FromCurrency == fromCurrency && c.ToCurrency == toCurrency)
                    .Select(c => c.ExchangeRate)
                    .FirstOrDefaultAsync();

                if (exchangeRate == 0)
                {
                    throw new Exception("Exchange rate not found for the provided currencies.");
                }

                return exchangeRate;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving exchange rate: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateBankAccountBalance(string iban, decimal newBalance)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(iban))
                {
                    throw new ArgumentException("IBAN must be provided.", nameof(iban));
                }

                if (newBalance < 0)
                {
                    throw new ArgumentException("Balance cannot be negative.");
                }

                var bankAccount = await _dbContext.BankAccounts
                    .FirstOrDefaultAsync(b => b.Iban == iban);

                if (bankAccount == null)
                {
                    throw new Exception($"Bank account with IBAN {iban} not found.");
                }

                bankAccount.Balance = newBalance;

                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating bank account balance: {ex.Message}", ex);
            }
        }
    }
}

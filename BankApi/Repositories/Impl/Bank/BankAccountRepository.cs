using BankApi.Data;
using Common.Models.Bank;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Repositories.Impl.Bank
{
    public class BankAccountRepository : IBankAccountRepository
    {
        private readonly ApiDbContext _context;

        public BankAccountRepository(ApiDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<BankAccount> GetBankAccountBalanceByIbanAsync(string IBAN)
        {
            if (string.IsNullOrWhiteSpace(IBAN))
            {
                throw new ArgumentException("IBAN cannot be null or empty.", nameof(IBAN));
            }

            var account = await _context.BankAccounts
                .Include(b => b.Transactions)
                .FirstOrDefaultAsync(b => b.Iban == IBAN) ?? throw new Exception($"Bank account with IBAN {IBAN} not found.");

            return account;
        }

        public async Task<List<BankAccount>> GetBankAccountsByUserIdAsync(int userId)
        {
            // CU INCLUDE NU MERGE, IDK 
            return await _context.BankAccounts
                //.Include(b => b.Currency)
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }

        public async Task<BankAccount> CreateBankAccountAsync(BankAccount bankAccount)
        {
            if (bankAccount == null)
            {
                throw new ArgumentNullException(nameof(bankAccount), "Bank account cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(bankAccount.Iban))
            {
                throw new ArgumentException("IBAN cannot be null or empty.", nameof(bankAccount.Iban));
            }
            if (bankAccount.UserId <= 0)
            {
                throw new ArgumentException("User ID must be greater than 0.", nameof(bankAccount.UserId));
            }

            _context.BankAccounts.Add(bankAccount);
            await _context.SaveChangesAsync();
            return bankAccount;
        }

        public async Task<bool> UpdateBankAccountAsync(BankAccount bankAccount)
        {
            if (bankAccount == null)
            {
                throw new ArgumentNullException(nameof(bankAccount), "Bank account cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(bankAccount.Iban))
            {
                throw new ArgumentException("IBAN cannot be null or empty.", nameof(bankAccount.Iban));
            }
            _context.BankAccounts.Update(bankAccount);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteBankAccountAsync(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
            {
                throw new ArgumentException("IBAN cannot be null or empty.", nameof(iban));
            }
            var bankAccount = await _context.BankAccounts.FirstOrDefaultAsync(b => b.Iban == iban);
            if (bankAccount == null)
            {
                throw new Exception($"Bank account with IBAN {iban} not found.");
            }
            _context.BankAccounts.Remove(bankAccount);
            return await _context.SaveChangesAsync() > 0;

        }

        public async Task<List<BankAccount>> GetAllBankAccountsAsync()
        {
            return await _context.BankAccounts.Include(b => b.Currency).ToListAsync();
        }
    }
}

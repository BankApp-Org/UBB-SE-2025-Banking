using BankApi.Repositories.Impl.Bank;
using Common.Models.Bank;
using Common.Services.Bank;

namespace BankApi.Services.Bank
{
    public class BankAccountService : IBankAccountService
    {
        private readonly IBankAccountRepository _bankAccountRepository;
        private readonly ICurrencyExchangeRepository _currencyExchangeRepository;
        private const string CountryCode = "RO";
        private const string BankCode = "BANK";
        private const int AccountNumberLength = 16;
        private readonly Random _random = new();

        public BankAccountService(IBankAccountRepository bankAccountRepository, ICurrencyExchangeRepository currencyExchangeRepository)
        {
            _bankAccountRepository = bankAccountRepository ?? throw new ArgumentNullException(nameof(bankAccountRepository));
            _currencyExchangeRepository = currencyExchangeRepository ?? throw new ArgumentNullException(nameof(currencyExchangeRepository));
        }

        public async Task<List<BankAccount>> GetUserBankAccounts(int userID)
        {
            if (userID <= 0)
            {
                throw new ArgumentException("User ID must be greater than 0.", nameof(userID));
            }

            try
            {
                return await _bankAccountRepository.GetBankAccountsByUserIdAsync(userID);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving bank accounts for user {userID}", ex);
            }
        }

        public async Task<BankAccount?> FindBankAccount(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
            {
                throw new ArgumentException("IBAN cannot be null or empty.", nameof(iban));
            }

            try
            {
                return await _bankAccountRepository.GetBankAccountBalanceByIbanAsync(iban);
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                // Return null when the account is not found instead of throwing an exception
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error finding bank account with IBAN {iban}", ex);
            }
        }

        public async Task<bool> CreateBankAccount(BankAccount bankAccount)
        {
            if (bankAccount == null)
            {
                throw new ArgumentNullException(nameof(bankAccount), "Bank account cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(bankAccount.Iban))
            {
                // Generate IBAN if not provided
                bankAccount.Iban = await GenerateIBAN();
            }

            try
            {
                var createdAccount = await _bankAccountRepository.CreateBankAccountAsync(bankAccount);
                return createdAccount != null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating bank account", ex);
            }
        }

        public async Task<bool> RemoveBankAccount(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
            {
                throw new ArgumentException("IBAN cannot be null or empty.", nameof(iban));
            }

            try
            {
                return await _bankAccountRepository.DeleteBankAccountAsync(iban);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing bank account with IBAN {iban}", ex);
            }
        }

        public async Task<bool> CheckIBANExists(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
            {
                throw new ArgumentException("IBAN cannot be null or empty.", nameof(iban));
            }

            try
            {
                var account = await FindBankAccount(iban);
                return account != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> GenerateIBAN()
        {
            string iban;
            bool exists;

            do
            {
                // Format: ROxx BANK 0000 0000 0000 0000
                string accountNumber = GenerateRandomAccountNumber(AccountNumberLength);
                iban = $"{CountryCode}{CalculateCheckDigits(CountryCode, BankCode, accountNumber)}{BankCode}{accountNumber}";
                exists = await CheckIBANExists(iban);
            }
            while (exists);

            return iban;
        }

        private string GenerateRandomAccountNumber(int length)
        {
            var chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        private string CalculateCheckDigits(string countryCode, string bankCode, string accountNumber)
        {
            // This is a simplified IBAN check digit calculation - in a real application, use a proper IBAN validation library
            // Actual algorithm: move country code to the end, convert letters to digits (A=10, B=11...), calculate MOD 97
            // Then subtract from 98 to get the check digits

            // For this implementation, we'll just use a placeholder
            return "42";
        }

        public async Task<bool> UpdateBankAccount(BankAccount bankAccount)
        {
            if (bankAccount == null)
            {
                throw new ArgumentNullException(nameof(bankAccount), "Bank account cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(bankAccount.Iban))
            {
                throw new ArgumentException("IBAN cannot be null or empty.", nameof(bankAccount.Iban));
            }

            try
            {
                return await _bankAccountRepository.UpdateBankAccountAsync(bankAccount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating bank account with IBAN {bankAccount.Iban}", ex);
            }
        }

        public async Task<decimal> ConvertCurrency(decimal amount, Currency fromCurrency, Currency toCurrency)
        {
            if (amount < 0)
            {
                throw new ArgumentException("Amount must be non-negative", nameof(amount));
            }

            if (fromCurrency == toCurrency)
            {
                return amount; // No conversion needed
            }

            try
            {
                var exchangeRate = await GetExchangeRateAsync(fromCurrency, toCurrency);
                return amount * exchangeRate.ExchangeRate;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting {amount} from {fromCurrency} to {toCurrency}", ex);
            }
        }

        public async Task<CurrencyExchange> GetExchangeRateAsync(Currency fromCurrency, Currency toCurrency)
        {
            try
            {
                return await _currencyExchangeRepository.GetExchangeRateAsync(fromCurrency, toCurrency);
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                // Forward "not found" exceptions to callers
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving exchange rate from {fromCurrency} to {toCurrency}", ex);
            }
        }

        public async Task<bool> CheckSufficientFunds(int userID, string accountIBAN, decimal amount, Currency currency)
        {
            if (userID <= 0)
            {
                throw new ArgumentException("User ID must be greater than 0.", nameof(userID));
            }
            if (string.IsNullOrWhiteSpace(accountIBAN))
            {
                throw new ArgumentException("IBAN cannot be null or empty.", nameof(accountIBAN));
            }
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than 0.", nameof(amount));
            }

            try
            {
                // Check if the account belongs to the user
                var userAccounts = await GetUserBankAccounts(userID);
                var account = userAccounts.FirstOrDefault(a => a.Iban == accountIBAN);

                if (account == null)
                {
                    throw new UnauthorizedAccessException($"Account with IBAN {accountIBAN} does not belong to user {userID}");
                }

                // Convert the amount to the account's currency if needed
                decimal requiredAmountInAccountCurrency = amount;
                if (currency != account.Currency)
                {
                    requiredAmountInAccountCurrency = await ConvertCurrency(amount, currency, account.Currency);
                }

                // Check if the account has sufficient funds
                return account.Balance >= requiredAmountInAccountCurrency;
            }
            catch (UnauthorizedAccessException)
            {
                throw; // Rethrow unauthorized access exceptions
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking sufficient funds for account {accountIBAN}", ex);
            }
        }

        public async Task<List<CurrencyExchange>> GetAllExchangeRatesAsync()
        {
            return await _currencyExchangeRepository.GetAllExchangeRatesAsync();
        }
    }
}

using BankApi.Data;
using BankApi.Repositories.Impl.Bank;
using Common.Models.Bank;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankApp.Repository.Tests
{
    [TestClass]
    public class BankTransactionRepositoryTests
    {
        private ApiDbContext _context;
        private BankTransactionRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new ApiDbContext(options);
            // Add users to the context and save
            var user1 = new Common.Models.User { Id = 1, CNP = "CNP1", FirstName = "Test", LastName = "User" };
            var user2 = new Common.Models.User { Id = 2, CNP = "CNP2", FirstName = "Test2", LastName = "User2" };
            _context.Users.AddRange(user1, user2);
            _context.SaveChanges();
            _repository = new BankTransactionRepository(_context);
        }

        private BankAccount CreateValidAccount(string iban = "IBAN1", int userId = 1)
        {
            var user = _context.Users.First(u => u.Id == userId);
            return new BankAccount
            {
                Iban = iban,
                Currency = Currency.RON,
                Blocked = false,
                Name = "Test Account",
                Transactions = new List<BankTransaction>(),
                User = user,
                UserId = userId,
                Balance = 100,
                DailyLimit = 1000,
                MaximumPerTransaction = 500,
                MaximumNrTransactions = 10
            };
        }

        private BankTransaction CreateValidTransaction(int id = 1)
        {
            var sender = CreateValidAccount("IBAN1", 1);
            var receiver = CreateValidAccount("IBAN2", 2);
            _context.BankAccounts.AddRange(sender, receiver);
            _context.SaveChanges();
            return new BankTransaction
            {
                TransactionId = id,
                SenderIban = sender.Iban,
                ReceiverIban = receiver.Iban,
                SenderAccount = sender,
                ReceiverAccount = receiver,
                TransactionDatetime = DateTime.UtcNow,
                SenderCurrency = Currency.RON,
                ReceiverCurrency = Currency.RON,
                SenderAmount = 100,
                ReceiverAmount = 100,
                TransactionType = TransactionType.Deposit,
                TransactionDescription = "Test Transaction"
            };
        }

        [TestMethod]
        public async Task AddTransaction_AddsTransaction()
        {
            var transaction = CreateValidTransaction();
            var result = await _repository.AddTransaction(transaction);
            Assert.IsTrue(result);
            Assert.AreEqual(1, _context.BankTransactions.Count());
        }

        [TestMethod]
        public async Task GetAllBankAccounts_ReturnsAll()
        {
            var account1 = CreateValidAccount("IBAN1", 1);
            var account2 = CreateValidAccount("IBAN2", 2);
            _context.BankAccounts.AddRange(account1, account2);
            _context.SaveChanges();
            var result = await _repository.GetAllBankAccounts();
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task GetAllCurrencyExchangeRates_ReturnsAll()
        {
            var rate = new CurrencyExchange { Id = 1, FromCurrency = Currency.RON, ToCurrency = Currency.EUR, ExchangeRate = 4.5M };
            _context.CurrencyExchanges.Add(rate);
            _context.SaveChanges();
            var result = await _repository.GetAllCurrencyExchangeRates();
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetBankAccountByIBAN_ReturnsAccount()
        {
            var account = CreateValidAccount();
            _context.BankAccounts.Add(account);
            _context.SaveChanges();
            var result = await _repository.GetBankAccountByIBAN(account.Iban);
            Assert.IsNotNull(result);
            Assert.AreEqual(account.Iban, result.Iban);
        }

        [TestMethod]
        public async Task GetBankAccountTransactions_ReturnsTransactions()
        {
            var transaction = CreateValidTransaction();
            _context.BankTransactions.Add(transaction);
            _context.SaveChanges();
            var result = await _repository.GetBankAccountTransactions("IBAN1");
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetExchangeRate_ReturnsRate()
        {
            var rate = new CurrencyExchange { Id = 1, FromCurrency = Currency.RON, ToCurrency = Currency.EUR, ExchangeRate = 4.5M };
            _context.CurrencyExchanges.Add(rate);
            _context.SaveChanges();
            var result = await _repository.GetExchangeRate(Currency.RON, Currency.EUR);
            Assert.AreEqual(4.5M, result);
        }

        [TestMethod]
        public async Task UpdateBankAccountBalance_UpdatesBalance()
        {
            var account = CreateValidAccount();
            _context.BankAccounts.Add(account);
            _context.SaveChanges();
            var result = await _repository.UpdateBankAccountBalance(account.Iban, 200);
            Assert.IsTrue(result);
            var updated = _context.BankAccounts.First();
            Assert.AreEqual(200, updated.Balance);
        }
    }
} 
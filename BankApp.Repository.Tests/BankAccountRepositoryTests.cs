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
    public class BankAccountRepositoryTests
    {
        private ApiDbContext _context;
        private BankAccountRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new ApiDbContext(options);
            var user1 = new Common.Models.User { Id = 1, CNP = "CNP1", FirstName = "Test", LastName = "User" };
            var user2 = new Common.Models.User { Id = 2, CNP = "CNP2", FirstName = "Test2", LastName = "User2" };
            _context.Users.AddRange(user1, user2);
            _context.SaveChanges();
            _repository = new BankAccountRepository(_context);
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
                Transactions = new List<Common.Models.Bank.BankTransaction>(),
                User = user,
                UserId = userId,
                Balance = 100,
                DailyLimit = 1000,
                MaximumPerTransaction = 500,
                MaximumNrTransactions = 10
            };
        }

        [TestMethod]
        public async Task CreateBankAccountAsync_AddsAccount()
        {
            var account = CreateValidAccount();
            var result = await _repository.CreateBankAccountAsync(account);
            Assert.IsNotNull(result);
            Assert.AreEqual(account.Iban, result.Iban);
        }

        [TestMethod]
        public async Task GetBankAccountBalanceByIbanAsync_ReturnsAccount()
        {
            var account = CreateValidAccount();
            _context.BankAccounts.Add(account);
            _context.SaveChanges();
            var result = await _repository.GetBankAccountBalanceByIbanAsync(account.Iban);
            Assert.IsNotNull(result);
            Assert.AreEqual(account.Iban, result.Iban);
        }

        [TestMethod]
        public async Task GetBankAccountsByUserIdAsync_ReturnsAccounts()
        {
            var account = CreateValidAccount();
            _context.BankAccounts.Add(account);
            _context.SaveChanges();
            var result = await _repository.GetBankAccountsByUserIdAsync(account.UserId);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetAllBankAccountsAsync_ReturnsAll()
        {
            var account1 = CreateValidAccount("IBAN1", 1);
            var account2 = CreateValidAccount("IBAN2", 2);
            _context.BankAccounts.AddRange(account1, account2);
            _context.SaveChanges();
            var result = await _repository.GetAllBankAccountsAsync();
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task UpdateBankAccountAsync_UpdatesAccount()
        {
            var account = CreateValidAccount();
            _context.BankAccounts.Add(account);
            _context.SaveChanges();
            account.Name = "Updated Name";
            var updated = await _repository.UpdateBankAccountAsync(account);
            Assert.IsTrue(updated);
            var fetched = await _repository.GetBankAccountBalanceByIbanAsync(account.Iban);
            Assert.AreEqual("Updated Name", fetched.Name);
        }

        [TestMethod]
        public async Task DeleteBankAccountAsync_DeletesAccount()
        {
            var account = CreateValidAccount();
            _context.BankAccounts.Add(account);
            _context.SaveChanges();
            var deleted = await _repository.DeleteBankAccountAsync(account.Iban);
            Assert.IsTrue(deleted);
            Assert.AreEqual(0, _context.BankAccounts.Count());
        }

        [TestMethod]
        public async Task CreateBankAccountAsync_Null_Throws()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _repository.CreateBankAccountAsync(null));
        }

        [TestMethod]
        public async Task GetBankAccountBalanceByIbanAsync_InvalidIban_Throws()
        {
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _repository.GetBankAccountBalanceByIbanAsync(""));
        }
    }
} 
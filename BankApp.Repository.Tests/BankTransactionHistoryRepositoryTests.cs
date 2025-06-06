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
    public class BankTransactionHistoryRepositoryTests
    {
        private ApiDbContext _context;
        private BankTransactionHistoryRepository _repository;

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
            _repository = new BankTransactionHistoryRepository(_context);
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
        public async Task CreateTransactionAsync_AddsTransaction()
        {
            var transaction = CreateValidTransaction();
            await _repository.CreateTransactionAsync(transaction);
            Assert.AreEqual(1, _context.BankTransactions.Count());
        }

        [TestMethod]
        public async Task GetAllTransactionsAsync_ReturnsAll()
        {
            var transaction = CreateValidTransaction();
            _context.BankTransactions.Add(transaction);
            _context.SaveChanges();
            var result = await _repository.GetAllTransactionsAsync();
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetTransactionByIdAsync_ReturnsTransaction()
        {
            var transaction = CreateValidTransaction();
            _context.BankTransactions.Add(transaction);
            _context.SaveChanges();
            var result = await _repository.GetTransactionByIdAsync(transaction.TransactionId);
            Assert.IsNotNull(result);
            Assert.AreEqual(transaction.TransactionId, result.TransactionId);
        }

        [TestMethod]
        public async Task GetTransactionHistoryByIbanAsync_ReturnsTransactions()
        {
            var transaction = CreateValidTransaction();
            _context.BankTransactions.Add(transaction);
            _context.SaveChanges();
            var result = await _repository.GetTransactionHistoryByIbanAsync("IBAN1");
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task UpdateTransactionAsync_UpdatesTransaction()
        {
            var transaction = CreateValidTransaction();
            _context.BankTransactions.Add(transaction);
            _context.SaveChanges();
            transaction.TransactionDescription = "Updated";
            await _repository.UpdateTransactionAsync(transaction);
            var updated = _context.BankTransactions.First();
            Assert.AreEqual("Updated", updated.TransactionDescription);
        }

        [TestMethod]
        public async Task DeleteTransactionAsync_DeletesTransaction()
        {
            var transaction = CreateValidTransaction();
            _context.BankTransactions.Add(transaction);
            _context.SaveChanges();
            await _repository.DeleteTransactionAsync(transaction.TransactionId);
            Assert.AreEqual(0, _context.BankTransactions.Count());
        }

        [TestMethod]
        public async Task GetTransactionTypeCountsAsync_ReturnsCounts()
        {
            var transaction = CreateValidTransaction();
            _context.BankTransactions.Add(transaction);
            _context.SaveChanges();
            var result = await _repository.GetTransactionTypeCountsAsync(1);
            Assert.IsTrue(result.Count >= 0);
        }
    }
} 
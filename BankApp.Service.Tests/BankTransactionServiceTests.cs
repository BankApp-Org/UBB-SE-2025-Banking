using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using BankApi.Services.Bank;
using BankApi.Repositories.Impl.Bank;
using Common.Models.Bank;
using Common.Services.Bank;
using Common.Models.Social;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Common.Models;

namespace BankApp.Service.Tests
{
    [TestClass]
    public class BankTransactionServiceTests
    {
        private Mock<IBankTransactionRepository> _transactionRepoMock;
        private Mock<IBankTransactionHistoryRepository> _historyRepoMock;
        private BankTransactionService _service;

        [TestInitialize]
        public void Setup()
        {
            _transactionRepoMock = new Mock<IBankTransactionRepository>();
            _historyRepoMock = new Mock<IBankTransactionHistoryRepository>();
            _service = new BankTransactionService(_transactionRepoMock.Object, _historyRepoMock.Object);
        }

        private BankAccount CreateValidBankAccount(string iban = "IBAN1", int userId = 1)
        {
            return new BankAccount
            {
                Iban = iban,
                Currency = Currency.RON,
                Blocked = false,
                Name = "Test Account",
                Transactions = new List<BankTransaction>(),
                User = new User { Id = userId, CNP = "CNP", FirstName = "Test", LastName = "User" },
                UserId = userId,
                Balance = 100,
                DailyLimit = 1000,
                MaximumPerTransaction = 500,
                MaximumNrTransactions = 10
            };
        }

        private BankTransaction CreateValidBankTransaction(int id = 1)
        {
            return new BankTransaction
            {
                TransactionId = id,
                SenderIban = "IBAN1",
                ReceiverIban = "IBAN2",
                SenderAccount = CreateValidBankAccount("IBAN1", 1),
                ReceiverAccount = CreateValidBankAccount("IBAN2", 2),
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
        public async Task GetTransactions_ValidFilters_ReturnsFilteredTransactions()
        {
            var now = DateTime.UtcNow;
            var filters = new TransactionFilters { Type = TransactionType.Deposit, StartDate = now.AddDays(-10), EndDate = now, Ordering = Ordering.Ascending };
            var transaction = CreateValidBankTransaction();
            transaction.TransactionDatetime = now.AddDays(-5);
            var transactions = new List<BankTransaction> { transaction };
            _historyRepoMock.Setup(r => r.GetAllTransactionsAsync()).ReturnsAsync(transactions);
            var result = await _service.GetTransactions(filters);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetTransactions_NullFilters_Throws()
        {
            await _service.GetTransactions(null);
        }

        [TestMethod]
        public async Task GetTransactionById_ValidId_ReturnsTransaction()
        {
            var transaction = CreateValidBankTransaction(1);
            _historyRepoMock.Setup(r => r.GetTransactionByIdAsync(1)).ReturnsAsync(transaction);
            var result = await _service.GetTransactionById(1);
            Assert.AreEqual(1, result.TransactionId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetTransactionById_InvalidId_Throws()
        {
            await _service.GetTransactionById(0);
        }

        [TestMethod]
        public async Task GetTransactionById_NotFound_ReturnsNull()
        {
            _historyRepoMock.Setup(r => r.GetTransactionByIdAsync(2)).ThrowsAsync(new KeyNotFoundException());
            var result = await _service.GetTransactionById(2);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CreateTransaction_ValidTransaction_ReturnsTrue()
        {
            var transaction = CreateValidBankTransaction(1);
            _historyRepoMock.Setup(r => r.CreateTransactionAsync(transaction)).Returns(Task.CompletedTask);
            var result = await _service.CreateTransaction(transaction);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateTransaction_NullTransaction_Throws()
        {
            await _service.CreateTransaction(null);
        }

        [TestMethod]
        public async Task UpdateTransaction_ValidTransaction_ReturnsTrue()
        {
            var transaction = CreateValidBankTransaction(1);
            _historyRepoMock.Setup(r => r.UpdateTransactionAsync(transaction)).Returns(Task.CompletedTask);
            var result = await _service.UpdateTransaction(transaction);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateTransaction_NullTransaction_Throws()
        {
            await _service.UpdateTransaction(null);
        }
    }
} 
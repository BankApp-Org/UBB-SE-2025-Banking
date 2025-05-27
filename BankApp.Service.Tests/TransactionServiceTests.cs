using BankApi.Repositories;
using BankApi.Services;
using Common.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace StockApp.Service.Tests
{
    [TestClass]
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class TransactionServiceTests
    {
        private Mock<ITransactionRepository> _mockRepo;
        private TransactionService _service;

        [TestInitialize]
        public void Init()
        {
            _mockRepo = new Mock<ITransactionRepository>();
            _service = new TransactionService(_mockRepo.Object);
        }

        [TestMethod]
        public async Task AddTransactionAsync_HappyCase_AddsTransaction()
        {
            var author = new User { CNP = "1234567890123", FirstName = "John", LastName = "Doe" };
            var transaction = new TransactionLogTransaction
            {
                StockSymbol = "TEST",
                StockName = "Test Stock",
                Type = "BUY",
                Amount = 10,
                PricePerStock = 100,
                Date = DateTime.Now,
                AuthorCNP = "1234567890123",
                Author = author
            };

            _mockRepo.Setup(r => r.AddTransactionAsync(transaction)).Returns(Task.CompletedTask);
            await _service.AddTransactionAsync(transaction);
            _mockRepo.Verify(r => r.AddTransactionAsync(transaction), Times.Once);
        }

        [TestMethod]
        public async Task AddTransactionAsync_NullTransaction_RepositoryThrows_PropagatesException()
        {
            _mockRepo.Setup(r => r.AddTransactionAsync(null)).ThrowsAsync(new ArgumentNullException());
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await _service.AddTransactionAsync(null));
        }

        [TestMethod]
        public async Task AddTransactionAsync_RepositoryThrows_PropagatesException()
        {
            var author = new User { CNP = "1234567890123", FirstName = "John", LastName = "Doe" };
            var transaction = new TransactionLogTransaction
            {
                StockSymbol = "TEST",
                StockName = "Test Stock",
                Type = "BUY",
                Amount = 10,
                PricePerStock = 100,
                Date = DateTime.Now,
                AuthorCNP = "1234567890123",
                Author = author
            };

            _mockRepo.Setup(r => r.AddTransactionAsync(transaction)).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _service.AddTransactionAsync(transaction));
        }

        [TestMethod]
        public async Task GetAllTransactionsAsync_HappyCase_ReturnsList()
        {
            var author = new User { CNP = "1234567890123", FirstName = "John", LastName = "Doe" };
            var transactions = new List<TransactionLogTransaction>
            {
                new() {
                    StockSymbol = "TEST",
                    StockName = "Test Stock",
                    Type = "BUY",
                    Amount = 10,
                    PricePerStock = 100,
                    Date = DateTime.Now,
                    AuthorCNP = "1234567890123",
                    Author = author
                }
            };

            _mockRepo.Setup(r => r.getAllTransactions()).ReturnsAsync(transactions);
            var result = await _service.GetAllTransactionsAsync();
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetAllTransactionsAsync_RepositoryThrows_PropagatesException()
        {
            _mockRepo.Setup(r => r.getAllTransactions()).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _service.GetAllTransactionsAsync());
        }

        [TestMethod]
        public async Task GetByFilterCriteriaAsync_HappyCase_ReturnsFilteredList()
        {
            var criteria = new TransactionFilterCriteria();
            var author = new User { CNP = "1234567890123", FirstName = "John", LastName = "Doe" };
            var transactions = new List<TransactionLogTransaction>
            {
                new() {
                    StockSymbol = "TEST",
                    StockName = "Test Stock",
                    Type = "BUY",
                    Amount = 10,
                    PricePerStock = 100,
                    Date = DateTime.Now,
                    AuthorCNP = "1234567890123",
                    Author = author
                }
            };

            _mockRepo.Setup(r => r.GetByFilterCriteriaAsync(criteria)).ReturnsAsync(transactions);
            var result = await _service.GetByFilterCriteriaAsync(criteria);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetByFilterCriteriaAsync_NullCriteria_RepositoryThrows_PropagatesException()
        {
            _mockRepo.Setup(r => r.GetByFilterCriteriaAsync(null)).ThrowsAsync(new ArgumentNullException());
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await _service.GetByFilterCriteriaAsync(null));
        }

        [TestMethod]
        public async Task GetByFilterCriteriaAsync_RepositoryThrows_PropagatesException()
        {
            var criteria = new TransactionFilterCriteria();
            _mockRepo.Setup(r => r.GetByFilterCriteriaAsync(criteria)).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _service.GetByFilterCriteriaAsync(criteria));
        }
    }
}

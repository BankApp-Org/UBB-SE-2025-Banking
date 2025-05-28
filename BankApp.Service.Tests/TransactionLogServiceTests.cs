using BankApi.Repositories;
using BankApi.Services;
using Common.Exceptions;
using Common.Models;
using Common.Models.Trading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace BankApp.Service.Tests
{
    [TestClass]
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class TransactionLogServiceTests
    {
        private Mock<ITransactionRepository> _mockRepo;
        private TransactionLogService _service;

        [TestInitialize]
        public void Init()
        {
            _mockRepo = new Mock<ITransactionRepository>();
            _service = new TransactionLogService(_mockRepo.Object);
        }

        [TestMethod]
        public async Task GetFilteredTransactions_HappyCase_ReturnsList()
        {
            var criteria = new StockTransactionFilterCriteria();
            var transactions = new List<StockTransactionTransaction> { new() { Amount = 1, PricePerStock = 1, StockName = "Test", Type = "BUY", Author = new()
            {
                CNP = "1234567890123",
                FirstName = "John",
                LastName = "Doe"
            }, Date = DateTime.Now, AuthorCNP = "1234567890123", StockSymbol = "TEST" } };

            _mockRepo.Setup(r => r.GetByFilterCriteriaAsync(criteria)).ReturnsAsync(transactions);
            var result = await _service.GetFilteredTransactions(criteria);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetFilteredTransactions_CriteriaInvalid_Throws()
        {
            // Use a real criteria instance with invalid values (MinTotalValue > MaxTotalValue)
            var invalidCriteria = new StockTransactionFilterCriteria
            {
                MinTotalValue = 10,
                MaxTotalValue = 5
            };
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _service.GetFilteredTransactions(invalidCriteria));
        }

        [TestMethod]
        public async Task GetFilteredTransactions_RepositoryThrows_Throws()
        {
            var criteria = new StockTransactionFilterCriteria();
            _mockRepo.Setup(r => r.GetByFilterCriteriaAsync(criteria)).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _service.GetFilteredTransactions(criteria));
        }

        [TestMethod]
        public void SortTransactions_HappyCase_SortsByDateAscending()
        {
            var author = new User { CNP = "1234567890123", FirstName = "John", LastName = "Doe" };
            var t1 = new StockTransactionTransaction
            {
                Date = DateTime.Now.AddDays(-1),
                StockSymbol = "TEST1",
                StockName = "Test Stock 1",
                Type = "BUY",
                Amount = 10,
                PricePerStock = 100,
                AuthorCNP = "1234567890123",
                Author = author
            };
            var t2 = new StockTransactionTransaction
            {
                Date = DateTime.Now,
                StockSymbol = "TEST2",
                StockName = "Test Stock 2",
                Type = "SELL",
                Amount = 5,
                PricePerStock = 200,
                AuthorCNP = "1234567890123",
                Author = author
            };
            var list = new List<StockTransactionTransaction> { t2, t1 };
            var sorted = _service.SortTransactions(list, "Date", true);
            Assert.AreEqual(t1, sorted[0]);
        }

        [TestMethod]
        public void SortTransactions_HappyCase_SortsByStockNameDescending()
        {
            var author = new User { CNP = "1234567890123", FirstName = "John", LastName = "Doe" };
            var t1 = new StockTransactionTransaction
            {
                StockName = "A",
                StockSymbol = "A",
                Type = "BUY",
                Amount = 10,
                PricePerStock = 100,
                Date = DateTime.Now,
                AuthorCNP = "1234567890123",
                Author = author
            };
            var t2 = new StockTransactionTransaction
            {
                StockName = "B",
                StockSymbol = "B",
                Type = "SELL",
                Amount = 5,
                PricePerStock = 200,
                Date = DateTime.Now,
                AuthorCNP = "1234567890123",
                Author = author
            };
            var list = new List<StockTransactionTransaction> { t1, t2 };
            var sorted = _service.SortTransactions(list, "Stock Name", false);
            Assert.AreEqual(t2, sorted[0]);
        }

        [TestMethod]
        public void SortTransactions_HappyCase_SortsByTotalValueAscending()
        {
            // TotalValue is read-only, so set Amount and PricePerStock
            var author = new User { CNP = "1234567890123", FirstName = "John", LastName = "Doe" };
            var t1 = new StockTransactionTransaction
            {
                Amount = 1,
                PricePerStock = 1,
                StockSymbol = "TEST1",
                StockName = "Test Stock 1",
                Type = "BUY",
                Date = DateTime.Now,
                AuthorCNP = "1234567890123",
                Author = author
            };
            var t2 = new StockTransactionTransaction
            {
                Amount = 1,
                PricePerStock = 2,
                StockSymbol = "TEST2",
                StockName = "Test Stock 2",
                Type = "SELL",
                Date = DateTime.Now,
                AuthorCNP = "1234567890123",
                Author = author
            };
            var list = new List<StockTransactionTransaction> { t2, t1 };
            var sorted = _service.SortTransactions(list, "Total Value", true);
            Assert.AreEqual(t1, sorted[0]);
        }

        [TestMethod]
        public void SortTransactions_InvalidSortType_Throws()
        {
            var list = new List<StockTransactionTransaction>();
            Assert.ThrowsExactly<InvalidSortTypeException>(() => _service.SortTransactions(list, "InvalidType"));
        }

        [TestMethod]
        public void ExportTransactions_UnsupportedFormat_Throws()
        {
            var transactions = new List<StockTransactionTransaction>();
            Assert.ThrowsExactly<ExportFormatNotSupportedException>(() => _service.ExportTransactions(transactions, "file.csv", "xml"));
        }

        [TestMethod]
        public void ExportTransactions_CsvFormat_CallsCsvExporter()
        {
            var author = new User { CNP = "1234567890123", FirstName = "John", LastName = "Doe" };
            var transactions = new List<StockTransactionTransaction> {
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
            var filePath = "file.csv";
            // Should not throw
            _service.ExportTransactions(transactions, filePath, "csv");
        }

        [TestMethod]
        public void ExportTransactions_JsonFormat_CallsJsonExporter()
        {
            var author = new User { CNP = "1234567890123", FirstName = "John", LastName = "Doe" };
            var transactions = new List<StockTransactionTransaction> {
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
            var filePath = "file.json";
            // Should not throw
            _service.ExportTransactions(transactions, filePath, "json");
        }

        [TestMethod]
        public void ExportTransactions_HtmlFormat_CallsHtmlExporter()
        {
            var author = new User { CNP = "1234567890123", FirstName = "John", LastName = "Doe" };
            var transactions = new List<StockTransactionTransaction> {
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
            var filePath = "file.html";
            // Should not throw
            _service.ExportTransactions(transactions, filePath, "html");
        }

        [TestMethod]
        public void ExportTransactions_NullTransactions_Throws()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => _service.ExportTransactions(null, "file.csv", "csv"));
        }

        [TestMethod]
        public void ExportTransactions_EmptyFilePath_Throws()
        {
            var transactions = new List<StockTransactionTransaction>();
            Assert.ThrowsExactly<ArgumentException>(() => _service.ExportTransactions(transactions, "", "csv"));
        }

        [TestMethod]
        public void ExportTransactions_EmptyFormat_Throws()
        {
            var transactions = new List<StockTransactionTransaction>();
            Assert.ThrowsExactly<ArgumentException>(() => _service.ExportTransactions(transactions, "file.csv", ""));
        }
    }
}

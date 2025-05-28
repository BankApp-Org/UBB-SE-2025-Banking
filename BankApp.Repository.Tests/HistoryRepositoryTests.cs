using BankApi.Data;
using BankApi.Repositories.Impl.Stocks;
using Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace BankApp.Repository.Tests
{
    [TestClass]
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class HistoryRepositoryTests
    {
        private ApiDbContext _context;
        private StockHistoryRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApiDbContext(options);

            // Seed some data
            _context.CreditScoreHistories.AddRange(
                new CreditScoreHistory { Id = 1, UserCnp = "123", Date = DateTime.Now.AddDays(-5), Score = 650 },
                new CreditScoreHistory { Id = 2, UserCnp = "123", Date = DateTime.Now.AddDays(-10), Score = 700 }
            );
            _context.SaveChanges();

            _repository = new StockHistoryRepository(_context);
        }

        [TestMethod]
        public async Task GetAllHistoryAsync_ReturnsOrderedList()
        {
            var result = await _repository.GetAllHistoryAsync();
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result[0].Date > result[1].Date);
        }

        [TestMethod]
        public async Task GetHistoryByIdAsync_ReturnsCorrectItem()
        {
            var result = await _repository.GetHistoryByIdAsync(1);
            Assert.IsNotNull(result);
            Assert.AreEqual(650, result.Score);
        }

        [TestMethod]
        public async Task AddHistoryAsync_ValidHistory_AddsCorrectly()
        {
            var newItem = new CreditScoreHistory { Id = 3, UserCnp = "456", Date = DateTime.Now, Score = 750 };
            var result = await _repository.AddHistoryAsync(newItem);

            var fromDb = await _context.CreditScoreHistories.FindAsync(3);
            Assert.IsNotNull(fromDb);
            Assert.AreEqual(750, fromDb.Score);
        }

        [TestMethod]
        public async Task AddHistoryAsync_Null_Throws()
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await _repository.AddHistoryAsync(null));
        }

        [TestMethod]
        public async Task AddHistoryAsync_InvalidScore_Throws()
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _repository.AddHistoryAsync(new CreditScoreHistory { Score = 2000 }));
        }

        [TestMethod]
        public async Task UpdateHistoryAsync_ValidUpdate_ChangesValues()
        {
            var update = new CreditScoreHistory { Id = 1, UserCnp = "999", Date = DateTime.Now, Score = 500 };
            var result = await _repository.UpdateHistoryAsync(update);
            Assert.AreEqual("999", result.UserCnp);
            Assert.AreEqual(500, result.Score);
        }

        [TestMethod]
        public async Task UpdateHistoryAsync_Null_Throws()
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await _repository.UpdateHistoryAsync(null));
        }

        [TestMethod]
        public async Task UpdateHistoryAsync_InvalidScore_Throws()
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _repository.UpdateHistoryAsync(new CreditScoreHistory { Id = 1, Score = -1 }));
        }

        [TestMethod]
        public async Task UpdateHistoryAsync_IdNotFound_Throws()
        {
            await Assert.ThrowsExactlyAsync<KeyNotFoundException>(async () => await _repository.UpdateHistoryAsync(new CreditScoreHistory { Id = 999, Score = 500 }));
        }

        [TestMethod]
        public async Task DeleteHistoryAsync_DeletesItem()
        {
            await _repository.DeleteHistoryAsync(1);
            var deleted = await _context.CreditScoreHistories.FindAsync(1);
            Assert.IsNull(deleted);
        }

        [TestMethod]
        public async Task DeleteHistoryAsync_IdNotFound_Throws()
        {
            await Assert.ThrowsExactlyAsync<KeyNotFoundException>(async () => await _repository.DeleteHistoryAsync(999));
        }

        [TestMethod]
        public async Task GetHistoryForUserAsync_ReturnsUserEntries()
        {
            var result = await _repository.GetHistoryForUserAsync("123");
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task GetHistoryForUserAsync_EmptyCnp_Throws()
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _repository.GetHistoryForUserAsync(" "));
        }

        [TestMethod]
        public async Task GetHistoryMonthlyAsync_ReturnsRecentEntries()
        {
            var result = await _repository.GetHistoryMonthlyAsync("123");
            Assert.IsTrue(result.All(h => h.Date >= DateTime.Now.AddMonths(-1)));
        }

        [TestMethod]
        public async Task GetHistoryMonthlyAsync_EmptyCnp_Throws()
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _repository.GetHistoryMonthlyAsync(""));
        }

        [TestMethod]
        public async Task GetHistoryYearlyAsync_ReturnsRecentYearEntries()
        {
            var result = await _repository.GetHistoryYearlyAsync("123");
            Assert.IsTrue(result.All(h => h.Date >= DateTime.Now.AddYears(-1)));
        }

        [TestMethod]
        public async Task GetHistoryYearlyAsync_EmptyCnp_Throws()
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _repository.GetHistoryYearlyAsync(null));
        }
    }
}

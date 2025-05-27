using BankApi.Data;
using BankApi.Repositories.Impl;
using Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace StockApp.Repository.Tests
{
    [TestClass]
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class InvestmentsRepositoryTests
    {
        private ApiDbContext _context;
        private InvestmentsRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApiDbContext(options);

            // Seed sample investments
            _context.Investments.AddRange(
                new Investment
                {
                    Id = 1,
                    InvestorCnp = "123",
                    AmountInvested = 1000,
                    AmountReturned = -1
                },
                new Investment
                {
                    Id = 2,
                    InvestorCnp = "456",
                    AmountInvested = 2000,
                    AmountReturned = 1500
                }
            );
            _context.SaveChanges();

            _repository = new InvestmentsRepository(_context);
        }

        [TestMethod]
        public async Task GetInvestmentsHistory_ReturnsAllInvestments()
        {
            var result = await _repository.GetInvestmentsHistory();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(i => i.InvestorCnp == "123"));
            Assert.IsTrue(result.Any(i => i.InvestorCnp == "456"));
        }

        [TestMethod]
        public async Task AddInvestment_AddsCorrectly()
        {
            var newInvestment = new Investment
            {
                InvestorCnp = "789",
                AmountInvested = 3000,
                AmountReturned = -1
            };

            await _repository.AddInvestment(newInvestment);

            var fromDb = await _context.Investments.FirstOrDefaultAsync(i => i.InvestorCnp == "789");
            Assert.IsNotNull(fromDb);
            Assert.AreEqual(3000, fromDb.AmountInvested);
        }

        [TestMethod]
        public async Task AddInvestment_Null_ThrowsException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await _repository.AddInvestment(null));
        }

        [TestMethod]
        public async Task UpdateInvestment_ValidUpdate_Succeeds()
        {
            await _repository.UpdateInvestment(1, "123", 1100);

            var updated = await _context.Investments.FindAsync(1);
            Assert.AreEqual(1100, updated.AmountReturned);
        }

        [TestMethod]
        public async Task UpdateInvestment_AlreadyReturned_ThrowsException()
        {
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _repository.UpdateInvestment(2, "456", 2000));
        }

        [TestMethod]
        public async Task UpdateInvestment_InvalidIdOrCNP_ThrowsException()
        {
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _repository.UpdateInvestment(999, "999", 1000));
        }
    }
}

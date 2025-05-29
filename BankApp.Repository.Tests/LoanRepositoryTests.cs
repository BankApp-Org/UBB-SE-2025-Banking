using BankApi.Data;
using BankApi.Repositories.Impl.Bank;
using Common.Models;
using Common.Models.Bank;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace BankApp.Repository.Tests
{
    [TestClass]
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class LoanRepositoryTests
    {
        private ApiDbContext _context;
        private LoanRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApiDbContext(options);

            // Seed test data
            _context.Loans.AddRange(
                new Loan { Id = 1, UserCnp = "123", LoanAmount = 1000, NumberOfMonths = 12, Status = "Pending" },
                new Loan { Id = 2, UserCnp = "456", LoanAmount = 2000, NumberOfMonths = 24, Status = "Pending" }
            );
            _context.CreditScoreHistories.Add(
                new CreditScoreHistory { Id = 1, UserCnp = "123", Score = 650, Date = DateTime.UtcNow.AddDays(-1).Date }
            );

            _context.SaveChanges();

            _repository = new LoanRepository(_context);
        }

        [TestMethod]
        public async Task GetLoansAsync_ReturnsAllLoans()
        {
            var result = await _repository.GetLoansAsync();
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task GetUserLoansAsync_ReturnsCorrectLoans()
        {
            var result = await _repository.GetUserLoansAsync("123");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1000, result[0].LoanAmount);
        }

        [TestMethod]
        public async Task AddLoanAsync_AddsLoan()
        {
            var loan = new Loan { UserCnp = "789", LoanAmount = 3000, NumberOfMonths = 36, Status = "Pending" };
            await _repository.AddLoanAsync(loan);

            var fromDb = await _context.Loans.FirstOrDefaultAsync(l => l.UserCnp == "789");
            Assert.IsNotNull(fromDb);
            Assert.AreEqual(3000, fromDb.LoanAmount);
        }

        [TestMethod]
        public async Task AddLoanAsync_Null_ThrowsException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await _repository.AddLoanAsync(null));
        }

        [TestMethod]
        public async Task UpdateLoanAsync_UpdatesSuccessfully()
        {
            var loan = await _context.Loans.FindAsync(1);
            loan.LoanAmount = 1500;

            await _repository.UpdateLoanAsync(loan);

            var updated = await _context.Loans.FindAsync(1);
            Assert.AreEqual(1500, updated.LoanAmount);
        }

        [TestMethod]
        public async Task UpdateLoanAsync_Null_ThrowsException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await _repository.UpdateLoanAsync(null));
        }

        [TestMethod]
        public async Task DeleteLoanAsync_DeletesLoan()
        {
            await _repository.DeleteLoanAsync(2);

            var deleted = await _context.Loans.FindAsync(2);
            Assert.IsNull(deleted);
        }

        [TestMethod]
        public async Task DeleteLoanAsync_InvalidId_Throws()
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _repository.DeleteLoanAsync(0));
        }

        [TestMethod]
        public async Task DeleteLoanAsync_NotFound_Throws()
        {
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _repository.DeleteLoanAsync(999));
        }

        [TestMethod]
        public async Task GetLoanByIdAsync_ReturnsCorrectLoan()
        {
            var loan = await _repository.GetLoanByIdAsync(1);
            Assert.IsNotNull(loan);
            Assert.AreEqual("123", loan.UserCnp);
        }

        [TestMethod]
        public async Task GetLoanByIdAsync_InvalidId_Throws()
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _repository.GetLoanByIdAsync(0));
        }

        [TestMethod]
        public async Task GetLoanByIdAsync_NotFound_Throws()
        {
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _repository.GetLoanByIdAsync(999));
        }

        [TestMethod]
        public async Task UpdateCreditScoreHistoryForUserAsync_AddsNewEntry()
        {
            string userCnp = "789";
            int newScore = 720;

            await _repository.UpdateCreditScoreHistoryForUserAsync(userCnp, newScore);

            var entry = await _context.CreditScoreHistories.FirstOrDefaultAsync(
                c => c.UserCnp == userCnp && c.Date == DateTime.UtcNow.Date);

            Assert.IsNotNull(entry);
            Assert.AreEqual(720, entry.Score);
        }

        [TestMethod]
        public async Task UpdateCreditScoreHistoryForUserAsync_UpdatesExistingEntry()
        {
            string userCnp = "123";
            int newScore = 780;

            _context.CreditScoreHistories.Add(new CreditScoreHistory
            {
                Id = 2,
                UserCnp = userCnp,
                Date = DateTime.UtcNow.Date,
                Score = 700
            });
            await _context.SaveChangesAsync();

            await _repository.UpdateCreditScoreHistoryForUserAsync(userCnp, newScore);

            var updated = await _context.CreditScoreHistories
                .FirstOrDefaultAsync(c => c.UserCnp == userCnp && c.Date == DateTime.UtcNow.Date);

            Assert.AreEqual(780, updated.Score);
        }

        [TestMethod]
        public async Task UpdateCreditScoreHistoryForUserAsync_EmptyCnp_Throws()
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _repository.UpdateCreditScoreHistoryForUserAsync("", 700));
        }
    }
}

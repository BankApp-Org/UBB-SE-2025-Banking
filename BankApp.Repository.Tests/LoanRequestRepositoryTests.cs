using BankApi.Data;
using BankApi.Repositories.Bank;
using BankApi.Repositories.Impl.Bank;
using Common.Models.Bank;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace BankApp.Repository.Tests
{
    [TestClass]
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class LoanRequestRepositoryTests
    {
        private ApiDbContext _context;
        private ILoanRequestRepository _repository;
        private Mock<ILogger<LoanRequestRepository>> _mockLogger;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApiDbContext(options);
            _mockLogger = new Mock<ILogger<LoanRequestRepository>>();
            _repository = new LoanRequestRepository(_context, _mockLogger.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        

        
        
        [TestMethod]
        public async Task SolveLoanRequestAsync_WithInvalidId_ShouldThrowException()
        {
            // Act
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _repository.SolveLoanRequestAsync(0));
        }

        [TestMethod]
        public async Task SolveLoanRequestAsync_WithNonExistentId_ShouldThrowInvalidOperationException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await _repository.SolveLoanRequestAsync(999));
        }

        
        [TestMethod]
        public async Task DeleteLoanRequestAsync_WithInvalidId_ShouldThrowException()
        {
            // Act
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _repository.DeleteLoanRequestAsync(0));
        }

        [TestMethod]
        public async Task DeleteLoanRequestAsync_WithNonExistentId_ShouldThrowException()
        {
            // Act
            await Assert.ThrowsExactlyAsync<KeyNotFoundException>(async () => await _repository.DeleteLoanRequestAsync(999));
        }
    }
}
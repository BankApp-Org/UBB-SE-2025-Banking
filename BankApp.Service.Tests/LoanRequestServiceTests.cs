using BankApi.Repositories;
using BankApi.Repositories.Bank;
using BankApi.Services.Bank;
using Common.Models;
using Common.Models.Bank;
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
    public class LoanRequestServiceTests
    {
        private Mock<ILoanRequestRepository> _mockLoanRequestRepository;
        private Mock<IUserRepository> _mockUserRepository;
        private LoanRequestService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockLoanRequestRepository = new Mock<ILoanRequestRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _service = new LoanRequestService(_mockLoanRequestRepository.Object, _mockUserRepository.Object);
        }

        private LoanRequest CreateValidLoanRequest(int id = 1, string cnp = "1234567890123", decimal amount = 5000)
        {
            var loan = new Loan
            {
                UserCnp = cnp,
                LoanAmount = amount,
                ApplicationDate = DateTime.UtcNow,
                RepaymentDate = DateTime.UtcNow.AddMonths(12),
                InterestRate = 5.0m,
                NumberOfMonths = 12,
                MonthlyPaymentAmount = amount / 12,
                Status = "Pending",
                MonthlyPaymentsCompleted = 0,
                RepaidAmount = 0,
                Penalty = 0
            };

            var loanRequest = new LoanRequest
            {
                Id = id,
                UserCnp = cnp,
                Status = "Pending",
                Loan = loan
            };

            // Establish the circular reference
            loan.LoanRequest = loanRequest;

            return loanRequest;
        }

        [TestMethod]
        public async Task GiveSuggestion_ReturnsEmptyString_WhenUserQualifies()
        {
            // Arrange
            var loanRequest = CreateValidLoanRequest(1, "123", 5000);
            var user = new User { CNP = "123", Income = 10000, CreditScore = 400, RiskScore = 50 };

            _mockUserRepository.Setup(x => x.GetByCnpAsync("123")).ReturnsAsync(user);

            // Act
            var result = await _service.GiveSuggestion(loanRequest);

            // Assert
            Assert.AreEqual(string.Empty, result);
            _mockUserRepository.Verify(x => x.GetByCnpAsync("123"), Times.Once);
        }

        [TestMethod]
        public async Task GiveSuggestion_ReturnsAmountTooHighMessage_WhenAmountExceedsIncome()
        {
            // Arrange
            var loanRequest = CreateValidLoanRequest(1, "123", 110000);
            var user = new User { CNP = "123", Income = 10000, CreditScore = 400, RiskScore = 50 };

            _mockUserRepository.Setup(x => x.GetByCnpAsync("123")).ReturnsAsync(user);

            // Act
            var result = await _service.GiveSuggestion(loanRequest);

            // Assert
            Assert.AreEqual("User does not qualify for loan: Amount requested is too high for user income", result);
        }

        [TestMethod]
        public async Task GiveSuggestion_ReturnsCreditScoreMessage_WhenCreditScoreLow()
        {
            // Arrange
            var loanRequest = CreateValidLoanRequest(1, "123", 5000);
            var user = new User { CNP = "123", Income = 10000, CreditScore = 250, RiskScore = 50 };

            _mockUserRepository.Setup(x => x.GetByCnpAsync("123")).ReturnsAsync(user);

            // Act
            var result = await _service.GiveSuggestion(loanRequest);

            // Assert
            Assert.AreEqual("User does not qualify for loan: Credit score is too low", result);
        }

        [TestMethod]
        public async Task GiveSuggestion_ReturnsRiskScoreMessage_WhenRiskScoreHigh()
        {
            // Arrange
            var loanRequest = CreateValidLoanRequest(1, "123", 5000);
            var user = new User { CNP = "123", Income = 10000, CreditScore = 400, RiskScore = 80 };

            _mockUserRepository.Setup(x => x.GetByCnpAsync("123")).ReturnsAsync(user);

            // Act
            var result = await _service.GiveSuggestion(loanRequest);

            // Assert
            Assert.AreEqual("User does not qualify for loan: User risk score is too high", result);
        }

        [TestMethod]
        public async Task GiveSuggestion_ReturnsCombinedMessages_WhenMultipleIssues()
        {
            // Arrange
            var loanRequest = CreateValidLoanRequest(1, "123", 110000);
            var user = new User { CNP = "123", Income = 10000, CreditScore = 250, RiskScore = 80 };

            _mockUserRepository.Setup(x => x.GetByCnpAsync("123")).ReturnsAsync(user);

            // Act
            var result = await _service.GiveSuggestion(loanRequest);

            // Assert
            Assert.AreEqual(
                "User does not qualify for loan: Amount requested is too high for user income, Credit score is too low, User risk score is too high",
                result);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GiveSuggestion_ThrowsException_WhenUserNotFound()
        {
            // Arrange
            var loanRequest = CreateValidLoanRequest(1, "123", 5000);
            _mockUserRepository.Setup(x => x.GetByCnpAsync("123")).ReturnsAsync((User)null);

            // Act
            await _service.GiveSuggestion(loanRequest);
        }

        [TestMethod]
        public async Task SolveLoanRequest_CallsRepository_WhenValidId()
        {
            // Arrange
            _mockLoanRequestRepository.Setup(x => x.SolveLoanRequestAsync(1)).Returns(Task.CompletedTask);

            // Act
            await _service.SolveLoanRequest(1);

            // Assert
            _mockLoanRequestRepository.Verify(x => x.SolveLoanRequestAsync(1), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task SolveLoanRequest_ThrowsException_WhenRepositoryFails()
        {
            // Arrange
            _mockLoanRequestRepository.Setup(x => x.SolveLoanRequestAsync(1))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            await _service.SolveLoanRequest(1);
        }

        [TestMethod]
        public async Task DeleteLoanRequest_CallsRepository_WhenValidId()
        {
            // Arrange
            _mockLoanRequestRepository.Setup(x => x.DeleteLoanRequestAsync(1)).Returns(Task.CompletedTask);

            // Act
            await _service.DeleteLoanRequest(1);

            // Assert
            _mockLoanRequestRepository.Verify(x => x.DeleteLoanRequestAsync(1), Times.Once);
        }

        [TestMethod]
        public async Task GetLoanRequests_ReturnsList_WhenRepositorySucceeds()
        {
            // Arrange
            var loan1 = new Loan
            {
                LoanAmount = 5000,
                ApplicationDate = DateTime.UtcNow,
                RepaymentDate = DateTime.UtcNow.AddMonths(12),
                Status = "Pending",
                InterestRate = 5.0m,
                NumberOfMonths = 12,
                MonthlyPaymentAmount = 5000 / 12,
                MonthlyPaymentsCompleted = 0,
                RepaidAmount = 0,
                Penalty = 0,
                UserCnp = "123"
            };

            var loan2 = new Loan
            {
                LoanAmount = 7000,
                ApplicationDate = DateTime.UtcNow,
                RepaymentDate = DateTime.UtcNow.AddMonths(12),
                Status = "Pending",
                InterestRate = 5.0m,
                NumberOfMonths = 12,
                MonthlyPaymentAmount = 7000 / 12,
                MonthlyPaymentsCompleted = 0,
                RepaidAmount = 0,
                Penalty = 0,
                UserCnp = "456"
            };

            var request1 = new LoanRequest { Id = 1, UserCnp = "123", Status = "Pending", Loan = loan1 };
            var request2 = new LoanRequest { Id = 2, UserCnp = "456", Status = "Pending", Loan = loan2 };

            // Complete the circular reference
            loan1.LoanRequest = request1;
            loan2.LoanRequest = request2;

            var expectedRequests = new List<LoanRequest> { request1, request2 };
            _mockLoanRequestRepository.Setup(x => x.GetLoanRequestsAsync()).ReturnsAsync(expectedRequests);

            // Act
            var result = await _service.GetLoanRequests();

            // Assert
            CollectionAssert.AreEqual(expectedRequests, result);
            _mockLoanRequestRepository.Verify(x => x.GetLoanRequestsAsync(), Times.Once);
        }
    }
}
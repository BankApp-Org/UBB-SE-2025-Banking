using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using BankApi.Services.Bank;
using BankApi.Repositories.Bank;
using BankApi.Repositories;
using Common.Models.Bank;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.Versioning;
using BankApi.Repositories.Impl.Bank;
using Common.Services.Bank;
using BankApi.Repositories.Trading;

namespace BankApp.Service.Tests
{
    [TestClass]
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class LoanServiceTests
    {
        private Mock<ILoanRepository> _loanRepoMock;
        private Mock<IUserRepository> _userRepoMock;
        private Mock<ICreditHistoryService> _creditHistoryServiceMock;
        private Mock<ICreditScoringService> _crediScoringServiceMock;
        private Mock<IBankTransactionRepository> _bankTransactionRepoMock;
        private Mock<IBankAccountService> _bankAccountServiceMock;
        private Mock<IStockTransactionRepository> _stockTransactionRepoMock;
        private LoanService _service;

        [TestInitialize]
        public void Setup()
        {
            _loanRepoMock = new Mock<ILoanRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _bankTransactionRepoMock = new Mock<IBankTransactionRepository>();
            _stockTransactionRepoMock = new Mock<IStockTransactionRepository>();
            _bankAccountServiceMock = new Mock<IBankAccountService>();
            _creditHistoryServiceMock = new Mock<ICreditHistoryService>();

            _crediScoringServiceMock = new Mock<ICreditScoringService>();
            _service = new LoanService(_loanRepoMock.Object, _userRepoMock.Object, _crediScoringServiceMock.Object, _bankAccountServiceMock.Object);
        }

        private User CreateValidUser(string cnp = "CNP", int id = 1)
        {
            return new User { Id = id, CNP = cnp, FirstName = "Test", LastName = "User", CreditScore = 500, RiskScore = 50 };
        }
        private Loan CreateValidLoan()
        {
            return new Loan { Id = 1, UserCnp = "CNP", LoanAmount = 1000, ApplicationDate = DateTime.Today, RepaymentDate = DateTime.Today.AddMonths(10), InterestRate = 5, NumberOfMonths = 10, MonthlyPaymentAmount = 110, MonthlyPaymentsCompleted = 0, Penalty = 0, Status = "active", RepaidAmount = 0, TaxPercentage = 0, DeadlineDate = DateTime.Today.AddMonths(10), LoanRequest = null! };
        }
        private LoanRequest CreateValidLoanRequest()
        {
            return new LoanRequest { UserCnp = "CNP", Loan = CreateValidLoan(), Status = "pending", AccountIban = "RO12BANK12345678901234567" };
        }

        [TestMethod]
        public async Task GetLoansAsync_ReturnsLoans()
        {
            var loans = new List<Loan> { CreateValidLoan() };
            _loanRepoMock.Setup(r => r.GetLoansAsync()).ReturnsAsync(loans);
            var result = await _service.GetLoansAsync();
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetUserLoansAsync_ReturnsUserLoans()
        {
            var userCnp = "CNP";
            var loans = new List<Loan> { CreateValidLoan() };
            _loanRepoMock.Setup(r => r.GetUserLoansAsync(userCnp)).ReturnsAsync(loans);
            var result = await _service.GetUserLoansAsync(userCnp);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(userCnp, result[0].UserCnp);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task AddLoanAsync_UserNotFound_Throws()
        {
            var loanRequest = CreateValidLoanRequest();
            _userRepoMock.Setup(r => r.GetByCnpAsync(loanRequest.UserCnp)).ReturnsAsync((User)null);
            await _service.AddLoanAsync(loanRequest);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddLoanAsync_LoanMissing_Throws()
        {
            var loanRequest = new LoanRequest { UserCnp = "CNP", Loan = null!, Status = "pending", AccountIban = "RO12BANK12345678901234567" };
            _userRepoMock.Setup(r => r.GetByCnpAsync(loanRequest.UserCnp)).ReturnsAsync(CreateValidUser());
            await _service.AddLoanAsync(loanRequest);
        }

        [TestMethod]
        public async Task AddLoanAsync_ValidRequest_CalculatesAndAddsLoan()
        {
            var user = CreateValidUser();
            var loanRequest = CreateValidLoanRequest();
            _userRepoMock.Setup(r => r.GetByCnpAsync(loanRequest.UserCnp)).ReturnsAsync(user);
            _loanRepoMock.Setup(r => r.AddLoanAsync(It.IsAny<Loan>())).Returns(Task.CompletedTask);
            await _service.AddLoanAsync(loanRequest);
            _loanRepoMock.Verify(r => r.AddLoanAsync(It.Is<Loan>(l => l.InterestRate > 0 && l.NumberOfMonths > 0 && l.MonthlyPaymentAmount > 0)), Times.Once);
        }
    }
}
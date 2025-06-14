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

namespace BankApp.Service.Tests
{
    [TestClass]
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class LoanServiceTests
    {
        private Mock<ILoanRepository> _loanRepoMock;
        private Mock<IUserRepository> _userRepoMock;
        private Mock<CreditScoringService> _crediScoringServiceMock;
        private Mock<IBankAccountService> _bankAccountServiceMock;
        private LoanService _service;

        [TestInitialize]
        public void Setup()
        {
            _loanRepoMock = new Mock<ILoanRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _crediScoringServiceMock = new Mock<CreditScoringService>();
            _bankAccountServiceMock = new Mock<IBankAccountService>();
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
    }
}
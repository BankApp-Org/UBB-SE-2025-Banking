using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using BankApi.Services.Bank;
using BankApi.Repositories.Impl.Bank;
using Common.Models.Bank;
using Common.Services.Bank;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Common.Models;

namespace BankApp.Service.Tests
{
    [TestClass]
    public class BankAccountServiceTests
    {
        private Mock<IBankAccountRepository> _bankAccountRepoMock;
        private Mock<ICurrencyExchangeRepository> _currencyExchangeRepoMock;
        private BankAccountService _service;

        [TestInitialize]
        public void Setup()
        {
            _bankAccountRepoMock = new Mock<IBankAccountRepository>();
            _currencyExchangeRepoMock = new Mock<ICurrencyExchangeRepository>();
            _service = new BankAccountService(_bankAccountRepoMock.Object, _currencyExchangeRepoMock.Object);
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

        [TestMethod]
        public async Task GetUserBankAccounts_ValidUserId_ReturnsAccounts()
        {
            // Arrange
            int userId = 1;
            var accounts = new List<BankAccount> { CreateValidBankAccount(userId: userId) };
            _bankAccountRepoMock.Setup(r => r.GetBankAccountsByUserIdAsync(userId)).ReturnsAsync(accounts);

            // Act
            var result = await _service.GetUserBankAccounts(userId);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("IBAN1", result[0].Iban);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetUserBankAccounts_InvalidUserId_Throws()
        {
            await _service.GetUserBankAccounts(0);
        }

        [TestMethod]
        public async Task FindBankAccount_ValidIban_ReturnsAccount()
        {
            var iban = "IBAN1";
            var account = CreateValidBankAccount(iban);
            _bankAccountRepoMock.Setup(r => r.GetBankAccountBalanceByIbanAsync(iban)).ReturnsAsync(account);
            var result = await _service.FindBankAccount(iban);
            Assert.AreEqual(iban, result.Iban);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task FindBankAccount_NullIban_Throws()
        {
            await _service.FindBankAccount(null);
        }

        [TestMethod]
        public async Task CreateBankAccount_ValidAccount_ReturnsTrue()
        {
            var account = CreateValidBankAccount();
            _bankAccountRepoMock.Setup(r => r.CreateBankAccountAsync(account)).ReturnsAsync(account);
            var result = await _service.CreateBankAccount(account);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateBankAccount_NullAccount_Throws()
        {
            await _service.CreateBankAccount(null);
        }

        [TestMethod]
        public async Task RemoveBankAccount_ValidIban_ReturnsTrue()
        {
            var iban = "IBAN1";
            _bankAccountRepoMock.Setup(r => r.DeleteBankAccountAsync(iban)).ReturnsAsync(true);
            var result = await _service.RemoveBankAccount(iban);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RemoveBankAccount_NullIban_Throws()
        {
            await _service.RemoveBankAccount(null);
        }

        [TestMethod]
        public async Task CheckIBANExists_AccountExists_ReturnsTrue()
        {
            var iban = "IBAN1";
            _bankAccountRepoMock.Setup(r => r.GetBankAccountBalanceByIbanAsync(iban)).ReturnsAsync(CreateValidBankAccount(iban));
            var result = await _service.CheckIBANExists(iban);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CheckIBANExists_AccountDoesNotExist_ReturnsFalse()
        {
            var iban = "IBAN2";
            _bankAccountRepoMock.Setup(r => r.GetBankAccountBalanceByIbanAsync(iban)).ThrowsAsync(new Exception());
            var result = await _service.CheckIBANExists(iban);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task UpdateBankAccount_ValidAccount_ReturnsTrue()
        {
            var account = CreateValidBankAccount();
            _bankAccountRepoMock.Setup(r => r.UpdateBankAccountAsync(account)).ReturnsAsync(true);
            var result = await _service.UpdateBankAccount(account);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateBankAccount_NullAccount_Throws()
        {
            await _service.UpdateBankAccount(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdateBankAccount_EmptyIban_Throws()
        {
            var account = CreateValidBankAccount(iban: "");
            await _service.UpdateBankAccount(account);
        }
    }
} 
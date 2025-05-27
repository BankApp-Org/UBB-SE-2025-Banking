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
    public class UserServiceTests
    {
        private Mock<IUserRepository> _mockRepo;
        private UserService _service;

        [TestInitialize]
        public void Init()
        {
            _mockRepo = new Mock<IUserRepository>();
            _service = new UserService(_mockRepo.Object);
        }

        [TestMethod]
        public async Task GetUserByCnpAsync_HappyCase_ReturnsUser()
        {
            var user = new User { CNP = "123", UserName = "Test" };
            _mockRepo.Setup(r => r.GetByCnpAsync("123")).ReturnsAsync(user);
            var result = await _service.GetUserByCnpAsync("123");
            Assert.IsNotNull(result);
            Assert.AreEqual("123", result.CNP);
        }

        [TestMethod]
        public async Task GetUserByCnpAsync_EmptyCnp_ThrowsArgumentException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _service.GetUserByCnpAsync(""));
        }

        [TestMethod]
        public async Task GetUserByCnpAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            _mockRepo.Setup(r => r.GetByCnpAsync("123")).ReturnsAsync((User)null);
            await Assert.ThrowsExactlyAsync<KeyNotFoundException>(async () => await _service.GetUserByCnpAsync("123"));
        }

        [TestMethod]
        public async Task GetUsers_HappyCase_ReturnsList()
        {
            var users = new List<User> { new() { CNP = "123" } };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(users);
            var result = await _service.GetUsers();
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task CreateUser_HappyCase_CreatesUser()
        {
            var user = new User { CNP = "123" };
            _mockRepo.Setup(r => r.CreateAsync(user)).Returns(Task.FromResult(user));
            await _service.CreateUser(user);
            _mockRepo.Verify(r => r.CreateAsync(user), Times.Once);
        }

        [TestMethod]
        public async Task CreateUser_NullUser_ThrowsArgumentNullException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await _service.CreateUser(null));
        }

        [TestMethod]
        public async Task UpdateUserAsync_HappyCase_UpdatesUser()
        {
            var existingUser = new User { CNP = "123", UserName = "Old", Image = "old.png", Description = "old", IsHidden = false };
            _mockRepo.Setup(r => r.GetByCnpAsync("123")).ReturnsAsync(existingUser);
            _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.FromResult(true));
        
            var updatedUser = new User
            {
                UserName = "New",
                Image = "new.png",
                Description = "new desc",
                IsHidden = true,
                Email = "test@example.com",
                PhoneNumber = "1234567890"
            };
        
            await _service.UpdateUserAsync(updatedUser, "123");
        
            _mockRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => 
                u.UserName == "New" && 
                u.Image == "new.png" && 
                u.Description == "new desc" && 
                u.IsHidden == true)), Times.Once);
        }

        [TestMethod]
        public async Task UpdateUserAsync_NullUser_ThrowsArgumentNullException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await _service.UpdateUserAsync(null, "123"));
        }

        [TestMethod]
        public async Task UpdateUserAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            var updatedUser = new User
            {
                UserName = "New",
                Image = "new.png",
                Description = "new desc",
                IsHidden = true
            };
        
            _mockRepo.Setup(r => r.GetByCnpAsync("123")).ReturnsAsync((User)null);
            await Assert.ThrowsExactlyAsync<KeyNotFoundException>(async () => await _service.UpdateUserAsync(updatedUser, "123"));
        }

        [TestMethod]
        public async Task UpdateIsAdminAsync_HappyCase_UpdatesRoles()
        {
            var user = new User { CNP = "123" };
            _mockRepo.Setup(r => r.GetByCnpAsync("123")).ReturnsAsync(user);
            _mockRepo.Setup(r => r.UpdateRolesAsync(user, It.IsAny<IList<string>>())).Returns(Task.FromResult(true));
            await _service.UpdateIsAdminAsync(true, "123");
            _mockRepo.Verify(r => r.UpdateRolesAsync(user, It.Is<IList<string>>(l => l.Contains("Admin"))), Times.Once);
        }

        [TestMethod]
        public async Task UpdateIsAdminAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            _mockRepo.Setup(r => r.GetByCnpAsync("123")).ReturnsAsync((User)null);
            await Assert.ThrowsExactlyAsync<KeyNotFoundException>(async () => await _service.UpdateIsAdminAsync(true, "123"));
        }

        [TestMethod]
        public async Task GetCurrentUserAsync_HappyCase_ReturnsUser()
        {
            var user = new User { CNP = "123" };
            _mockRepo.Setup(r => r.GetByCnpAsync("123")).ReturnsAsync(user);
            var result = await _service.GetCurrentUserAsync("123");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetCurrentUserAsync_EmptyCnp_ThrowsArgumentException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _service.GetCurrentUserAsync(""));
        }

        [TestMethod]
        public async Task GetCurrentUserAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            _mockRepo.Setup(r => r.GetByCnpAsync("123")).ReturnsAsync((User)null);
            await Assert.ThrowsExactlyAsync<KeyNotFoundException>(async () => await _service.GetCurrentUserAsync("123"));
        }

        [TestMethod]
        public async Task GetCurrentUserGemsAsync_HappyCase_ReturnsGems()
        {
            var user = new User { CNP = "123", GemBalance = 42 };
            _mockRepo.Setup(r => r.GetByCnpAsync("123")).ReturnsAsync(user);
            var result = await _service.GetCurrentUserGemsAsync("123");
            Assert.AreEqual(42, result);
        }

        [TestMethod]
        public async Task GetCurrentUserGemsAsync_EmptyCnp_ThrowsArgumentException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _service.GetCurrentUserGemsAsync(""));
        }

        [TestMethod]
        public async Task GetCurrentUserGemsAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            _mockRepo.Setup(r => r.GetByCnpAsync("123")).ReturnsAsync((User)null);
            await Assert.ThrowsExactlyAsync<KeyNotFoundException>(async () => await _service.GetCurrentUserGemsAsync("123"));
        }

        [TestMethod]
        public async Task AddDefaultRoleToAllUsersAsync_HappyCase_ReturnsCount()
        {
            _mockRepo.Setup(r => r.AddDefaultRoleToAllUsersAsync()).ReturnsAsync(5);
            var result = await _service.AddDefaultRoleToAllUsersAsync();
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public async Task AddDefaultRoleToAllUsersAsync_RepositoryThrows_ThrowsException()
        {
            _mockRepo.Setup(r => r.AddDefaultRoleToAllUsersAsync()).ThrowsAsync(new Exception());
            await Assert.ThrowsExactlyAsync<Exception>(async () => await _service.AddDefaultRoleToAllUsersAsync());
        }
    }
}
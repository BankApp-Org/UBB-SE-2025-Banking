using BankApi.Data;
using BankApi.Repositories.Impl;
using Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Xunit;

namespace BankApp.Repository.Tests
{
    [SupportedOSPlatform("windows10.0.26100.0")]

    public class UserRepositoryTests
    {
        private static ApiDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApiDbContext(options);
        }

        private static User CreateTestUser(int id = 1) => new()
        {
            Id = id,
            UserName = "testuser",
            CNP = "1234567890",
            Email = "test@example.com",
            PasswordHash = "password"
        };

        private static UserRepository GetRepository(ApiDbContext context, Mock<UserManager<User>> userManagerMock, Mock<RoleManager<IdentityRole<int>>> roleManagerMock)
        {
            var logger = Mock.Of<ILogger<UserRepository>>();
            return new UserRepository(context, logger, userManagerMock.Object, roleManagerMock.Object);
        }

        private static Mock<UserManager<User>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        }

        private static Mock<RoleManager<IdentityRole<int>>> GetMockRoleManager()
        {
            var store = new Mock<IRoleStore<IdentityRole<int>>>();
            return new Mock<RoleManager<IdentityRole<int>>>(store.Object, null, null, null, null);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllUsers()
        {
            using var context = GetInMemoryContext();
            context.Users.Add(CreateTestUser());
            await context.SaveChangesAsync();

            var repo = GetRepository(context, GetMockUserManager(), GetMockRoleManager());

            var users = await repo.GetAllAsync();

            Assert.Single(users);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectUser()
        {
            using var context = GetInMemoryContext();
            var user = CreateTestUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repo = GetRepository(context, GetMockUserManager(), GetMockRoleManager());

            var result = await repo.GetByIdAsync(user.Id);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
        }

        [Fact]
        public async Task GetByCnpAsync_ReturnsCorrectUser()
        {
            using var context = GetInMemoryContext();
            var user = CreateTestUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repo = GetRepository(context, GetMockUserManager(), GetMockRoleManager());

            var result = await repo.GetByCnpAsync(user.CNP);

            Assert.NotNull(result);
            Assert.Equal(user.CNP, result.CNP);
        }

        [Fact]
        public async Task GetByUsernameAsync_ReturnsCorrectUser()
        {
            using var context = GetInMemoryContext();
            var user = CreateTestUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repo = GetRepository(context, GetMockUserManager(), GetMockRoleManager());

            var result = await repo.GetByUsernameAsync(user.UserName);

            Assert.NotNull(result);
            Assert.Equal(user.UserName, result.UserName);
        }

        [Fact]
        public async Task CreateAsync_Succeeds_WhenValid()
        {
            using var context = GetInMemoryContext();
            var user = CreateTestUser();

            var userManagerMock = GetMockUserManager();
            userManagerMock.Setup(m => m.FindByNameAsync(user.UserName)).ReturnsAsync((User)null);
            userManagerMock.Setup(m => m.CreateAsync(user, user.PasswordHash)).ReturnsAsync(IdentityResult.Success);

            var repo = GetRepository(context, userManagerMock, GetMockRoleManager());

            var result = await repo.CreateAsync(user);

            Assert.NotNull(result);
            Assert.Equal(user.UserName, result.UserName);
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenDuplicate()
        {
            using var context = GetInMemoryContext();
            var user = CreateTestUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userManagerMock = GetMockUserManager();
            userManagerMock.Setup(m => m.FindByNameAsync(user.UserName)).ReturnsAsync(user);

            var repo = GetRepository(context, userManagerMock, GetMockRoleManager());

            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.CreateAsync(user));
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenIdentityCreateFails()
        {
            using var context = GetInMemoryContext();
            var user = CreateTestUser();

            var userManagerMock = GetMockUserManager();
            userManagerMock.Setup(m => m.FindByNameAsync(user.UserName)).ReturnsAsync((User)null);
            userManagerMock.Setup(m => m.CreateAsync(user, user.PasswordHash))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

            var repo = GetRepository(context, userManagerMock, GetMockRoleManager());

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => repo.CreateAsync(user));
            Assert.Contains("Password too weak", ex.Message);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsTrue_WhenUpdated()
        {
            using var context = GetInMemoryContext();
            var user = CreateTestUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            user.Email = "updated@example.com";
            var repo = GetRepository(context, GetMockUserManager(), GetMockRoleManager());

            var result = await repo.UpdateAsync(user);

            Assert.True(result);
        }

        [Fact]
        public async Task UpdateRolesAsync_Throws_WhenUserIsNull()
        {
            var repo = GetRepository(GetInMemoryContext(), GetMockUserManager(), GetMockRoleManager());
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateRolesAsync(null, ["Admin"]));
        }

        [Fact]
        public async Task UpdateRolesAsync_Throws_WhenRolesAreNull()
        {
            var repo = GetRepository(GetInMemoryContext(), GetMockUserManager(), GetMockRoleManager());
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateRolesAsync(CreateTestUser(), null));
        }

        [Fact]
        public async Task UpdateRolesAsync_ReturnsFalse_WhenRemoveFails()
        {
            var user = CreateTestUser();
            var userManagerMock = GetMockUserManager();
            userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["User"]);
            userManagerMock.Setup(m => m.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Failed());

            var repo = GetRepository(GetInMemoryContext(), userManagerMock, GetMockRoleManager());

            var result = await repo.UpdateRolesAsync(user, ["Admin"]);
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateRolesAsync_ReturnsFalse_WhenAddFails()
        {
            var user = CreateTestUser();
            var userManagerMock = GetMockUserManager();
            userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync([]);
            userManagerMock.Setup(m => m.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Failed());

            var repo = GetRepository(GetInMemoryContext(), userManagerMock, GetMockRoleManager());

            var result = await repo.UpdateRolesAsync(user, ["Admin"]);
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_RemovesUser()
        {
            // Arrange
            var context = GetInMemoryContext();
            var user = CreateTestUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            mockUserManager
                .Setup(m => m.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            mockUserManager
                .Setup(m => m.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            var mockRoleManager = GetMockRoleManager();
            var logger = Mock.Of<ILogger<UserRepository>>();
            var repo = new UserRepository(context, logger, mockUserManager.Object, mockRoleManager.Object);

            // Act
            var result = await repo.DeleteAsync(user.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AddDefaultRoleToAllUsersAsync_AssignsRole()
        {
            var context = GetInMemoryContext();
            var user = CreateTestUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userManagerMock = GetMockUserManager();
            var roleManagerMock = GetMockRoleManager();

            roleManagerMock.Setup(r => r.RoleExistsAsync("User")).ReturnsAsync(false);
            roleManagerMock.Setup(r => r.CreateAsync(It.IsAny<IdentityRole<int>>())).ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(um => um.IsInRoleAsync(user, "User")).ReturnsAsync(false);
            userManagerMock.Setup(um => um.AddToRoleAsync(user, "User")).ReturnsAsync(IdentityResult.Success);

            var repo = GetRepository(context, userManagerMock, roleManagerMock);

            var count = await repo.AddDefaultRoleToAllUsersAsync();

            Assert.Equal(1, count);
        }

        [Fact]
        public async Task AddDefaultRoleToAllUsersAsync_LogsAndContinues_OnAddFailure()
        {
            var context = GetInMemoryContext();
            var user = CreateTestUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userManagerMock = GetMockUserManager();
            var roleManagerMock = GetMockRoleManager();

            roleManagerMock.Setup(r => r.RoleExistsAsync("User")).ReturnsAsync(true);
            userManagerMock.Setup(um => um.IsInRoleAsync(user, "User")).ReturnsAsync(false);
            userManagerMock.Setup(um => um.AddToRoleAsync(user, "User"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role assign failed" }));

            var repo = GetRepository(context, userManagerMock, roleManagerMock);

            var count = await repo.AddDefaultRoleToAllUsersAsync();

            Assert.Equal(0, count);
        }

        [Fact]
        public async Task AddDefaultRoleToAllUsersAsync_Throws_OnException()
        {
            var context = GetInMemoryContext();
            var user = CreateTestUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userManagerMock = GetMockUserManager();
            var roleManagerMock = GetMockRoleManager();

            roleManagerMock.Setup(r => r.RoleExistsAsync("User")).ThrowsAsync(new Exception("Unexpected error"));

            var repo = GetRepository(context, userManagerMock, roleManagerMock);

            await Assert.ThrowsAsync<Exception>(() => repo.AddDefaultRoleToAllUsersAsync());
        }
    }
}
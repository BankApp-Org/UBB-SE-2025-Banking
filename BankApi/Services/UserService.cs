namespace BankApi.Services
{
    using BankApi.Data;
    using BankApi.Repositories;
    using Common.DTOs;
    using Common.Models;
    using Common.Models.Social;
    using Common.Services;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<User> userManager;
        private readonly ApiDbContext dbContext;

        public UserService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor, UserManager<User> userManager, ApiDbContext dbContext)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<User> GetUserByCnpAsync(string cnp)
        {
            return await userRepository.GetByCnpAsync(cnp);
        }

        public async Task<User> GetByIdAsync(int userId)
        {
            return await this.userRepository.GetByIdAsync(userId);
        }

        public Task<List<User>> GetUsers()
        {
            return userRepository.GetAllAsync();
        }

        public async Task<List<User>> GetNonFriends(string userCNP)
        {
            var user = await this.GetCurrentUserAsync(userCNP);
            return await userRepository.GetAllAsync().ContinueWith(t => t.Result.FindAll(u => !user.Friends.Contains(u)));
        }

        public async Task AddFriend(User friend)
        {
            var user = await this.GetCurrentUserAsync(null);
            await userRepository.AddFriend(user, friend);
        }

        public async Task RemoveFriend(User friend)
        {
            var user = await this.GetCurrentUserAsync(null);
            await userRepository.RemoveFriend(user, friend);
        }

        public async Task CreateUser(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            await userRepository.CreateAsync(user);
        }

        /// <summary>
        /// Updates the user's profile with new information.
        /// </summary>
        /// <param name="user">The user object with updated information.</param>
        /// <param name="userCNP">The CNP of the user to update.</param>
        public async Task UpdateUserAsync(User updatedUser, string userCNP)
        {
            ArgumentNullException.ThrowIfNull(updatedUser);

            if (string.IsNullOrWhiteSpace(userCNP))
            {
                throw new ArgumentException("User CNP cannot be empty");
            }

            User existingUser = await this.GetUserByCnpAsync(userCNP) ?? throw new KeyNotFoundException($"User with CNP {userCNP} not found.");

            // Update only the fields that are allowed to be updated
            existingUser.UserName = updatedUser.UserName;
            existingUser.Image = updatedUser.Image;
            existingUser.Description = updatedUser.Description;
            existingUser.IsHidden = updatedUser.IsHidden;
            existingUser.Email = updatedUser.Email;
            existingUser.PhoneNumber = updatedUser.PhoneNumber;

            await userRepository.UpdateAsync(existingUser);
        }

        /// <summary>
        /// Updates the admin status of the current user.
        /// </summary>
        /// <param name="isAdmin"> Indicates if the user should be an admin.</param>
        public async Task UpdateIsAdminAsync(bool isAdmin, string userCNP)
        {
            User user = await this.GetUserByCnpAsync(userCNP) ?? throw new KeyNotFoundException($"User with CNP {userCNP} not found.");

            await userRepository.UpdateRolesAsync(user, [isAdmin ? "Admin" : "User"]);
        }

        public async Task<User> GetCurrentUserAsync(string userCNP)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User ID not found in claims.");
            }

            return await userRepository.GetByIdAsync(int.Parse(userId)) ?? throw new KeyNotFoundException($"User with ID {userId} not found.");
        }

        public async Task<int> GetCurrentUserGemsAsync(string userCNP)
        {
            if (string.IsNullOrWhiteSpace(userCNP))
            {
                throw new ArgumentException("CNP cannot be empty");
            }
            User user = await this.GetUserByCnpAsync(userCNP) ?? throw new KeyNotFoundException($"User with CNP {userCNP} not found.");
            return user.GemBalance;
        }

        public async Task<int> AddDefaultRoleToAllUsersAsync()
        {
            return await userRepository.AddDefaultRoleToAllUsersAsync();
        }

        public async Task<List<SocialUserDto>> GetNonFriendsUsers(string userCNP)
        {
            if (string.IsNullOrWhiteSpace(userCNP))
            {
                throw new ArgumentException("CNP cannot be empty");
            }
            User currentUser = await this.GetCurrentUserAsync(userCNP);
            List<User> allUsers = await this.GetUsers();
            List<User> nonFriends = allUsers.FindAll(u => !currentUser.Friends.Contains(u) && u.CNP != userCNP);
            return nonFriends.ConvertAll(u => new SocialUserDto
            {
                Cnp = u.CNP,
                UserID = u.Id,
                Email = u.Email?.ToString(),
                FirstName = u.FirstName,
                LastName = u.LastName,
                Username = u.UserName,
                ReportedCount = u.ReportedCount
            });
        }

        public async Task<string> DeleteUser(string password)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User ID not found in claims.");
            }

            var user = await userRepository.GetByIdAsync(int.Parse(userId));
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            // Password verification
            var passwordValid = await userManager.CheckPasswordAsync(user, password);
            if (!passwordValid)
            {
                throw new UnauthorizedAccessException("Incorrect password.");
            }

            // Delete related ChatReports
            var chatReports = await dbContext.Set<ChatReport>()
                .Where(cr => cr.SubmitterCnp == user.CNP)
                .ToListAsync();
            dbContext.RemoveRange(chatReports);
            await dbContext.SaveChangesAsync();

            // Now delete the user
            var result = await userRepository.DeleteAsync(user.Id);
            if (!result)
            {
                throw new InvalidOperationException("Failed to delete user. You may have related data (bank accounts, transactions, etc.) that must be removed first.");
            }

            return "User deleted";
        }
    }
}
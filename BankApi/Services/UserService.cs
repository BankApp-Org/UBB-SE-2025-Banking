namespace BankApi.Services
{
    using BankApi.Repositories;
    using Common.Models;
    using Common.Services;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;

        public UserService(IUserRepository userRepository)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<User> GetUserByCnpAsync(string cnp)
        {
            return string.IsNullOrWhiteSpace(cnp)
                ? throw new ArgumentException("CNP cannot be empty")
                : await userRepository.GetByCnpAsync(cnp) ?? throw new KeyNotFoundException($"User with CNP {cnp} not found.");
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
            user.Friends.Add(friend);
        }

        public async Task RemoveFriend(User friend)
        {
            var user = await this.GetCurrentUserAsync(null);
            user.Friends.Remove(friend);
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
            return string.IsNullOrWhiteSpace(userCNP)
                ? throw new ArgumentException("CNP cannot be empty")
                : await this.GetUserByCnpAsync(userCNP) ?? throw new KeyNotFoundException($"User with CNP {userCNP} not found.");
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
    }
}
namespace Common.Services
{
    using Common.DTOs;
    using Common.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IUserService
    {
        Task CreateUser(User user);

        Task<User> GetUserByCnpAsync(string cnp);

        Task<User> GetByIdAsync(int userId);

        Task<List<User>> GetUsers();

        Task<List<User>> GetNonFriends(string cnp);

        Task AddFriend(User friend);
        Task RemoveFriend(User friend);

        Task UpdateIsAdminAsync(bool newIsAdmin, string? userCNP = null);

        Task UpdateUserAsync(User user, string? userCNP = null);

        Task<User> GetCurrentUserAsync(string? userCNP = null);

        Task<int> GetCurrentUserGemsAsync(string? userCNP = null);

        Task<int> AddDefaultRoleToAllUsersAsync();

        Task<List<SocialUserDto>> GetNonFriendsUsers(string userCNP);
    }
}

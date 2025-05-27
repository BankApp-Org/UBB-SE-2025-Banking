namespace Common.Services
{
    using Common.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IUserService
    {
        Task CreateUser(User user);

        Task<User> GetUserByCnpAsync(string cnp);

        Task<List<User>> GetUsers();

        Task UpdateIsAdminAsync(bool newIsAdmin, string? userCNP = null);

        Task UpdateUserAsync(User user, string? userCNP = null);

        Task<User> GetCurrentUserAsync(string? userCNP = null);

        Task<int> GetCurrentUserGemsAsync(string? userCNP = null);

        Task<int> AddDefaultRoleToAllUsersAsync();
    }
}

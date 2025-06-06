using Common.Models;

namespace BankApi.Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync();
        Task<User> GetByIdAsync(int id);
        Task<User> GetByCnpAsync(string cnp);
        Task<User> GetByUsernameAsync(string username);
        Task<User> CreateAsync(User user);
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
        Task AddFriend(User user, User friend);
        Task RemoveFriend(User user, User friend);
        Task<bool> UpdateRolesAsync(User user, IEnumerable<string> roleNames);
        Task<int> AddDefaultRoleToAllUsersAsync();
    }
}

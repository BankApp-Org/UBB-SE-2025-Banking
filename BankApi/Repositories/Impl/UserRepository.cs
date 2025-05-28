using BankApi.Data;
using Common.Models;
using Common.Models.Bank;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Repositories.Impl
{
    public class UserRepository : IUserRepository
    {
        private readonly ApiDbContext _context;
        private readonly ILogger<UserRepository> _logger;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public UserRepository(
            ApiDbContext context,
            ILogger<UserRepository> logger,
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task<List<User>> GetAllAsync() => await _context.Users.ToListAsync();

        public async Task<User> GetByIdAsync(int id) => await _context.Users.FindAsync(id);

        public async Task<User> GetByCnpAsync(string cnp) => await _context.Users.FirstOrDefaultAsync(u => u.CNP == cnp);

        public async Task<User> GetByUsernameAsync(string username) => await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

        public async Task<User> CreateAsync(User user)
        {
            if (await _userManager.FindByNameAsync(user.UserName) == null && await _context.Users.AllAsync(u => u.CNP != user.CNP))
            {
                var result = await _userManager.CreateAsync(user, user.PasswordHash);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
                else
                {
                    await _context.SaveChangesAsync();
                    return user;
                }
            }
            throw new InvalidOperationException($"User with username {user.UserName} or CNP {user.CNP} already exists.");
        }

        public async Task<bool> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateRolesAsync(User user, IEnumerable<string> roleNames)
        {
            ArgumentNullException.ThrowIfNull(user);

            ArgumentNullException.ThrowIfNull(roleNames);

            try
            {
                // Get current roles for the user
                var currentRoles = await _userManager.GetRolesAsync(user);

                // Remove user from all current roles
                if (currentRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        return false;
                    }
                }

                // Add user to the new roles
                if (roleNames.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, roleNames);
                    if (!addResult.Succeeded)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> AddDefaultRoleToAllUsersAsync()
        {
            try
            {
                const string defaultRole = "User";
                int successCount = 0;
                var allUsers = await GetAllAsync();

                // Ensure the User role exists
                if (!await _roleManager.RoleExistsAsync(defaultRole))
                {
                    await _roleManager.CreateAsync(new IdentityRole<int>(defaultRole));
                    _logger.LogInformation($"Created the '{defaultRole}' role");
                }

                foreach (var user in allUsers)
                {
                    // Check if user already has the User role
                    if (!await _userManager.IsInRoleAsync(user, defaultRole))
                    {
                        var result = await _userManager.AddToRoleAsync(user, defaultRole);
                        if (result.Succeeded)
                        {
                            successCount++;
                            _logger.LogInformation($"Added '{defaultRole}' role to user {user.UserName}");
                        }
                        else
                        {
                            _logger.LogWarning($"Failed to add '{defaultRole}' role to user {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                }

                return successCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding default role to users");
                throw;
            }
        }

        public async Task<List<BankAccount>> GetUserBankAccounts(int id_user)
        {
            if (id_user <= 0)
            {
                return [];
            }

            try
            {
                return await _context.BankAccounts
                    .Where(b => b.UserId == id_user)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return [];
            }
        }
    }
}

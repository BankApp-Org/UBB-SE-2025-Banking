using Common.Models;

namespace BankApi.Repositories
{
    public interface IActivityRepository
    {
        Task<List<ActivityLog>> GetActivityForUserAsync(string userCnp);
        Task<ActivityLog> AddActivityAsync(ActivityLog activity);
        Task<List<ActivityLog>> GetAllActivitiesAsync();
        Task<ActivityLog> GetActivityByIdAsync(int id);
        Task<bool> DeleteActivityAsync(int id);
    }
}
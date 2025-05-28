using Common.Models.Bank;

namespace BankApi.Repositories.Bank
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
using BankApi.Data;
using BankApi.Repositories.Bank;
using Common.Models.Bank;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Repositories.Impl.Bank
{
    public class ActivityRepository(ApiDbContext context, ILogger<ActivityRepository> logger) : IActivityRepository
    {
        private readonly ApiDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger<ActivityRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<List<ActivityLog>> GetActivityForUserAsync(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                return await _context.ActivityLogs
                    .Where(a => a.UserCnp == userCnp)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activities for user {UserCnp}", userCnp);
                throw;
            }
        }

        public async Task<ActivityLog> AddActivityAsync(ActivityLog activity)
        {
            if (string.IsNullOrWhiteSpace(activity.UserCnp))
                throw new ArgumentException("User CNP cannot be empty", nameof(activity.UserCnp));
            if (string.IsNullOrWhiteSpace(activity.ActivityName))
                throw new ArgumentException("Activity name cannot be empty", nameof(activity.ActivityName));
            if (activity.LastModifiedAmount <= 0)
                throw new ArgumentException("Amount must be greater than 0", nameof(activity.LastModifiedAmount));

            try
            {
                activity.CreatedAt = DateTime.UtcNow; // Ensure CreatedAt is set to current time

                await _context.ActivityLogs.AddAsync(activity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Added new activity for user {UserCnp}", activity.UserCnp);
                return activity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding activity for user {UserCnp}", activity.UserCnp);
                throw;
            }
        }

        public async Task<List<ActivityLog>> GetAllActivitiesAsync()
        {
            try
            {
                return await _context.ActivityLogs
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all activities");
                throw;
            }
        }

        public async Task<ActivityLog> GetActivityByIdAsync(int id)
        {
            try
            {
                var activity = await _context.ActivityLogs.FindAsync(id);
                return activity ?? throw new KeyNotFoundException($"Activity with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity with ID {ActivityId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteActivityAsync(int id)
        {
            try
            {
                var activity = await _context.ActivityLogs.FindAsync(id);
                if (activity == null)
                {
                    return false;
                }

                _context.ActivityLogs.Remove(activity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting activity with ID {ActivityId}", id);
                throw;
            }
        }
    }
}
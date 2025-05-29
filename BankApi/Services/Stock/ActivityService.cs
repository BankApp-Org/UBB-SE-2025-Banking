namespace BankApi.Services.Stock
{
    using BankApi.Repositories.Bank;
    using Common.Models.Bank;
    using Common.Services.Bank;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Service for managing user activities.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ActivityService"/> class.
    /// </remarks>
    /// <param name="activityRepository">The activity repository.</param>
    /// <exception cref="ArgumentNullException">Thrown when activityRepository is null.</exception>
    public class ActivityService(IActivityRepository activityRepository) : IActivityService
    {
        private readonly IActivityRepository _activityRepository = activityRepository ?? throw new ArgumentNullException(nameof(activityRepository));

        /// <inheritdoc/>
        public async Task<List<ActivityLog>> GetActivityForUser(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                return await _activityRepository.GetActivityForUserAsync(userCnp);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving activities for user {userCnp}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<ActivityLog> AddActivity(string userCnp, string activityName, int amount, string details)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            if (string.IsNullOrWhiteSpace(activityName))
            {
                throw new ArgumentException("Activity name cannot be empty", nameof(activityName));
            }

            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than 0", nameof(amount));
            }

            try
            {
                ActivityLog activity = new()
                {
                    UserCnp = userCnp,
                    ActivityName = activityName,
                    LastModifiedAmount = amount,
                    ActivityDetails = details,
                    CreatedAt = DateTime.UtcNow,
                };
                return await _activityRepository.AddActivityAsync(activity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding activity for user {userCnp}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<List<ActivityLog>> GetAllActivities()
        {
            try
            {
                return await _activityRepository.GetAllActivitiesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all activities", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<ActivityLog> GetActivityById(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Activity ID must be greater than 0", nameof(id));
            }

            try
            {
                return await _activityRepository.GetActivityByIdAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving activity with ID {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteActivity(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Activity ID must be greater than 0", nameof(id));
            }

            try
            {
                return await _activityRepository.DeleteActivityAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting activity with ID {id}", ex);
            }
        }
    }
}

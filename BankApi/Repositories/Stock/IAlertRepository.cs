// BankApi/Repositories/IAlertRepository.cs
using Common.Models;
using Common.Models.Trading;

namespace BankApi.Repositories.Trading
{
    /// <summary>
    /// Interface for repository operations on <see cref="Alert"/> entities.
    /// </summary>
    public interface IAlertRepository
    {
        /// <summary>
        /// Gets all alerts asynchronously.
        /// </summary>
        /// <returns>A list of all configured alerts.</returns>
        Task<List<Alert>> GetAllAlertsAsync();

        /// <summary>
        /// Retrieves a single alert by its identifier asynchronously.
        /// </summary>
        /// <param name="alertId">The unique ID of the alert.</param>
        /// <returns>The matching <see cref="Alert"/>.</returns>
        Task<Alert> GetAlertByIdAsync(int alertId);

        /// <summary>
        /// Adds a new alert asynchronously.
        /// </summary>
        /// <param name="alert">The alert to add.</param>
        /// <returns>The newly created <see cref="Alert"/> with assigned ID.</returns>
        Task<Alert> AddAlertAsync(Alert alert);

        /// <summary>
        /// Updates an existing alert asynchronously.
        /// </summary>
        /// <param name="alert">The alert with updated properties.</param>
        /// <returns>The updated <see cref="Alert"/>.</returns>
        Task<Alert> UpdateAlertAsync(Alert alert);

        /// <summary>
        /// Deletes an alert asynchronously.
        /// </summary>
        /// <param name="alertId">ID of the alert to delete.</param>
        /// <returns>True if deletion was successful, false otherwise.</returns>
        Task<bool> DeleteAlertAsync(int alertId);

        /// <summary>
        /// Gets all triggered alerts.
        /// </summary>
        /// <returns>A list of triggered alerts.</returns>
        Task<List<TriggeredAlert>> GetTriggeredAlertsAsync();

        /// <summary>
        /// Clears the triggered alerts.
        /// </summary>
        Task ClearTriggeredAlertsAsync();

        /// <summary>
        /// Checks if an alert is triggered for a stock at the given price.
        /// </summary>
        /// <param name="stockName">Name of the stock.</param>
        /// <param name="currentPrice">Current price of the stock.</param>
        /// <returns>True if an alert is triggered, false otherwise.</returns>
        Task<bool> IsAlertTriggeredAsync(string stockName, decimal currentPrice);

        /// <summary>
        /// Records a triggered alert if conditions are met.
        /// </summary>
        /// <param name="stockName">Name of the stock.</param>
        /// <param name="currentPrice">Current price of the stock.</param>
        /// <returns>The triggered alert if created, otherwise null.</returns>
        Task<TriggeredAlert> TriggerAlertAsync(string stockName, decimal currentPrice);
    }
}

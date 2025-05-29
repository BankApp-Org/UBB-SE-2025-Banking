// BankApi/Repositories/AlertRepository.cs
using BankApi.Data;
using BankApi.Repositories.Stock;
using Common.Models;
using Common.Models.Trading;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Repositories.Impl.Stocks
{
    /// <summary>
    /// BaseStockRepository for managing <see cref="Alert"/> entities and their triggered instances using Entity Framework Core.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AlertRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context.</param>
    public class AlertRepository(ApiDbContext context) : IAlertRepository
    {
        private readonly ApiDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        /// <summary>
        /// Gets all alerts asynchronously.
        /// </summary>
        /// <returns>A list of all configured alerts.</returns>
        public async Task<List<Alert>> GetAllAlertsAsync()
        {
            return await _context.Alerts.ToListAsync();
        }

        /// <summary>
        /// Retrieves a single alert by its identifier asynchronously.
        /// </summary>
        /// <param name="alertId">The unique ID of the alert.</param>
        /// <returns>The matching <see cref="Alert"/>.</returns>
        public async Task<Alert> GetAlertByIdAsync(int alertId)
        {
            var alert = await _context.Alerts.FindAsync(alertId);
            return alert ?? throw new KeyNotFoundException($"Alert with ID {alertId} not found.");
        }

        /// <summary>
        /// Adds a new alert asynchronously.
        /// </summary>
        /// <param name="alert">The alert to add.</param>
        /// <returns>The newly created <see cref="Alert"/> with assigned ID.</returns>
        public async Task<Alert> AddAlertAsync(Alert alert)
        {
            _context.Alerts.Add(alert);
            await _context.SaveChangesAsync();
            return alert;
        }

        /// <summary>
        /// Updates an existing alert asynchronously.
        /// </summary>
        /// <param name="alert">The alert with updated properties.</param>
        /// <returns>The updated <see cref="Alert"/>.</returns>
        public async Task<Alert> UpdateAlertAsync(Alert alert)
        {
            try
            {
                var existingAlert = await _context.Alerts.FindAsync(alert.AlertId) ?? throw new KeyNotFoundException($"Alert with ID {alert.AlertId} not found.");
                // Update properties
                existingAlert.StockName = alert.StockName;
                existingAlert.Name = alert.Name;
                existingAlert.LowerBound = alert.LowerBound;
                existingAlert.UpperBound = alert.UpperBound;
                existingAlert.ToggleOnOff = alert.ToggleOnOff;

                await _context.SaveChangesAsync();
                return existingAlert;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Failed to update alert with ID {alert.AlertId}.", ex);
            }
        }

        /// <summary>
        /// Deletes an alert asynchronously.
        /// </summary>
        /// <param name="alertId">ID of the alert to delete.</param>
        /// <returns>True if deletion was successful, false otherwise.</returns>
        public async Task<bool> DeleteAlertAsync(int alertId)
        {
            var alert = await _context.Alerts.FindAsync(alertId);
            if (alert == null)
            {
                return false;
            }

            _context.Alerts.Remove(alert);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Gets all triggered alerts.
        /// </summary>
        /// <returns>A list of triggered alerts.</returns>
        public async Task<List<TriggeredAlert>> GetTriggeredAlertsAsync()
        {
            return await _context.TriggeredAlerts.ToListAsync();
        }

        /// <summary>
        /// Clears the triggered alerts.
        /// </summary>
        public async Task ClearTriggeredAlertsAsync()
        {
            _context.TriggeredAlerts.RemoveRange(await _context.TriggeredAlerts.ToListAsync());
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Checks if an alert is triggered for a stock at the given price.
        /// </summary>
        /// <param name="stockName">Name of the stock.</param>
        /// <param name="currentPrice">Current price of the stock.</param>
        /// <returns>True if an alert is triggered, false otherwise.</returns>
        public async Task<bool> IsAlertTriggeredAsync(string stockName, decimal currentPrice)
        {
            var alert = await _context.Alerts.FirstOrDefaultAsync(a => a.StockName == stockName);
            return alert != null
                   && alert.ToggleOnOff
                   && (currentPrice >= alert.UpperBound || currentPrice <= alert.LowerBound);
        }

        /// <summary>
        /// Records a triggered alert if conditions are met.
        /// </summary>
        /// <param name="stockName">Name of the stock.</param>
        /// <param name="currentPrice">Current price of the stock.</param>
        /// <returns>The triggered alert if created, otherwise null.</returns>
        public async Task<TriggeredAlert> TriggerAlertAsync(string stockName, decimal currentPrice)
        {
            if (!await IsAlertTriggeredAsync(stockName, currentPrice))
            {
                return null;
            }

            var alert = await _context.Alerts.FirstAsync(a => a.StockName == stockName);
            string message = $"Alert triggered for {stockName}: Price = {currentPrice}, Bounds: [{alert.LowerBound} - {alert.UpperBound}]";

            var triggeredAlert = new TriggeredAlert
            {
                StockName = stockName,
                Message = message,
                TriggeredAt = DateTime.UtcNow
            };

            _context.TriggeredAlerts.Add(triggeredAlert);
            await _context.SaveChangesAsync();

            return triggeredAlert;
        }
    }
}

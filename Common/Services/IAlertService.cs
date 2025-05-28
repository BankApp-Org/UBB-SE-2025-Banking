namespace Common.Services
{
    using Common.Models;
    using Common.Models.Trading;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAlertService
    {
        Task<Alert> CreateAlertAsync(string stockName, string name, decimal upperBound, decimal lowerBound, bool toggleOnOff);
        Task<Alert?> GetAlertByIdAsync(int alertId);
        Task<List<Alert>> GetAllAlertsAsync();
        Task<List<Alert>> GetAllAlertsOnAsync();
        Task RemoveAlertAsync(int alertId);
        Task UpdateAlertAsync(Alert alert);
        Task UpdateAlertAsync(int alertId, string stockName, string name, decimal upperBound, decimal lowerBound, bool toggleOnOff);
        Task<List<TriggeredAlert>> GetTriggeredAlertsAsync();
    }
}
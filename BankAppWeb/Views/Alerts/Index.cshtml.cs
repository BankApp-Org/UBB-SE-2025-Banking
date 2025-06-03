using Common.Models.Trading;
using Common.Services.Trading;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace BankAppWeb.Views.Alerts
{
    public class IndexModel : PageModel
    {
        private readonly IAlertService _alertService;

        public IndexModel(IAlertService alertService)
        {
            _alertService = alertService;
        }

        public List<Alert> Alerts { get; private set; } = [];
        public string? ErrorMessage { get; private set; }
        public string? SuccessMessage { get; private set; }
        public string SelectedStockName { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            try
            {
                Alerts = await _alertService.GetAllAlertsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading alerts: {ex.Message}";
            }
        }

        public async Task CreateAlertAsync(CreateAlertDto createAlert)
        {
            // Validate the alert
            if (string.IsNullOrWhiteSpace(createAlert.Name))
            {
                throw new ArgumentException("Alert name cannot be empty.");
            }

            if (createAlert.LowerBound >= createAlert.UpperBound)
            {
                throw new ArgumentException("Lower bound must be less than upper bound.");
            }

            if (string.IsNullOrWhiteSpace(createAlert.StockName))
            {
                throw new ArgumentException("Stock name cannot be empty.");
            }

            var newAlert = await _alertService.CreateAlertAsync(
                createAlert.StockName,
                createAlert.Name,
                createAlert.UpperBound,
                createAlert.LowerBound,
                createAlert.ToggleOnOff);

            // Reload alerts to show the new one
            await OnGetAsync();
        }

        public async Task UpdateAlertAsync(Alert alert)
        {
            // Validate the alert
            if (string.IsNullOrWhiteSpace(alert.Name))
            {
                throw new ArgumentException("Alert name cannot be empty.");
            }

            if (alert.LowerBound >= alert.UpperBound)
            {
                throw new ArgumentException("Lower bound must be less than upper bound.");
            }

            await _alertService.UpdateAlertAsync(alert);
        }

        public async Task DeleteAlertAsync(int alertId)
        {
            await _alertService.RemoveAlertAsync(alertId);
        }

        public async Task SaveAllAlertsAsync(List<Alert> alerts)
        {
            foreach (var alert in alerts)
            {
                if (alert.LowerBound >= alert.UpperBound)
                {
                    throw new ArgumentException("Lower bound must be less than upper bound.");
                }

                if (string.IsNullOrWhiteSpace(alert.Name))
                {
                    throw new ArgumentException("Alert name cannot be empty.");
                }

                await _alertService.UpdateAlertAsync(alert);
            }
        }

        public class CreateAlertDto
        {
            [Required]
            [MaxLength(100)]
            public string StockName { get; set; } = string.Empty;

            [Required]
            [MaxLength(100)]
            public string Name { get; set; } = string.Empty;

            [Required]
            [Range(0.01, double.MaxValue, ErrorMessage = "Upper bound must be greater than 0")]
            public decimal UpperBound { get; set; }

            [Required]
            [Range(0.01, double.MaxValue, ErrorMessage = "Lower bound must be greater than 0")]
            public decimal LowerBound { get; set; }

            public bool ToggleOnOff { get; set; } = true;
        }
    }
}

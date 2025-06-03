using BankAppWeb.Views.Alerts;
using Common.Models.Trading;
using Common.Services.Trading;
using Microsoft.AspNetCore.Mvc;

namespace BankAppWeb.Controllers
{
    public class AlertsController : Controller
    {
        private readonly IAlertService _alertService;

        public AlertsController(IAlertService alertService)
        {
            _alertService = alertService;
        }

        public async Task<IActionResult> Index(string? stockName = null)
        {
            var model = new IndexModel(_alertService);
            if (!string.IsNullOrEmpty(stockName))
            {
                model.SelectedStockName = stockName;
            }
            await model.OnGetAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAlert(IndexModel.CreateAlertDto createAlert)
        {
            var model = new IndexModel(_alertService);

            if (ModelState.IsValid)
            {
                try
                {
                    await model.CreateAlertAsync(createAlert);
                    TempData["SuccessMessage"] = "Alert created successfully!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error creating alert: {ex.Message}";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Please fill in all required fields correctly.";
            }

            return RedirectToAction("Index", new { stockName = createAlert.StockName });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAlert(Alert alert)
        {
            var model = new IndexModel(_alertService);

            try
            {
                await model.UpdateAlertAsync(alert);
                TempData["SuccessMessage"] = "Alert updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating alert: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAlert(int alertId)
        {
            var model = new IndexModel(_alertService);

            try
            {
                await model.DeleteAlertAsync(alertId);
                TempData["SuccessMessage"] = "Alert deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting alert: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAllAlerts(List<Alert> alerts)
        {
            var model = new IndexModel(_alertService);

            try
            {
                await model.SaveAllAlertsAsync(alerts);
                TempData["SuccessMessage"] = "All alerts saved successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error saving alerts: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}

using Common.Models;
using Common.Models.Trading;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AlertController(IAlertService alertService) : ControllerBase
    {
        private readonly IAlertService _alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));

        [HttpGet]
        public async Task<ActionResult<List<Alert>>> GetAllAlerts()
        {
            return await _alertService.GetAllAlertsAsync();
        }

        [HttpGet("on")]
        public async Task<ActionResult<List<Alert>>> GetAllAlertsOn()
        {
            return await _alertService.GetAllAlertsOnAsync();
        }

        [HttpGet("{alertId}")]
        public async Task<ActionResult<Alert>> GetAlertById(int alertId)
        {
            var alert = await _alertService.GetAlertByIdAsync(alertId);

            return alert == null ? (ActionResult<Alert>)NotFound() : (ActionResult<Alert>)alert;
        }

        [HttpGet("triggered")]
        public async Task<ActionResult<List<TriggeredAlert>>> GetTriggeredAlerts()
        {
            return await _alertService.GetTriggeredAlertsAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Alert>> CreateAlert([FromBody] AlertCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var alert = await _alertService.CreateAlertAsync(
                dto.StockName,
                dto.Name,
                dto.UpperBound,
                dto.LowerBound,
                dto.ToggleOnOff);

            return CreatedAtAction(nameof(GetAlertById), new { alertId = alert.AlertId }, alert);
        }

        [HttpPut("{alertId}")]
        public async Task<IActionResult> UpdateAlert(int alertId, [FromBody] AlertUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingAlert = await _alertService.GetAlertByIdAsync(alertId);
            if (existingAlert == null)
                return NotFound();

            await _alertService.UpdateAlertAsync(
                alertId,
                dto.StockName,
                dto.Name,
                dto.UpperBound,
                dto.LowerBound,
                dto.ToggleOnOff);

            return NoContent();
        }

        [HttpDelete("{alertId}")]
        public async Task<IActionResult> DeleteAlert(int alertId)
        {
            var existingAlert = await _alertService.GetAlertByIdAsync(alertId);
            if (existingAlert == null)
                return NotFound();

            await _alertService.RemoveAlertAsync(alertId);
            return NoContent();
        }
    }

    public class AlertCreateDto
    {
        public string StockName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal UpperBound { get; set; }
        public decimal LowerBound { get; set; }
        public bool ToggleOnOff { get; set; }
    }

    public class AlertUpdateDto
    {
        public string StockName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal UpperBound { get; set; }
        public decimal LowerBound { get; set; }
        public bool ToggleOnOff { get; set; }
    }
}

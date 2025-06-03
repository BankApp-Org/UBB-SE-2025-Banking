using Common.DTOs;
using Common.Services;
using Common.Services.Social;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;
        private readonly IChatReportService _reportService;

        public ReportController(INotificationService notificationService, IUserService userService, IChatReportService reportService)
        {
            _reportService = reportService;
            _notificationService = notificationService;
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReportDto>> GetReportById(int id)
        {
            var report = await _reportService.GetReportById(id);
            if (report == null)
                return NotFound();

            var dto = new ReportDto
            {
                MessageID = report.MessageID,
                ReporterUserID = report.ReporterUserID,
                Reason = report.Reason,
                Description = report.Description,
                Status = report.Status,
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult> AddReport([FromBody] ReportDto reportDto)
        {
            if (reportDto == null)
                return BadRequest("Report data is required.");

            var report = new Report(
                reportDto.MessageID,
                reportDto.ReporterUserID,
                reportDto.Status,
                reportDto.Reason,
                reportDto.Description
            );

            await _reportService.AddReport(report);
            return CreatedAtAction(nameof(GetReportById), new { id = report.MessageID }, reportDto);
        }

        [HttpGet("exists/{messageId}/{reporterUserId}")]
        public async Task<ActionResult<bool>> CheckIfReportExists(int messageId, int reporterUserId)
        {
            var exists = await _reportService.CheckIfReportExists(messageId, reporterUserId);
            return Ok(exists);
        }

        [HttpPost("increase-report-count/{reportedId}")]
        public async Task<ActionResult> IncreaseReportCount(int reportedId)
        {
            await _reportService.IncreaseReportCount(reportedId);
            return NoContent();
        }

        [HttpPost("log-reports")]
        public async Task<ActionResult> LogReportedMessages([FromBody] List<ReportDto> reportDtos)
        {
            if (reportDtos == null || reportDtos.Count == 0)
                return BadRequest("Report data is required.");

            var reports = reportDtos.Select(dto => new Report(
                dto.MessageID,
                dto.ReporterUserID,
                dto.Status,
                dto.Reason,
                dto.Description
            )).ToList();

            await _reportService.LogReportedMessages(reports);
            return NoContent();
        }

        [HttpPost("send")]
        public async Task<ActionResult> SendReport([FromBody] ReportDto reportDto)
        {
            if (reportDto == null)
                return BadRequest("Report data is required.");

            var report = new Report(
                reportDto.MessageID,
                reportDto.ReporterUserID,
                reportDto.Status,
                reportDto.Reason,
                reportDto.Description
            );

            await _reportService.SendReport(report);
            return NoContent();
        }
    }
}
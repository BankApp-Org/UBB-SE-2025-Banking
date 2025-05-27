using System.Security.Claims;
using BankApi.Repositories;
using Common.Models;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BillSplitReportController(IBillSplitReportService billSplitReportService, IUserRepository userRepository) : ControllerBase
    {
        private readonly IBillSplitReportService _billSplitReportService = billSplitReportService ?? throw new ArgumentNullException(nameof(billSplitReportService));
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository)); // To fetch user CNP

        private async Task<string> GetCurrentUserCnp()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var user = await _userRepository.GetByIdAsync(int.Parse(userId));
            return user == null ? throw new Exception("User not found") : user.CNP;
        }

        [HttpGet]
        public async Task<ActionResult<List<BillSplitReport>>> GetBillSplitReports()
        {
            try
            {
                var reports = await _billSplitReportService.GetBillSplitReportsAsync();
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BillSplitReport>> GetBillSplitReportById(int id)
        {
            try
            {
                var report = await _billSplitReportService.GetBillSplitReportByIdAsync(id);
                if (report == null)
                {
                    return NotFound($"Bill split report with ID {id} not found");
                }

                // Ensure user has access to this report (they are involved or an admin)
                var currentUserCnp = await GetCurrentUserCnp();
                return report.ReportingUserCnp != currentUserCnp &&
                    report.ReportedUserCnp != currentUserCnp &&
                    !User.IsInRole("Admin")
                    ? (ActionResult<BillSplitReport>)Forbid()
                    : (ActionResult<BillSplitReport>)Ok(report);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<BillSplitReport>> CreateBillSplitReport([FromBody] BillSplitReport report)
        {
            try
            {
                // Always use the current user's CNP as the reporting user
                var currentUserCnp = await GetCurrentUserCnp();
                report.ReportingUserCnp = currentUserCnp;

                // Validate that the user isn't reporting themselves
                if (report.ReportedUserCnp == currentUserCnp)
                {
                    return BadRequest("You cannot report yourself for a bill split");
                }

                var createdReport = await _billSplitReportService.CreateBillSplitReportAsync(report);
                return CreatedAtAction(nameof(GetBillSplitReportById), new { id = createdReport.Id }, createdReport);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BillSplitReport>> UpdateBillSplitReport(int id, [FromBody] BillSplitReport report)
        {
            try
            {
                // Ensure the ID in the URL matches the one in the body
                if (id != report.Id)
                {
                    return BadRequest("Report ID mismatch");
                }

                // Verify the report exists
                var existingReport = await _billSplitReportService.GetBillSplitReportByIdAsync(id);
                if (existingReport == null)
                {
                    return NotFound($"Bill split report with ID {id} not found");
                }

                // Ensure user has permission to update this report (they created it or are an admin)
                var currentUserCnp = await GetCurrentUserCnp();
                if (existingReport.ReportingUserCnp != currentUserCnp && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Prevent changing the original reporting user
                report.ReportingUserCnp = existingReport.ReportingUserCnp;

                var updatedReport = await _billSplitReportService.UpdateBillSplitReportAsync(report);
                return Ok(updatedReport);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBillSplitReport(int id)
        {
            try
            {
                // Fetch the report to ensure it exists and for authorization checks
                var report = await _billSplitReportService.GetBillSplitReportByIdAsync(id);
                if (report == null)
                {
                    return NotFound($"Bill split report with ID {id} not found");
                }

                // Add authorization: ensure the user is an admin or the reporter
                var currentUserCnp = await GetCurrentUserCnp();
                if (report.ReportingUserCnp != currentUserCnp && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                await _billSplitReportService.DeleteBillSplitReportAsync(report);
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}/daysOverdue")]
        public async Task<ActionResult<int>> GetDaysOverdue(int id)
        {
            try
            {
                var report = await _billSplitReportService.GetBillSplitReportByIdAsync(id);
                if (report == null)
                {
                    return NotFound($"Bill split report with ID {id} not found");
                }

                // Ensure user has access to this report (they are involved or an admin)
                var currentUserCnp = await GetCurrentUserCnp();
                if (report.ReportingUserCnp != currentUserCnp &&
                    report.ReportedUserCnp != currentUserCnp &&
                    !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var daysOverdue = await _billSplitReportService.GetDaysOverdueAsync(report);
                return Ok(daysOverdue);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{id}/solve")]
        public async Task<IActionResult> SolveBillSplitReport(int id)
        {
            try
            {
                var report = await _billSplitReportService.GetBillSplitReportByIdAsync(id);
                if (report == null)
                {
                    return NotFound($"Bill split report with ID {id} not found");
                }

                // Add authorization: ensure the user is an admin or involved in the report
                var currentUserCnp = await GetCurrentUserCnp();
                if (report.ReportingUserCnp != currentUserCnp &&
                    report.ReportedUserCnp != currentUserCnp &&
                    !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                await _billSplitReportService.SolveBillSplitReportAsync(report);
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user/{userCnp}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<BillSplitReport>>> GetReportsByUser(string userCnp)
        {
            try
            {
                // Fetch all reports
                var allReports = await _billSplitReportService.GetBillSplitReportsAsync();

                // Filter reports where the specified user is either the reporter or reported
                var userReports = allReports.Where(r =>
                    r.ReportingUserCnp == userCnp || r.ReportedUserCnp == userCnp).ToList();

                return Ok(userReports);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("my-reports")]
        public async Task<ActionResult<List<BillSplitReport>>> GetMyReports()
        {
            try
            {
                var currentUserCnp = await GetCurrentUserCnp();

                // Fetch all reports
                var allReports = await _billSplitReportService.GetBillSplitReportsAsync();

                // Filter reports where the current user is either the reporter or reported
                var userReports = allReports.Where(r =>
                    r.ReportingUserCnp == currentUserCnp || r.ReportedUserCnp == currentUserCnp).ToList();

                return Ok(userReports);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

using BankApi.Repositories;
using Common.Models.Social;
using Common.Services;
using Common.Services.Social;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankApi.Controllers
{
    // Static class for legacy models to avoid namespace conflicts
    public static class LegacyReportModels
    {
        public class Report
        {
            public int MessageID { get; set; }
            public int ReporterUserID { get; set; }
            public string Status { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;

            public Report(int messageID, int reporterUserID, string status, string reason, string description)
            {
                MessageID = messageID;
                ReporterUserID = reporterUserID;
                Status = status;
                Reason = reason;
                Description = description;
            }
        }
    }

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatReportController : ControllerBase
    {
        private readonly IChatReportService _chatReportService;
        private readonly IUserRepository _userRepository;
        private readonly IMessageService _messagesService;
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;

        public ChatReportController(
            IChatReportService chatReportService,
            IUserRepository userRepository,
            IMessageService messagesService,
            INotificationService notificationService = null,
            IUserService userService = null)
        {
            _chatReportService = chatReportService ?? throw new ArgumentNullException(nameof(chatReportService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _messagesService = messagesService ?? throw new ArgumentNullException(nameof(messagesService));
            _notificationService = notificationService;
            _userService = userService;
        }

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
        [Authorize(Roles = "Admin")] // Only admins can view all chat reports
        public async Task<ActionResult<List<ChatReport>>> GetAllChatReports()
        {
            try
            {
                var reports = await _chatReportService.GetAllChatReportsAsync();
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ChatReport>> GetChatReportById(int id)
        {
            try
            {
                var report = await _chatReportService.GetChatReportByIdAsync(id);
                if (report == null)
                {
                    return NotFound($"Chat report with ID {id} not found");
                }

                // Ensure user has access to this report (they submitted it or are an admin)
                var currentUserCnp = await GetCurrentUserCnp();
                return report.SubmitterCnp != currentUserCnp && !User.IsInRole("Admin") ? (ActionResult<ChatReport>)Forbid() : (ActionResult<ChatReport>)Ok(report);
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
        public async Task<ActionResult<ChatReport>> AddChatReport([FromBody] ChatReport report)
        {
            try
            {
                // Set the submitter CNP to the current user
                var currentUserCnp = await GetCurrentUserCnp();
                report.SubmitterCnp = currentUserCnp;

                // Validate that user doesn't report themselves
                if (report.ReportedUserCnp == currentUserCnp)
                {
                    return BadRequest("You cannot report yourself");
                }

                await _chatReportService.AddChatReportAsync(report);
                return CreatedAtAction(nameof(GetChatReportById), new { id = report.Id }, report);
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
        public async Task<IActionResult> DeleteChatReport(int id)
        {
            try
            {
                // Verify the report exists
                var report = await _chatReportService.GetChatReportByIdAsync(id);
                if (report == null)
                {
                    return NotFound($"Chat report with ID {id} not found");
                }

                // Ensure user has permission (they submitted it or are an admin)
                var currentUserCnp = await GetCurrentUserCnp();
                if (report.SubmitterCnp != currentUserCnp && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                await _chatReportService.DeleteChatReportAsync(id);
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

        [HttpGet("user-tips/{userCnp}")]
        public async Task<ActionResult<int>> GetNumberOfGivenTipsForUser(string userCnp)
        {
            try
            {
                // Only allow users to view their own tips, or admins to view anyone's
                var currentUserCnp = await GetCurrentUserCnp();
                if (userCnp != currentUserCnp && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var count = await _chatReportService.GetNumberOfGivenTipsForUserAsync(userCnp);
                return Ok(count);
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

        [HttpPost("activity-log")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateActivityLog([FromBody] ActivityLogUpdateDto update)
        {
            try
            {
                var currentUserCnp = await GetCurrentUserCnp();
                await _chatReportService.UpdateActivityLogAsync(update.Amount, currentUserCnp);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("score-history")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateScoreHistoryForUser([FromBody] ScoreHistoryUpdateDto update)
        {
            try
            {
                var currentUserCnp = await GetCurrentUserCnp();
                await _chatReportService.UpdateScoreHistoryForUserAsync(update.NewScore, currentUserCnp);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("punish-with-message/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PunishUserWithMessage(int id, [FromBody] PunishmentMessageDto punishmentDto)
        {
            try
            {
                // Get the chat report
                var report = await _chatReportService.GetChatReportByIdAsync(id);
                if (report == null)
                {
                    return NotFound($"Chat report with ID {id} not found");
                }

                // Send a notification message to the user about the punishment
                if (!string.IsNullOrEmpty(punishmentDto.MessageContent) && _notificationService != null)
                {
                    try
                    {
                        // Get the user by CNP to obtain the user ID
                        var reportedUser = await _userRepository.GetByCnpAsync(report.ReportedUserCnp);
                        if (reportedUser != null)
                        {
                            // Create a notification object
                            var notification = new Notification
                            {
                                Content = punishmentDto.MessageContent,
                                UserId = reportedUser.Id,
                                User = reportedUser,
                                Timestamp = DateTime.UtcNow
                            };
                            
                            // Send notification using the correct method name
                            await _notificationService.CreateNotification(notification);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error but continue with punishment
                        Console.WriteLine($"Failed to send notification: {ex.Message}");
                    }
                }

                // Apply the punishment using the existing report
                if (punishmentDto.ShouldPunish)
                {
                    await _chatReportService.PunishUser(report);
                }
                else
                {
                    await _chatReportService.DoNotPunishUser(report);
                }

                return Ok(new { Message = punishmentDto.ShouldPunish ? "User punished and message sent" : "Report closed without punishment" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("exists/{messageId}/{reporterUserId}")]
        public async Task<ActionResult<bool>> CheckIfReportExists(int messageId, int reporterUserId)
        {
            try
            {
                if (_chatReportService == null)
                {
                    return StatusCode(500, "Service not available");
                }

                var exists = await _chatReportService.CheckIfReportExists(messageId, reporterUserId);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }

    public class ActivityLogUpdateDto
    {
        public int Amount { get; set; }
    }

    public class ScoreHistoryUpdateDto
    {
        public int NewScore { get; set; }
    }

    public class SendMessageDto
    {
        public string UserCnp { get; set; } = string.Empty;
        public string MessageType { get; set; } = "System";
        public string MessageContent { get; set; } = string.Empty;
    }

    public class PunishmentMessageDto
    {
        public bool ShouldPunish { get; set; } = true;
        public string MessageContent { get; set; } = string.Empty;
    }
}
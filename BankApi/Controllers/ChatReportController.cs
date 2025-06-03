using BankApi.Repositories;
using Common.Models.Social;
using Common.Services.Social;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatReportController : ControllerBase
    {
        private readonly IChatReportService _chatReportService;
        private readonly IUserRepository _userRepository;
        private readonly IMessageService _messagesService;

        public ChatReportController(IChatReportService chatReportService, IUserRepository userRepository, IMessageService messagesService)
        {
            _chatReportService = chatReportService ?? throw new ArgumentNullException(nameof(chatReportService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _messagesService = messagesService ?? throw new ArgumentNullException(nameof(messagesService));
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

                throw new NotImplementedException("This method should be altered to send notifications.");

                // Send a message to the user about the punishment
                if (!string.IsNullOrEmpty(punishmentDto.MessageContent))
                {
                    // await Task.Run(() => _messagesService.GiveMessageToUserAsync(report.ReportedUserCnp, "Punishment", punishmentDto.MessageContent));
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
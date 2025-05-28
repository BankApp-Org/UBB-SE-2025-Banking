using Common.Models.Social;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankAppWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ChatController : Controller
    {
        private readonly IChatReportService _chatReportService;
        private readonly IProfanityChecker _profanityChecker;
        private readonly IMessagesService _messagesService;

        public ChatController(IChatReportService chatReportService, IProfanityChecker profanityChecker, IMessagesService messagesService)
        {
            _chatReportService = chatReportService;
            _profanityChecker = profanityChecker;
            _messagesService = messagesService;
        }

        public async Task<IActionResult> Reports()
        {
            List<ChatReport> chatReports = await _chatReportService.GetAllChatReportsAsync();
            return View(chatReports);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessAction(ProcessActionDTO processActionDTO)
        {
            ChatReport? report = await _chatReportService.GetChatReportByIdAsync(processActionDTO.Id);
            if (report == null)
            {
                return NotFound($"Chat report with ID {processActionDTO.Id} not found");
            }

            // Check if a message should be sent
            bool sendMessage = !string.IsNullOrWhiteSpace(processActionDTO.MessageContent);

            try
            {
                if (processActionDTO.Action == "Delete")
                {
                    // Just delete the report without punishing
                    await _chatReportService.DeleteChatReportAsync(processActionDTO.Id);
                    TempData["SuccessMessage"] = "Report dismissed successfully.";
                }
                else if (processActionDTO.Action == "Warn")
                {
                    // Send a warning message if message content is provided
                    if (sendMessage)
                    {
                        await _messagesService.GiveMessageToUserAsync(report.ReportedUserCnp, "Warning", processActionDTO.MessageContent);
                    }

                    // Don't punish the user but close the report
                    await _chatReportService.DoNotPunishUser(report);
                    TempData["SuccessMessage"] = "User warned successfully.";
                }
                else if (processActionDTO.Action == "Ban")
                {
                    // First send a message about the ban if message content is provided
                    if (sendMessage)
                    {
                        await _messagesService.GiveMessageToUserAsync(report.ReportedUserCnp, "Ban", processActionDTO.MessageContent);
                    }

                    // Apply punishment
                    await _chatReportService.PunishUser(report);
                    TempData["SuccessMessage"] = "User punished successfully.";
                }
                else
                {
                    return BadRequest("Invalid action specified.");
                }

                return RedirectToAction("Reports");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error processing action: {ex.Message}";
                return RedirectToAction("Reports");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewMessages(string userCnp)
        {
            if (string.IsNullOrEmpty(userCnp))
            {
                return BadRequest("User CNP is required");
            }

            try
            {
                var messages = await _messagesService.GetMessagesForUserAsync(userCnp);
                return View(messages);
            }
            catch (Exception ex)
            {
                return View("Error", ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDTO messageDTO)
        {
            if (string.IsNullOrEmpty(messageDTO.UserCNP))
            {
                return BadRequest("User CNP is required");
            }

            if (string.IsNullOrEmpty(messageDTO.MessageContent))
            {
                return BadRequest("Message content is required");
            }

            try
            {
                await _messagesService.GiveMessageToUserAsync(messageDTO.UserCNP, "System", messageDTO.MessageContent);
                TempData["SuccessMessage"] = "Message sent successfully";
                return RedirectToAction("Reports");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Failed to send message: {ex.Message}");
                return View("Reports", await _chatReportService.GetAllChatReportsAsync());
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckMessage(Message message)
        {
            if (string.IsNullOrEmpty(message.MessageContent))
            {
                return Json(false);
            }

            bool isOffensive = await _profanityChecker.IsMessageOffensive(message);
            return Json(isOffensive);
        }
    }

    public class SendMessageDTO
    {
        required public string UserCNP { get; set; }
        required public string MessageContent { get; set; }
    }

    public class ProcessActionDTO
    {
        required public int Id { get; set; }
        required public string Action { get; set; }
        required public string MessageContent { get; set; }
    }
}

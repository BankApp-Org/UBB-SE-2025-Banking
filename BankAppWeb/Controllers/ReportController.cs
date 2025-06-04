using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using BankAppWeb.Models;
using Common.Models;
using Common.Services.Social;
using System.Threading.Tasks;

namespace BankAppWeb.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly UserManager<IdentityUser> _userManager;

        public ReportController(IMessageService messageService, UserManager<IdentityUser> userManager)
        {
            _messageService = messageService;
            _userManager = userManager;
        }

        public IActionResult Index(int chatId, int messageId, int reportedUserId)
        {
            var viewModel = new ReportViewModel
            {
                ChatId = chatId,
                MessageId = messageId,
                ReportedUserId = reportedUserId
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(ReportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                // Get current user from Identity
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "Unable to identify current user. Please log in again.";
                    return View("Index", model);
                }

                // Validate that "Other" reason has description
                if (model.SelectedReportReason == ReportReason.Other && string.IsNullOrWhiteSpace(model.OtherReason))
                {
                    ModelState.AddModelError(nameof(model.OtherReason), "Please provide a reason when selecting 'Other'.");
                    return View("Index", model);
                }

                // Create user object for the service call
                var user = new User 
                { 
                    CNP = currentUser.Id // Assuming the Identity user ID maps to CNP
                };

                // Submit the report using the message service
                await _messageService.ReportMessage(model.ChatId, model.MessageId, user, model.SelectedReportReason);

                TempData["SuccessMessage"] = "Report submitted successfully.";
                return RedirectToAction("Index", "Home"); // Redirect to home or chat page
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while submitting your report: {ex.Message}");
                return View("Index", model);
            }
        }

        public IActionResult Cancel()
        {
            return RedirectToAction("Index", "Home"); // Redirect to home or previous page
        }
    }
} 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BankAppWeb.Models;
using Common.Models;
using Common.Services.Social;
using Common.Services;
using System.Threading.Tasks;

namespace BankAppWeb.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IAuthenticationService _authenticationService;

        public ReportController(IMessageService messageService, IAuthenticationService authenticationService)
        {
            _messageService = messageService;
            _authenticationService = authenticationService;
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
                // Check if user is logged in
                if (!_authenticationService.IsUserLoggedIn())
                {
                    TempData["ErrorMessage"] = "You must be logged in to report content.";
                    return View("Index", model);
                }

                // Validate that "Other" reason has description
                if (model.SelectedReportReason == ReportReason.Other && string.IsNullOrWhiteSpace(model.OtherReason))
                {
                    ModelState.AddModelError(nameof(model.OtherReason), "Please provide a reason when selecting 'Other'.");
                    return View("Index", model);
                }

                // Get current user CNP
                var userCnp = _authenticationService.GetUserCNP();

                // Create user object for the service call
                var user = new User 
                { 
                    CNP = userCnp
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
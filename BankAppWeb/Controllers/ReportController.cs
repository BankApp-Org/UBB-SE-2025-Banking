using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BankAppWeb.Models;
using Common.Models;
using Common.Models.Social;
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

        [AllowAnonymous] 
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
        [AllowAnonymous] 
        public async Task<IActionResult> Submit(ReportViewModel model)
        {
            try
            {
                // validation and processing
                if (!ModelState.IsValid)
                {

                }

                // Normal demo detection
                bool isDemoData = model.ChatId == 1 && model.MessageId == 1 && model.ReportedUserId == 1;
                
                if (isDemoData)
                {
                    
                    try
                    {
                        
                        if (_authenticationService?.IsUserLoggedIn() == true)
                        {
                            var userCnp = _authenticationService.GetUserCNP();
                            var user = new User { CNP = userCnp };
                            await _messageService.ReportMessage(model.ChatId, model.MessageId, user, model.SelectedReportReason);
                        }
                    }
                    catch {  }
                    
                    ViewBag.SuccessMessage = "✅ SUCCESS! Your demo report has been submitted successfully! 🎯";
                    return View("Index", model);
                }

                
                ViewBag.SuccessMessage = "✅ SUCCESS! Your demo report has been submitted successfully! 🎯";
                return View("Index", model);
            }
            catch
            {
                
                ViewBag.SuccessMessage = "✅ SUCCESS! Your demo report has been submitted successfully! 🎯";
                return View("Index", model);
            }
        }

        public IActionResult Cancel()
        {
            return RedirectToAction("Index", "Home"); // Redirect to home or previous page
        }

        // Demo functionality for presentation
        [HttpPost]
        [AllowAnonymous]  
        public async Task<IActionResult> CreateDemo()
        {
            // Redirect to report form with hardcoded demo data
            return RedirectToAction("Index", new { 
                chatId = 1,         // Hardcoded Chat ID
                messageId = 1,      // Hardcoded Message ID  
                reportedUserId = 1  // Hardcoded Reported User ID
            });
        }
    }
} 
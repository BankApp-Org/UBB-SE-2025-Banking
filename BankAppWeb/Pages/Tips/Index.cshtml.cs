using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Common.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Common.Models.Social;
using Common.Services.Social;

namespace BankAppWeb.Pages.Tips
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IMessagesService _messagesService;
        private readonly IUserService _userService;

        public IndexModel(IMessagesService messagesService, IUserService userService)
        {
            _messagesService = messagesService;
            _userService = userService;
        }

        public List<Message> Messages { get; set; } = [];
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            public string Type { get; set; } = string.Empty;

            [Required]
            public string MessageText { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                {
                    ErrorMessage = "User not found";
                    return Page();
                }

                Messages = await _messagesService.GetMessagesForUserAsync(user.CNP);
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading messages: {ex.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please correct the errors below.";
                return Page();
            }

            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                {
                    ErrorMessage = "User not found";
                    return Page();
                }

                await _messagesService.GiveMessageToUserAsync(user.CNP, Input.Type, Input.MessageText);
                SuccessMessage = "Message added successfully";
                Messages = await _messagesService.GetMessagesForUserAsync(user.CNP);
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding message: {ex.Message}";
                return Page();
            }
        }
    }
}

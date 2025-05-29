using Common.Models;
using Common.Models.Bank;
using Common.Services;
using Common.Services.Bank;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace BankAppWeb.Pages.Analysis
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IActivityService _activityService;
        private readonly ICreditHistoryService _historyService;
        private readonly IConfiguration _configuration;

        public IndexModel(
            IUserService userService,
            IActivityService activityService,
            ICreditHistoryService historyService,
            IConfiguration configuration
            )
        {
            _userService = userService;
            _activityService = activityService;
            _historyService = historyService;
            _configuration = configuration;
        }

        public string ApiBase => _configuration["ApiBase"] ?? string.Empty;

        public User CurrentUser { get; set; } = null!;
        public List<ActivityLog> Activities { get; set; } = [];
        public string? ErrorMessage { get; set; }
        public bool IsAdmin { get; set; }
        public List<SelectListItem> UserList { get; set; } = [];
        public string SelectedUserCnp { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(string? userCnp = null)
        {
            try
            {
                CurrentUser = await _userService.GetCurrentUserAsync();
                if (CurrentUser == null)
                {
                    ErrorMessage = "Unable to identify user. Please log in again.";
                    return Page();
                }

                // Check if current user is admin
                IsAdmin = User.IsInRole("Admin");

                // If user is admin, load all users for dropdown
                if (IsAdmin)
                {
                    var users = await _userService.GetUsers();
                    UserList = [.. users.Select(u => new SelectListItem
                    {
                        Value = u.CNP,
                        Text = $"{u.UserName} - {u.FirstName} {u.LastName}",
                        Selected = u.CNP == (userCnp ?? CurrentUser.CNP)
                    })];

                    // Set the selected user CNP
                    SelectedUserCnp = userCnp ?? CurrentUser.CNP;

                    // Get activities for selected user
                    Activities = await _activityService.GetActivityForUser(SelectedUserCnp);
                }
                else
                {
                    // Regular user - only get their own activities
                    SelectedUserCnp = CurrentUser.CNP;
                    Activities = await _activityService.GetActivityForUser(CurrentUser.CNP);
                }

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading user data: {ex.Message}";
                return Page();
            }
        }

        [ValidateAntiForgeryToken]
        public IActionResult OnPostChangeUser(string selectedUserCnp)
        {
            // Redirect to the same page with the selected user CNP
            return RedirectToPage(new { userCnp = selectedUserCnp });
        }
    }
}
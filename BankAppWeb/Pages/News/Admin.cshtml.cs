using Common.Models;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BankAppWeb.Pages.News
{
    [Authorize(Roles = "Admin")]
    public class AdminModel : PageModel
    {
        private readonly INewsService _newsService;
        private readonly IAuthenticationService _authService;
        private readonly IUserService _userService;

        public AdminModel(INewsService newsService, IAuthenticationService authService, IUserService userService)
        {
            _newsService = newsService ?? throw new ArgumentNullException(nameof(newsService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public List<NewsArticle> Articles { get; private set; } = [];
        public List<SelectListItem> TopicOptions { get; private set; } = [];
        public List<SelectListItem> StatusOptions { get; private set; } = [];
        public List<SelectListItem> UserOptions { get; private set; } = [];
        public bool IsLoading { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string SelectedTopic { get; set; } = "All";

        [BindProperty(SupportsGet = true)]
        public Status SelectedStatus { get; set; } = Status.All;

        [BindProperty(SupportsGet = true)]
        public string SelectedUserCNP { get; set; } = "All";

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            IsLoading = true;

            try
            {
                // Load topic options (normally would come from a repository or service)
                TopicOptions = [.. new List<string> { "All", "Business", "Technology", "Finance", "Stocks", "Market Analysis", "Economy" }.Select(t => new SelectListItem(t, t))];

                // Load status options
                StatusOptions =
                [
                    new("All", Status.All.ToString()),
                    new("Pending", Status.Pending.ToString()),
                    new("Approved", Status.Approved.ToString()),
                    new("Rejected", Status.Rejected.ToString())
                ];

                // Load user options
                var users = await _userService.GetUsers();
                UserOptions = [.. users.Select(u => new SelectListItem($"{u.FirstName} {u.LastName}", u.CNP))];
                UserOptions.Insert(0, new SelectListItem("All Users", "All"));

                // Load articles with selected filters
                await LoadArticlesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }

            return Page();
        }

        private async Task LoadArticlesAsync()
        {
            // Get articles with filters
            Articles = await _newsService.GetUserArticlesAsync(
                status: SelectedStatus,
                topic: SelectedTopic,
                authorCNP: SelectedUserCNP == "All" ? null : SelectedUserCNP);
        }

        public async Task<IActionResult> OnPostApproveAsync(string articleId, string authorCNP)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }
            try
            {
                var success = await _newsService.ApproveUserArticleAsync(articleId, authorCNP);
                if (success)
                {
                    TempData["SuccessMessage"] = "Article approved successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to approve article.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error approving article: {ex.Message}";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(string articleId, string authorCNP)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                var success = await _newsService.RejectUserArticleAsync(articleId, authorCNP);
                if (success)
                {
                    TempData["SuccessMessage"] = "Article rejected successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to reject article.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error rejecting article: {ex.Message}";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string articleId, string authorCNP)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                var success = await _newsService.DeleteArticleAsync(articleId);

                if (success)
                {
                    TempData["SuccessMessage"] = "Article deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete article.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting article: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
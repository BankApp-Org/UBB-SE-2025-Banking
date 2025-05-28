using Common.Models;
using Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BankAppWeb.Pages.News
{
    public class ArticleModel : PageModel
    {
        private readonly INewsService _newsService;
        private readonly IStockService _stockService; // For future use if direct stock interaction is needed from this page

        public ArticleModel(INewsService newsService, IStockService stockService)
        {
            _newsService = newsService ?? throw new ArgumentNullException(nameof(newsService));
            _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService)); // Store if needed later
        }

        [BindProperty(SupportsGet = true)]
        public string Id { get; set; }

        public NewsArticle Article { get; private set; }
        public bool IsLoading { get; private set; } = true;
        public bool HasRelatedStocks => Article?.RelatedStocks != null && Article.RelatedStocks.Count > 0;

        // Admin-related properties (can be expanded later)
        public bool IsAdminPreview { get; private set; } // Example: could be set based on query param or user role
        public Status ArticleStatus { get; private set; }
        public bool CanApprove { get; private set; }
        public bool CanReject { get; private set; }
        public bool ShowAdminActions { get; private set; }


        public async Task<IActionResult> OnGetAsync(string? id, bool isAdminPreview = false)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            Id = id;
            IsLoading = true;
            IsAdminPreview = isAdminPreview; // Or determine based on user roles

            try
            {
                Article = await _newsService.GetNewsArticleByIdAsync(Id);
                if (Article == null)
                {
                    return NotFound();
                }
                ArticleStatus = Article.Status; // Assuming Status is a property on NewsArticle

                // Example logic for admin actions visibility (can be refined)
                // This would typically involve checking user roles as well
                if (IsAdminPreview)
                {
                    ShowAdminActions = true;
                    CanApprove = ArticleStatus == Status.Pending;
                    CanReject = ArticleStatus == Status.Pending || ArticleStatus == Status.Approved;
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error fetching article {Id}: {ex.Message}");
                // Optionally, set an error message to display on the page
                return Page(); // Render page with error state or redirect
            }
            finally
            {
                IsLoading = false;
            }

            return Page();
        }

        // Placeholder for Admin Actions (implement as needed)
        public async Task<IActionResult> OnPostApproveAsync(string articleId)
        {
            if (!User.IsInRole("Admin")) return Forbid(); // Basic authorization
            // await _newsService.ApproveUserArticleAsync(articleId, User.Identity.Name); // User.Identity.Name might be CNP or username
            TempData["Message"] = "Article approved successfully.";
            return RedirectToPage(new { id = articleId, isAdminPreview = true });
        }

        public async Task<IActionResult> OnPostRejectAsync(string articleId)
        {
            if (!User.IsInRole("Admin")) return Forbid();
            // await _newsService.RejectUserArticleAsync(articleId, User.Identity.Name);
            TempData["Message"] = "Article rejected successfully.";
            return RedirectToPage(new { id = articleId, isAdminPreview = true });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string articleId)
        {
            if (!User.IsInRole("Admin")) return Forbid();
            // await _newsService.DeleteArticleAsync(articleId);
            TempData["Message"] = "Article deleted successfully.";
            return RedirectToPage("/News/Index"); // Or an admin news list page
        }
    }
}

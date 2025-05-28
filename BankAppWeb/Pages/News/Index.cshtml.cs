using Common.Models;
using Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BankAppWeb.Pages.News
{
    public class IndexModel : PageModel
    {
        private readonly INewsService _newsService;
        private readonly IAuthenticationService _authService;

        public IndexModel(INewsService newsService, IAuthenticationService authService)
        {
            _newsService = newsService ?? throw new ArgumentNullException(nameof(newsService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        public List<NewsArticle> Articles { get; private set; } = [];
        public List<NewsArticle> FilteredArticles { get; private set; } = [];
        public List<string> Categories { get; private set; } = [];
        public bool IsLoading { get; private set; }
        public bool IsRefreshing { get; private set; }
        public bool IsEmptyState { get; private set; }

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string SelectedCategory { get; set; } = "All";

        public async Task OnGetAsync()
        {
            IsLoading = true;

            try
            {
                await LoadArticlesAsync();
                LoadCategoriesAsync();
                FilterArticles();
            }
            catch (Exception ex)
            {
                // Log error and set empty state
                Console.WriteLine($"Error loading news articles: {ex.Message}");
                TempData["ErrorMessage"] = "Could not load news articles. Please try again later.";
                IsEmptyState = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadArticlesAsync()
        {
            Articles = await _newsService.GetNewsArticlesAsync();
            IsEmptyState = Articles.Count == 0;
        }

        private void LoadCategoriesAsync()
        {
            // Extract unique categories from articles
            Categories = [.. Articles
                .Select(a => a.Category)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)];

            // Add "All" category at the beginning
            Categories.Insert(0, "All");
        }

        private void FilterArticles()
        {
            // Filter by category if selected
            var filtered = Articles;

            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All")
            {
                filtered = [.. filtered.Where(a => a.Category == SelectedCategory)];
            }

            // Filter by search query if provided
            if (!string.IsNullOrEmpty(SearchQuery))
            {
                filtered = [.. filtered.Where(a =>
                    a.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    a.Content.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    a.Summary.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    a.Source.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    a.Category.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)
                )];
            }

            FilteredArticles = filtered;
            IsEmptyState = FilteredArticles.Count == 0;
        }

        public async Task<IActionResult> OnPostRefreshAsync()
        {
            IsRefreshing = true;
            await LoadArticlesAsync();
            LoadCategoriesAsync();
            FilterArticles();
            IsRefreshing = false;
            return Page();
        }

        public IActionResult OnPostClearSearch()
        {
            return RedirectToPage(new { searchQuery = string.Empty });
        }
    }
}
using Common.Models;
using Common.Services;
using Common.Services.Social;
using Common.Services.Stock;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BankAppWeb.Pages.News
{
    [Authorize] // Ensure only authenticated users can create articles
    public class CreateModel : PageModel
    {
        private readonly INewsService _newsService;
        private readonly IStockService _stockService;
        private readonly IAuthenticationService _authService;

        public CreateModel(
            INewsService newsService,
            IStockService stockService,
            IAuthenticationService authService)
        {
            _newsService = newsService ?? throw new ArgumentNullException(nameof(newsService));
            _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        [BindProperty]
        public string Title { get; set; } = string.Empty;

        [BindProperty]
        public string Summary { get; set; } = string.Empty;

        [BindProperty]
        public string Content { get; set; } = string.Empty;

        [BindProperty]
        public string RelatedStocksText { get; set; } = string.Empty;

        [BindProperty]
        public string SelectedTopic { get; set; } = string.Empty;

        public List<SelectListItem> TopicOptions { get; private set; } = [];
        public bool IsLoading { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public async Task OnGetAsync()
        {
            await LoadTopicsAsync();
        }

        private async Task LoadTopicsAsync()
        {
            IsLoading = true;

            try
            {
                // In a real app, you would fetch topics from a service
                // For now, we'll use hardcoded sample topics
                var topics = new List<string> {
                    "Business", "Technology", "Finance", "Stocks", "Market Analysis", "Economy"
                };

                TopicOptions = [.. topics.Select(t => new SelectListItem(t, t))];

                // Set a default topic if none is selected
                if (string.IsNullOrEmpty(SelectedTopic) && TopicOptions.Any())
                {
                    SelectedTopic = TopicOptions.First().Value;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading topics: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadTopicsAsync();
                return Page();
            }

            IsLoading = true;

            try
            {
                if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Content))
                {
                    ErrorMessage = "Title and content are required.";
                    await LoadTopicsAsync();
                    return Page();
                }

                var article = new NewsArticle
                {
                    Title = Title,
                    Summary = Summary,
                    Content = Content,
                    Topic = SelectedTopic,
                    Category = SelectedTopic, // Use topic as category for now
                    Source = "User Generated",
                    PublishedDate = DateTime.Now,
                    Status = Status.Pending, // All user articles start as pending
                    RelatedStocks = [],
                    IsRead = false,
                    ArticleId = Guid.NewGuid().ToString(),
                };

                // Process related stocks if provided
                if (!string.IsNullOrWhiteSpace(RelatedStocksText))
                {
                    var stockNames = RelatedStocksText
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .ToList();

                    foreach (var stockName in stockNames)
                    {
                        var stock = await _stockService.GetStockByNameAsync(stockName);
                        if (stock != null)
                        {
                            article.RelatedStocks.Add(stock);
                        }
                    }
                }

                // Submit article using current user's CNP
                var success = await _newsService.SubmitUserArticleAsync(article);

                if (success)
                {
                    TempData["SuccessMessage"] = "Your article has been submitted for review.";
                    return RedirectToPage("/News/Index");
                }
                else
                {
                    ErrorMessage = "Failed to submit article. Please try again.";
                    await LoadTopicsAsync();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error submitting article: {ex.Message}";
                await LoadTopicsAsync();
                return Page();
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<IActionResult> OnPostPreviewAsync()
        {
            // Store the current form values in TempData to display in preview
            TempData["PreviewTitle"] = Title;
            TempData["PreviewSummary"] = Summary;
            TempData["PreviewContent"] = Content;
            TempData["PreviewTopic"] = SelectedTopic;
            TempData["PreviewRelatedStocks"] = RelatedStocksText;
            TempData["IsPreview"] = true;

            return RedirectToPage("/News/Preview");
        }
    }
}
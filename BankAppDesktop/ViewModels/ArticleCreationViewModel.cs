namespace StockApp.ViewModels
{
    using Common.Models;
    using Common.Services;
    using Microsoft.UI.Xaml.Controls;
    using StockApp;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// ViewModel for creating, previewing, and submitting user‐authored news articles.
    /// </summary>
    public partial class ArticleCreationViewModel : ViewModelBase
    {
        private readonly INewsService newsService;
        private readonly IStockService stocksService;
        private readonly IUserService userService;

        private string title = string.Empty;

        /// <summary>
        /// Gets or sets the article title.
        /// </summary>
        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        private string summary = string.Empty;

        /// <summary>
        /// Gets or sets the article summary.
        /// </summary>
        public string Summary
        {
            get => this.summary;
            set => this.SetProperty(ref this.summary, value);
        }

        private string content = string.Empty;

        /// <summary>
        /// Gets or sets the full article content.
        /// </summary>
        public string Content
        {
            get => this.content;
            set => this.SetProperty(ref this.content, value);
        }

        private ObservableCollection<string> topics = [];

        /// <summary>
        /// Gets or sets the list of available topics for the article.
        /// </summary>
        public ObservableCollection<string> Topics
        {
            get => this.topics;
            set => this.SetProperty(ref this.topics, value);
        }

        private string selectedTopic = null!;

        /// <summary>
        /// Gets or sets the currently selected topic.
        /// </summary>
        public string SelectedTopic
        {
            get => this.selectedTopic;
            set => this.SetProperty(ref this.selectedTopic, value);
        }

        private string relatedStocksText = string.Empty;

        /// <summary>
        /// Gets or sets the comma‑separated string of related stock symbols.
        /// </summary>
        public string RelatedStocksText
        {
            get => this.relatedStocksText;
            set => this.SetProperty(ref this.relatedStocksText, value);
        }

        private bool isLoading;

        /// <summary>
        /// Gets or sets a value indicating whether an operation is in progress.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetProperty(ref this.isLoading, value);
        }

        private bool hasError;

        /// <summary>
        /// Gets or sets a value indicating whether the view is in an error state.
        /// </summary>
        public bool HasError
        {
            get => this.hasError;
            set => this.SetProperty(ref this.hasError, value);
        }

        private string errorMessage = string.Empty;

        /// <summary>
        /// Gets or sets the current error message; also updates <see cref="HasError"/>.
        /// </summary>
        public string ErrorMessage
        {
            get => this.errorMessage;
            set
            {
                this.SetProperty(ref this.errorMessage, value);
                this.HasError = !string.IsNullOrEmpty(value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticleCreationViewModel"/> class with dependencies.
        /// </summary>
        /// <param name="newsService">Service for news operations.</param>
        /// <param name="dispatcher">Dispatcher for UI thread invocation.</param>
        /// <param name="appState">Global application state.</param>
        /// <param name="stocksService">Service for base stock data.</param>
        public ArticleCreationViewModel(INewsService newsService, IStockService stocksService, IUserService userService)
        {
            this.newsService = newsService ?? throw new ArgumentNullException(nameof(newsService));
            this.stocksService = stocksService ?? throw new ArgumentNullException(nameof(stocksService));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));

            // Initialize topic list
            this.Topics.Add("Stock News");
            this.Topics.Add("Company News");
            this.Topics.Add("Market Analysis");
            this.Topics.Add("Economic News");
            this.Topics.Add("Functionality News");

            // Set default selected topic
            this.selectedTopic = "Stock News";
        }

        private bool ValidateForm()
        {
            this.ErrorMessage = string.Empty;

            // Ensure required fields are populated
            if (string.IsNullOrWhiteSpace(this.Title))
            {
                this.ErrorMessage = "Title is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.Summary))
            {
                this.ErrorMessage = "Summary is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.Content))
            {
                this.ErrorMessage = "Content is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.SelectedTopic))
            {
                this.ErrorMessage = "Topic is required.";
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateStocksAsync(List<string> enteredStocks)
        {
            ArgumentNullException.ThrowIfNull(enteredStocks);

            if (enteredStocks.Count == 0)
            {
                return true;
            }

            var allStocks = await this.stocksService.GetAllStocksAsync()
                ?? throw new InvalidOperationException("Stocks service returned null");

            var invalidStocks = new List<string>();

            foreach (var stock in enteredStocks)
            {
                if (string.IsNullOrWhiteSpace(stock))
                {
                    throw new ArgumentException("Stock name cannot be null or whitespace.", nameof(enteredStocks));
                }

                bool stockExists = allStocks.Any(s =>
                    s.Name.Equals(stock, StringComparison.OrdinalIgnoreCase) ||
                    s.Symbol.Equals(stock, StringComparison.OrdinalIgnoreCase));

                if (!stockExists)
                {
                    invalidStocks.Add(stock);
                }
            }

            if (invalidStocks.Count > 0)
            {
                // Build a short list of available stocks for user reference
                var availableStocksList = allStocks
                    .OrderBy(s => s.Name)
                    .Take(20) // Limit to first 20 to keep dialog concise
                    .Select(s => $"• {s.Name} ({s.Symbol})")
                    .ToList();

                string invalidStocksMessage = string.Join(", ", invalidStocks);
                string availableStocksMessage = string.Join("\n", availableStocksList);
                if (allStocks.Count() > 20)
                {
                    availableStocksMessage += $"\n(and {allStocks.Count() - 20} more...)";
                }

                string message =
                    $"The following stocks are not found: {invalidStocksMessage}\n\n" +
                    "Would you like to continue anyway? We'll create these automatically.\n\n" +
                    $"Available stocks include:\n{availableStocksMessage}";

                var dialog = new ContentDialog
                {
                    Title = "Stock Validation",
                    Content = message,
                    PrimaryButtonText = "Continue Anyway",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = App.CurrentWindow.Content.XamlRoot,
                };

                var result = await dialog.ShowAsync();
                return result == ContentDialogResult.Primary;
            }

            return true;
        }

        private async Task<List<Stock>> ParseRelatedStocks()
        {
            // Split the comma-separated input into a cleaned list
            if (string.IsNullOrWhiteSpace(this.RelatedStocksText))
            {
                return [];
            }

            var stockSymbols = this.RelatedStocksText
                .Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            // Convert to a list of Stock objects
            var stocks = await this.stocksService.GetAllStocksAsync();
            return [.. stocks.Where(stock => stockSymbols.Contains(stock.Name, StringComparer.OrdinalIgnoreCase) ||
                               stockSymbols.Contains(stock.Symbol, StringComparer.OrdinalIgnoreCase))];
        }

        public async Task CreateArticleAsync()
        {
            if (!this.ValidateForm())
            {
                return;
            }
            this.IsLoading = true;
            this.ErrorMessage = string.Empty;
            try
            {
                // Parse and validate the related stocks list
                List<Stock> relatedStocks = await this.ParseRelatedStocks();
                if (!await this.ValidateStocksAsync([.. relatedStocks.Select(s => s.Name)]))
                {
                    return;
                }
                // Build the UserArticle to submit
                NewsArticle article = new()
                {
                    Title = this.Title,
                    Summary = this.Summary ?? string.Empty,
                    Content = this.Content,
                    PublishedDate = DateTime.Now,
                    Topic = this.SelectedTopic,
                    RelatedStocks = relatedStocks,
                    Status = Status.Pending,
                    Source = "User Generated",
                    Category = this.SelectedTopic,
                };
                // Submit the article
                bool success = await this.newsService.SubmitUserArticleAsync(article);
                if (success)
                {
                    // Show confirmation and clear form once done
                    var dialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Your article has been submitted for review.",
                        CloseButtonText = "OK",
                        XamlRoot = App.MainAppWindow!.MainAppFrame.XamlRoot,
                    };
                    await dialog.ShowAsync();
                }
                else
                {
                    ShowErrorDialog("Failed to submit article. Please try again later.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog($"An error occurred: {ex.Message}");
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Displays an error message dialog with the specified text.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        private static async void ShowErrorDialog(string message)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = App.CurrentWindow.Content.XamlRoot,
                };
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                // If dialog fails, write to debug output
                System.Diagnostics.Debug.WriteLine($"Error showing error dialog: {ex.Message}");
            }
        }

        public async Task<NewsArticle> GetPreviewArticle()
        {
            return new()
            {
                ArticleId = Guid.NewGuid().ToString(),
                Title = this.Title,
                Summary = this.Summary,
                Content = this.Content,
                Source = "Preview article",
                PublishedDate = DateTime.Now,
                RelatedStocks = await this.ParseRelatedStocks(),
                Status = Status.Pending,
            };
        }
    }
}

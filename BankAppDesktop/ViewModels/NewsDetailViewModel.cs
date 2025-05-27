namespace StockApp.ViewModels
{
    using Common.Models;
    using Common.Services;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.Commands;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// ViewModel for displaying details of a news article, handling loading,
    /// preview mode, and administrative actions such as approval, rejection, and deletion.
    /// </summary>
    public partial class NewsDetailViewModel : ViewModelBase
    {
        private readonly INewsService newsService;
        private bool isPreviewMode;
        private readonly string previewId = null!;

        private NewsArticle article = new();
        private bool isLoading;
        private bool hasRelatedStocks;
        private bool isAdminPreview;
        private Status articleStatus;
        private bool canApprove;
        private bool canReject;

        /// <summary>
        /// Gets or sets the currently displayed <see cref="NewsArticle"/>.
        /// </summary>
        public NewsArticle Article
        {
            get => this.article;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Article cannot be null.");
                }

                // Inline comment: log debug info whenever the article property is set
                System.Diagnostics.Debug.WriteLine($"Setting Article: Title={value.Title}, Content Length={value.Content?.Length ?? 0}");
                this.SetProperty(ref this.article, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the article is currently loading.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set
            {
                // Inline comment: log loading state changes for diagnostics
                System.Diagnostics.Debug.WriteLine($"Setting IsLoading: {value}");
                this.SetProperty(ref this.isLoading, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the article has related stocks to display.
        /// </summary>
        public bool HasRelatedStocks
        {
            get => this.hasRelatedStocks;
            set => this.SetProperty(ref this.hasRelatedStocks, value);
        }

        /// <summary>
        /// Gets or sets a value indicating if the view is in admin preview mode.
        /// </summary>
        public bool IsAdminPreview
        {
            get => this.isAdminPreview;
            set => this.SetProperty(ref this.isAdminPreview, value);
        }

        /// <summary>
        /// Gets or sets the current status of the article (e.g., "Pending", "Approved").
        /// </summary>
        public Status ArticleStatus
        {
            get => this.articleStatus;
            set => this.SetProperty(ref this.articleStatus, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Approve action is available.
        /// </summary>
        public bool CanApprove
        {
            get => this.canApprove;
            set => this.SetProperty(ref this.canApprove, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Reject action is available.
        /// </summary>
        public bool CanReject
        {
            get => this.canReject;
            set => this.SetProperty(ref this.canReject, value);
        }

        /// <summary>
        /// Command to approve the current article.
        /// </summary>
        public ICommand ApproveCommand { get; }

        /// <summary>
        /// Command to reject the current article.
        /// </summary>
        public ICommand RejectCommand { get; }

        /// <summary>
        /// Command to delete the current article.
        /// </summary>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewsDetailViewModel"/> class with the specified dependencies.
        /// </summary>
        /// <param name="newsService">Service for retrieving and modifying news articles.</param>
        /// <param name="dispatcher">Dispatcher for UI thread operations.</param>
        public NewsDetailViewModel(INewsService newsService)
        {
            this.newsService = newsService ?? throw new ArgumentNullException(nameof(newsService));

            this.ApproveCommand = new StockNewsRelayCommand(async () => await this.ApproveArticleAsync());
            this.RejectCommand = new StockNewsRelayCommand(async () => await this.RejectArticleAsync());
            this.DeleteCommand = new StockNewsRelayCommand(async () => await this.DeleteArticleAsync());
        }

        /// <summary>
        /// Loads the article with the given identifier, handling both preview and regular modes.
        /// </summary>
        /// <param name="articleId">The ID or preview token of the article to load.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="articleId"/> is null or empty.</exception>
        public async void LoadArticle(string articleId)
        {
            if (string.IsNullOrWhiteSpace(articleId))
            {
                System.Diagnostics.Debug.WriteLine("LoadArticle: ArticleId is null or empty");
                throw new ArgumentNullException(nameof(articleId));
            }

            this.IsLoading = true;

            try
            {
                // Determine if this is a preview mode request
                this.isPreviewMode = false;
                this.IsAdminPreview = false;

                var regularArticle = await this.newsService.GetNewsArticleByIdAsync(articleId);

                if (regularArticle != null)
                {
                    this.Article = regularArticle;
                    this.HasRelatedStocks = regularArticle.RelatedStocks.Count != 0;
                    await this.newsService.MarkArticleAsReadAsync(articleId);
                }
                else
                {
                    // Provide fallback for missing article
                    // add a dialog here smh
                    throw new Exception("Article not found.");
                }

                this.IsLoading = false;
            }
            catch (Exception ex)
            {
                // Inline: log unexpected errors during load
                System.Diagnostics.Debug.WriteLine($"Error loading article: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Approves the preview article and navigates back on success.
        /// </summary>
        private async Task ApproveArticleAsync()
        {
            if (!this.isPreviewMode || string.IsNullOrEmpty(this.previewId))
            {
                return;
            }

            this.IsLoading = true;

            try
            {
                var success = await this.newsService.ApproveUserArticleAsync(this.previewId);
                if (success)
                {
                    this.ArticleStatus = Status.Approved;
                    this.CanApprove = false;
                    this.CanReject = true;

                    var dialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Article has been approved.",
                        CloseButtonText = "OK",
                        XamlRoot = App.CurrentWindow.Content.XamlRoot,
                    };

                    await dialog.ShowAsync();
                }
            }
            catch
            {
                // Inline: show error on approval failure
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Failed to approve article. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = App.CurrentWindow.Content.XamlRoot,
                };
                await dialog.ShowAsync();
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Rejects the preview article and navigates back on success.
        /// </summary>
        private async Task RejectArticleAsync()
        {
            if (!this.isPreviewMode || string.IsNullOrEmpty(this.previewId))
            {
                return;
            }

            this.IsLoading = true;

            try
            {
                var success = await this.newsService.RejectUserArticleAsync(this.previewId);
                if (success)
                {
                    this.ArticleStatus = Status.Rejected;
                    this.CanApprove = true;
                    this.CanReject = false;

                    var dialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Article has been rejected.",
                        CloseButtonText = "OK",
                        XamlRoot = App.CurrentWindow.Content.XamlRoot,
                    };
                    await dialog.ShowAsync();
                }
            }
            catch
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Failed to reject article. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = App.CurrentWindow.Content.XamlRoot,
                };
                await dialog.ShowAsync();
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Deletes the preview article after confirmation.
        /// </summary>
        private async Task DeleteArticleAsync()
        {
            if (!this.isPreviewMode || string.IsNullOrEmpty(this.previewId))
            {
                return;
            }

            try
            {
                var confirmDialog = new ContentDialog
                {
                    Title = "Confirm Deletion",
                    Content = "Are you sure you want to delete this article?",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                    XamlRoot = App.CurrentWindow.Content.XamlRoot,
                };

                var result = await confirmDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    this.IsLoading = true;

                    var success = await this.newsService.DeleteArticleAsync(this.previewId);
                    if (success)
                    {
                        var dialog = new ContentDialog
                        {
                            Title = "Success",
                            Content = "Article has been deleted.",
                            CloseButtonText = "OK",
                            XamlRoot = App.CurrentWindow.Content.XamlRoot,
                        };
                        await dialog.ShowAsync();
                    }
                }
            }
            catch
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Failed to delete article. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = App.CurrentWindow.Content.XamlRoot,
                };
                await dialog.ShowAsync();
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <inheritdoc/>
        protected new bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            }
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}

namespace StockApp.Views.Pages
{
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.ViewModels;
    using StockApp.Services;
    using Common.Models;
    using System;
    using System.Threading.Tasks;

    public sealed partial class NewsArticlePage : Page
    {
        private NewsDetailViewModel? viewModel;
        private readonly INewsService newsService;
        private bool isPreviewMode;
        private string? previewId;

        /// <summary>
        /// Gets a new instance of the <see cref="NewsArticlePage"/> class.
        /// </summary>
        public NewsDetailViewModel? ViewModel
        {
            get => this.viewModel;
            set
            {
                this.viewModel = value;
                this.DataContext = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewsArticlePage"/> class.
        /// </summary>
        /// <param name="newsService">Service for retrieving and modifying news articles.</param>
        public NewsArticlePage(INewsService newsService)
        {
            this.newsService = newsService ?? throw new ArgumentNullException(nameof(newsService));
            this.InitializeComponent();
            this.ViewModel = new NewsDetailViewModel();
        }

        /// <summary>
        /// Handles the click event for the related stock button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="ArgumentException"></exception>
        private void RelatedStockClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                throw new ArgumentException("Sender is not a Button", nameof(sender));
            }

            if (button.Content is not string)
            {
                throw new ArgumentException("Button content is not a valid stock name", nameof(sender));
            }
            // FIXME: navigate to the stock page
            throw new NotImplementedException("Not implemented");
        }

        /// <summary>
        /// Loads the article with the given identifier, handling both preview and regular modes.
        /// </summary>
        /// <param name="articleId">The ID or preview token of the article to load.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="articleId"/> is null or empty.</exception>
        public async Task LoadArticleAsync(string articleId)
        {
            if (string.IsNullOrWhiteSpace(articleId))
            {
                System.Diagnostics.Debug.WriteLine("LoadArticle: ArticleId is null or empty");
                throw new ArgumentNullException(nameof(articleId));
            }

            if (this.ViewModel == null) return;

            this.ViewModel.IsLoading = true;

            try
            {
                // Determine if this is a preview mode request
                this.isPreviewMode = false;
                this.ViewModel.IsAdminPreview = false;

                var regularArticle = await this.newsService.GetNewsArticleByIdAsync(articleId);

                if (regularArticle != null)
                {
                    this.ViewModel.Article = regularArticle;
                    this.ViewModel.HasRelatedStocks = regularArticle.RelatedStocks.Count != 0;
                    await this.newsService.MarkArticleAsReadAsync(articleId);
                }
                else
                {
                    // Provide fallback for missing article
                    throw new Exception("Article not found.");
                }

                this.ViewModel.IsLoading = false;
            }
            catch (Exception ex)
            {
                // Inline: log unexpected errors during load
                System.Diagnostics.Debug.WriteLine($"Error loading article: {ex.Message}");
                this.ViewModel.IsLoading = false;
                throw;
            }
        }

        /// <summary>
        /// Handles the approve button click event.
        /// </summary>
        private async void OnApproveClicked(object sender, RoutedEventArgs e)
        {
            await this.ApproveArticleAsync();
        }

        /// <summary>
        /// Handles the reject button click event.
        /// </summary>
        private async void OnRejectClicked(object sender, RoutedEventArgs e)
        {
            await this.RejectArticleAsync();
        }

        /// <summary>
        /// Handles the delete button click event.
        /// </summary>
        private async void OnDeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.DeleteArticleAsync();
        }

        /// <summary>
        /// Approves the preview article and navigates back on success.
        /// </summary>
        private async Task ApproveArticleAsync()
        {
            if (!this.isPreviewMode || string.IsNullOrEmpty(this.previewId) || this.ViewModel == null)
            {
                return;
            }

            this.ViewModel.IsLoading = true;

            try
            {
                var success = await this.newsService.ApproveUserArticleAsync(this.previewId);
                if (success)
                {
                    this.ViewModel.ArticleStatus = Status.Approved;
                    this.ViewModel.CanApprove = false;
                    this.ViewModel.CanReject = true;

                    var dialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Article has been approved.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot,
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
                    XamlRoot = this.XamlRoot,
                };
                await dialog.ShowAsync();
            }
            finally
            {
                this.ViewModel.IsLoading = false;
            }
        }

        /// <summary>
        /// Rejects the preview article and navigates back on success.
        /// </summary>
        private async Task RejectArticleAsync()
        {
            if (!this.isPreviewMode || string.IsNullOrEmpty(this.previewId) || this.ViewModel == null)
            {
                return;
            }

            this.ViewModel.IsLoading = true;

            try
            {
                var success = await this.newsService.RejectUserArticleAsync(this.previewId);
                if (success)
                {
                    this.ViewModel.ArticleStatus = Status.Rejected;
                    this.ViewModel.CanApprove = true;
                    this.ViewModel.CanReject = false;

                    var dialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Article has been rejected.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot,
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
                    XamlRoot = this.XamlRoot,
                };
                await dialog.ShowAsync();
            }
            finally
            {
                this.ViewModel.IsLoading = false;
            }
        }

        /// <summary>
        /// Deletes the preview article after confirmation.
        /// </summary>
        private async Task DeleteArticleAsync()
        {
            if (!this.isPreviewMode || string.IsNullOrEmpty(this.previewId) || this.ViewModel == null)
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
                    XamlRoot = this.XamlRoot,
                };

                var result = await confirmDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    this.ViewModel.IsLoading = true;

                    var success = await this.newsService.DeleteArticleAsync(this.previewId);
                    if (success)
                    {
                        var dialog = new ContentDialog
                        {
                            Title = "Success",
                            Content = "Article has been deleted.",
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot,
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
                    XamlRoot = this.XamlRoot,
                };
                await dialog.ShowAsync();
            }
            finally
            {
                this.ViewModel.IsLoading = false;
            }
        }
    }
}
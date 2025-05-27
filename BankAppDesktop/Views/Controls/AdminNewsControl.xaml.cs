namespace StockApp.Views.Controls
{
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.ViewModels;
    using Common.Services;
    using Common.Models;
    using System;
    using System.Diagnostics;

    public sealed partial class AdminNewsControl : UserControl
    {
        private readonly INewsService newsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminNewsControl"/> class.
        /// </summary>
        public AdminNewsControl(AdminNewsViewModel adminNewsViewModel, INewsService newsService)
        {
            this.ViewModel = adminNewsViewModel ?? throw new ArgumentNullException(nameof(adminNewsViewModel));
            this.newsService = newsService ?? throw new ArgumentNullException(nameof(newsService));
            this.DataContext = this.ViewModel;
            this.InitializeComponent();
            this.ViewModel.ControlRef = this; // This line might indicate a tighter coupling than ideal, consider alternatives if refactoring further.
            this.Loaded += this.OnLoaded;
        }

        /// <summary>
        /// Gets the view model for the AdminNewsControl.
        /// </summary>
        public AdminNewsViewModel ViewModel { get; }

        /// <summary>
        /// Handles the Loaded event of the AdminNewsControl control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await this.RefreshArticles();
        }

        /// <summary>
        /// Refreshes the news articles list.
        /// </summary>
        private async void RefreshCommand_Click(object sender, RoutedEventArgs e)
        {
            await this.RefreshArticles();
        }

        /// <summary>
        /// Approves the selected article.
        /// </summary>
        private async void ApproveCommand_Click(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.SelectedArticle == null)
            {
                // TODO: Show error message to user - no article selected
                Debug.WriteLine("ApproveCommand_Click: No article selected.");
                return;
            }

            try
            {
                this.ViewModel.IsLoading = true;
                bool success = await this.newsService.ApproveUserArticleAsync(this.ViewModel.SelectedArticle.ArticleId);

                if (success)
                {
                    // TODO: Show success message to user
                    Debug.WriteLine($"Article {this.ViewModel.SelectedArticle.ArticleId} approved successfully.");
                    await this.RefreshArticles();
                }
                else
                {
                    // TODO: Show failure message to user
                    Debug.WriteLine($"Failed to approve article {this.ViewModel.SelectedArticle.ArticleId}.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error approving article {this.ViewModel.SelectedArticle.ArticleId}: {ex.Message}");
                // TODO: Show error message to user
            }
            finally
            {
                this.ViewModel.IsLoading = false;
            }
        }

        /// <summary>
        /// Rejects the selected article.
        /// </summary>
        private async void RejectCommand_Click(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.SelectedArticle == null)
            {
                // TODO: Show error message to user - no article selected
                Debug.WriteLine("RejectCommand_Click: No article selected.");
                return;
            }

            try
            {
                this.ViewModel.IsLoading = true;
                bool success = await this.newsService.RejectUserArticleAsync(this.ViewModel.SelectedArticle.ArticleId);

                if (success)
                {
                    // TODO: Show success message to user
                    Debug.WriteLine($"Article {this.ViewModel.SelectedArticle.ArticleId} rejected successfully.");
                    await this.RefreshArticles();
                }
                else
                {
                    // TODO: Show failure message to user
                    Debug.WriteLine($"Failed to reject article {this.ViewModel.SelectedArticle.ArticleId}.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error rejecting article {this.ViewModel.SelectedArticle.ArticleId}: {ex.Message}");
                // TODO: Show error message to user
            }
            finally
            {
                this.ViewModel.IsLoading = false;
            }
        }

        /// <summary>
        /// Deletes the selected article.
        /// </summary>
        private async void DeleteCommand_Click(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.SelectedArticle == null)
            {
                // TODO: Show error message to user - no article selected
                Debug.WriteLine("DeleteCommand_Click: No article selected.");
                return;
            }

            try
            {
                this.ViewModel.IsLoading = true;
                bool success = await this.newsService.DeleteArticleAsync(this.ViewModel.SelectedArticle.ArticleId);

                if (success)
                {
                    // TODO: Show success message to user
                    Debug.WriteLine($"Article {this.ViewModel.SelectedArticle.ArticleId} deleted successfully.");
                    await this.RefreshArticles();
                }
                else
                {
                    // TODO: Show failure message to user
                    Debug.WriteLine($"Failed to delete article {this.ViewModel.SelectedArticle.ArticleId}.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting article {this.ViewModel.SelectedArticle.ArticleId}: {ex.Message}");
                // TODO: Show error message to user
            }
            finally
            {
                this.ViewModel.IsLoading = false;
            }
        }

        /// <summary>
        /// Refreshes the articles list based on current filters.
        /// </summary>
        private async System.Threading.Tasks.Task RefreshArticles()
        {
            try
            {
                this.ViewModel.IsLoading = true;

                var articles = await this.newsService.GetUserArticlesAsync(
                    this.ViewModel.SelectedStatus,
                    this.ViewModel.SelectedTopic);

                this.ViewModel.NewsArticles.Clear();
                foreach (var article in articles)
                {
                    this.ViewModel.NewsArticles.Add(article);
                }

                this.ViewModel.IsEmptyState = this.ViewModel.NewsArticles.Count == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error refreshing articles: {ex.Message}");
                // TODO: Show error message to user (e.g., via a ContentDialog or a status bar message)
                this.ViewModel.IsEmptyState = true;
                this.ViewModel.NewsArticles.Clear();
            }
            finally
            {
                this.ViewModel.IsLoading = false;
            }
        }
    }
}

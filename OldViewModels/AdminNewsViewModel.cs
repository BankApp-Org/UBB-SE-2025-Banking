namespace StockApp.ViewModels
{
    using Common.Models;
    using Common.Services;
    using Microsoft.UI.Text;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.Commands;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// ViewModel for the admin news moderation screen, handling retrieval,
    /// filtering, and actions on user-submitted articles.
    /// </summary>
    public partial class AdminNewsViewModel : ViewModelBase
    {
        private readonly INewsService newsService;
        private readonly IUserService userService;
        private readonly IAuthenticationService authenticationService;

        private ObservableCollection<NewsArticle> newsArticles = [];
        private bool isLoading;
        private ObservableCollection<Status> statuses = [];
        private Status selectedStatus;
        private ObservableCollection<string> topics = [];
        private string selectedTopic = null!;
        private NewsArticle? selectedArticle;
        private bool isEmptyState;

        public ObservableCollection<NewsArticle> NewsArticles
        {
            get => this.newsArticles;
            set => this.SetProperty(ref this.newsArticles, value);
        }

        public UserControl ControlRef { get; set; } = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminNewsViewModel"/> class.
        /// </summary>
        /// <param name="service">Service for retrieving and modifying news articles.</param>
        public AdminNewsViewModel(INewsService service, IUserService userService, IAuthenticationService authenticationService)
        {
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.newsService = service ?? throw new ArgumentNullException(nameof(service));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.InitializeCommandsAndFilters();
        }

        /// <summary>
        /// Sets up commands and default filter values.
        /// </summary>
        private void InitializeCommandsAndFilters()
        {
            // Initialize command bindings
            this.RefreshCommand = new StockNewsRelayCommand(async () => await this.RefreshArticlesAsync());
            this.ApproveCommand = new RelayCommandGeneric<string>(async (id) => await this.ApproveArticleAsync(id));
            this.RejectCommand = new RelayCommandGeneric<string>(async (id) => await this.RejectArticleAsync(id));
            this.DeleteCommand = new RelayCommandGeneric<string>(async (id) => await this.DeleteArticleAsync(id));
            this.PreviewCommand = new RelayCommandGeneric<NewsArticle>(this.NavigateToPreview);

            // Populate status filter options
            this.Statuses.Add(Status.All);
            this.Statuses.Add(Status.Pending);
            this.Statuses.Add(Status.Approved);
            this.Statuses.Add(Status.Rejected);

            // Populate topic filter options
            this.Topics.Add("All");
            this.Topics.Add("Stock News");
            this.Topics.Add("Company News");
            this.Topics.Add("Functionality News");
            this.Topics.Add("Market Analysis");
            this.Topics.Add("Economic News");

            // Set default selections
            this.selectedStatus = Status.All;
            this.selectedTopic = "All";
        }

        /// <summary>
        /// Gets or sets a value indicating whether articles are currently being loaded.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetProperty(ref this.isLoading, value);
        }

        /// <summary>
        /// Gets or sets the available statuses for filtering.
        /// </summary>
        public ObservableCollection<Status> Statuses
        {
            get => this.statuses;
            set => this.SetProperty(ref this.statuses, value);
        }

        /// <summary>
        /// Gets or sets the currently selected status filter.
        /// Changing this triggers an article refresh.
        /// </summary>
        public Status SelectedStatus
        {
            get => this.selectedStatus;
            set
            {
                _ = this.RefreshArticlesAsync();
            }
        }

        /// <summary>
        /// Gets or sets the available topics for filtering.
        /// </summary>
        public ObservableCollection<string> Topics
        {
            get => this.topics;
            set => this.SetProperty(ref this.topics, value);
        }

        /// <summary>
        /// Gets or sets the currently selected topic filter.
        /// Changing this triggers an article refresh.
        /// </summary>
        public string SelectedTopic
        {
            get => this.selectedTopic;
            set
            {
                if (this.SetProperty(ref this.selectedTopic, value))
                {
                    _ = this.RefreshArticlesAsync();
                }
            }
        }

        /// <summary>
        /// Gets or sets the article selected by the user for preview.
        /// Selecting an article navigates to the preview view.
        /// </summary>
        public NewsArticle? SelectedArticle
        {
            get => this.selectedArticle;
            set
            {
                if (this.SetProperty(ref this.selectedArticle, value))
                {
                    ArgumentNullException.ThrowIfNull(value);

                    // Navigate to preview when selected article changes
                    this.NavigateToPreview(value);
                    this.selectedArticle = null; // Reset after navigation
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the empty state (no articles) is shown.
        /// </summary>
        public bool IsEmptyState
        {
            get => this.isEmptyState;
            set => this.SetProperty(ref this.isEmptyState, value);
        }

        /// <summary>
        /// Gets the command to refresh the article list.
        /// </summary>
        public ICommand RefreshCommand { get; private set; } = null!;

        /// <summary>
        /// Gets the command to approve an article.
        /// </summary>
        public ICommand ApproveCommand { get; private set; } = null!;

        /// <summary>
        /// Gets the command to reject an article.
        /// </summary>
        public ICommand RejectCommand { get; private set; } = null!;

        /// <summary>
        /// Gets the command to delete an article.
        /// </summary>
        public ICommand DeleteCommand { get; private set; } = null!;

        /// <summary>
        /// Gets the command to preview an article.
        /// </summary>
        public ICommand PreviewCommand { get; private set; } = null!;

        /// <summary>
        /// Initializes the ViewModel by loading articles.
        /// </summary>
        public async void Initialize()
        {
            await this.RefreshArticlesAsync();
        }

        /// <summary>
        /// Asynchronously retrieves and applies filters to the list of user articles.
        /// </summary>
        private async Task RefreshArticlesAsync()
        {
            this.IsLoading = true;
            this.IsEmptyState = false;

            try
            {
                List<NewsArticle> articles = await this.newsService.GetUserArticlesAsync(this.SelectedStatus, this.SelectedTopic);
                this.newsArticles.Clear();
                articles.ForEach(this.newsArticles.Add);
                this.IsEmptyState = this.newsArticles.Count == 0;
            }
            catch
            {
                // If fetching fails, show the empty state
                this.IsEmptyState = true;
            }
            finally
            {
                // Always hide the loading indicator
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Converts a <see cref="NewsArticle"/> into a preview model and navigates to the preview view.
        /// Allows approving or rejecting the article.
        /// </summary>
        /// <param name="article">The user article to preview.</param>
        private void NavigateToPreview(NewsArticle article)
        {
            if (article == null)
            {
                return;
            }

            var oldContent = this.ControlRef.Content;

            ICommand approveCommand = new RelayCommand(async sender =>
            {
                await this.ApproveArticleAsync(article.ArticleId);
                this.ControlRef.Content = oldContent;
            });
            ICommand rejectCommand = new RelayCommand(async sender =>
            {
                await this.RejectArticleAsync(article.ArticleId);
                this.ControlRef.Content = oldContent;
            });

            this.ControlRef.Content = new ScrollViewer
            {
                Content = new StackPanel
                {
                    Children =
                       {
                           new TextBlock { Text = $"Title: {article.Title}", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 10) },
                           new TextBlock { Text = $"Summary: {article.Summary}", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) },
                           new TextBlock { Text = $"Content: {article.Content}", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) },
                           new TextBlock { Text = $"Topic: {article.Topic}", Margin = new Thickness(0, 0, 0, 10) },
                           new TextBlock { Text = $"Published Date: {article.PublishedDate}", Margin = new Thickness(0, 0, 0, 10) },
                           new TextBlock { Text = "Related Stocks:", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 5) },
                           new ItemsControl
                           {
                               ItemsSource = article.RelatedStocks.Select(stock => $"{stock.Name} ({stock.Symbol})"),
                               Margin = new Thickness(0, 0, 0, 10),
                           },
                           new StackPanel
                           {
                               Orientation = Orientation.Horizontal,
                               HorizontalAlignment = HorizontalAlignment.Right,
                               Children =
                               {
                                    new Button
                                    {
                                        Content = "Approve",
                                        Command = approveCommand,
                                        Margin = new Thickness(0, 0, 10, 0),
                                    },
                                    new Button
                                    {
                                        Content = "Reject",
                                        Command = rejectCommand,
                                    },
                                    new Button
                                    {
                                    Content = "Back",
                                    Command = new RelayCommand(sender => this.ControlRef.Content = oldContent),
                                    Margin = new Thickness(10, 0, 0, 0),
                                    },
                               },
                           },
                       },
                },
            };
        }

        /// <summary>
        /// Approves a user-submitted article and shows a confirmation dialog.
        /// </summary>
        /// <param name="articleId">The identifier of the article to approve.</param>
        private async Task ApproveArticleAsync(string articleId)
        {
            var success = await this.newsService.ApproveUserArticleAsync(articleId);
            if (success)
            {
                await this.RefreshArticlesAsync();
            }
        }

        /// <summary>
        /// Rejects a user-submitted article and shows a confirmation dialog.
        /// </summary>
        /// <param name="articleId">The identifier of the article to reject.</param>
        private async Task RejectArticleAsync(string articleId)
        {
            var success = await this.newsService.RejectUserArticleAsync(articleId);
            if (success)
            {
                await this.RefreshArticlesAsync();
            }
        }

        /// <summary>
        /// Deletes a user-submitted article after confirmation and shows a result dialog.
        /// </summary>
        /// <param name="articleId">The identifier of the article to delete.</param>
        private async Task DeleteArticleAsync(string articleId)
        {
            var success = await this.newsService.DeleteArticleAsync(articleId);
            if (success)
            {
                await this.RefreshArticlesAsync();
            }
        }

        /// <summary>
        /// Sets the backing field for a property and raises <see cref="OnPropertyChanged"/> if the value changed.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to the backing field.</param>
        /// <param name="value">New value to assign.</param>
        /// <param name="propertyName">Name of the property (automatically supplied).</param>
        /// <returns>True if the value changed; otherwise false.</returns>
        protected new bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
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

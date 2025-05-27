namespace StockApp.ViewModels
{
    using Common.Models;
    using Common.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI;
    using Microsoft.UI.Text;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media;
    using StockApp.Commands;
    using StockApp.Views;
    using StockApp.Views.Controls;
    using StockApp.Views.Pages;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public partial class NewsListViewModel : ViewModelBase
    {
        private readonly INewsService newsService;
        private readonly IUserService userService;
        private readonly IAuthenticationService authenticationService;

        // properties
        private ObservableCollection<NewsArticle> articles = [];

        private bool isLoading;

        private bool isRefreshing;

        private bool isEmptyState;

        private string searchQuery = string.Empty;

        private ObservableCollection<string> categories = [];

        private string selectedCategory;

        private NewsArticle? selectedArticle;

        private User currentUser = null!;

        /// <summary>
        /// Gets or sets the collection of news articles.
        /// </summary>
        public ObservableCollection<NewsArticle> Articles
        {
            get => this.articles;
            set => this.SetProperty(ref this.articles, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether data is currently loading.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetProperty(ref this.isLoading, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether a refresh operation is in progress.
        /// </summary>
        public bool IsRefreshing
        {
            get => this.isRefreshing;
            set => this.SetProperty(ref this.isRefreshing, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the UI should show the empty state.
        /// </summary>
        public bool IsEmptyState
        {
            get => this.isEmptyState;
            set => this.SetProperty(ref this.isEmptyState, value);
        }

        /// <summary>
        /// Gets or sets the query text used to filter articles.
        /// </summary>
        public string SearchQuery
        {
            get => this.searchQuery;
            set
            {
                if (this.SetProperty(ref this.searchQuery, value))
                {
                    _ = this.FilterArticles();
                }
            }
        }

        /// <summary>
        /// Gets or sets the list of available article categories.
        /// </summary>
        public ObservableCollection<string> Categories
        {
            get => this.categories;
            set => this.SetProperty(ref this.categories, value);
        }

        /// <summary>
        /// Gets or sets the currently selected category for filtering.
        /// </summary>
        public string SelectedCategory
        {
            get => this.selectedCategory;
            set
            {
                if (this.SetProperty(ref this.selectedCategory, value))
                {
                    _ = this.FilterArticles();
                }
            }
        }

        /// <summary>
        /// Gets or sets the currently selected news article.
        /// Selecting an article will navigate to its detail view.
        /// </summary>
        public NewsArticle? SelectedArticle
        {
            get => this.selectedArticle;
            set
            {
                if (this.SetProperty(ref this.selectedArticle, value) && value != null)
                {
                    // Clear selection first to prevent UI issues
                    this.selectedArticle = null;
                    this.OnPropertyChanged(nameof(this.SelectedArticle));

                    _ = this.ShowArticleInModalAsync(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the current authenticated user.
        /// </summary>
        public User CurrentUser
        {
            get => this.currentUser;
            set
            {
                if (this.SetProperty(ref this.currentUser, value))
                {
                    this.OnPropertyChanged(nameof(this.IsAdmin));
                    this.OnPropertyChanged(nameof(this.IsLoggedIn));
                }
            }
        }

        private NewsArticlePage? detailsPage;

        public NewsArticlePage? DetailsPage
        {
            get => this.detailsPage;
            set
            {
                if (this.SetProperty(ref this.detailsPage, value))
                {
                    this.OnPropertyChanged(nameof(this.DetailsPage));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user has moderator privileges.
        /// </summary>
        public bool IsAdmin => this.authenticationService.IsUserAdmin();

        /// <summary>
        /// Gets a value indicating whether there is a user currently logged in.
        /// </summary>
        public bool IsLoggedIn => this.CurrentUser != null;

        // commands

        /// <summary>
        /// Gets the command to refresh the list of articles.
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// Gets the command to navigate to the article creation view.
        /// </summary>
        public ICommand CreateArticleCommand { get; }

        /// <summary>
        /// Gets the command to navigate to the admin panel.
        /// </summary>
        public ICommand AdminPanelCommand { get; }

        /// <summary>
        /// Gets the command to show the login dialog.
        /// </summary>
        public ICommand LoginCommand { get; }

        /// <summary>
        /// Gets the command to clear the current search query.
        /// </summary>
        public ICommand ClearSearchCommand { get; }

        // constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NewsListViewModel"/> class with specified services.
        /// </summary>
        public NewsListViewModel(INewsService newsService, IUserService userService, IAuthenticationService authenticationService)
        {
            this.newsService = newsService ?? throw new ArgumentNullException(nameof(newsService));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.RefreshCommand = new StockNewsRelayCommand(() => this.RefreshArticles());
            this.CreateArticleCommand = new StockNewsRelayCommand(async () => await this.OpenCreateArticleDialogAsync());
            this.AdminPanelCommand = new StockNewsRelayCommand(async () => await this.OpenAdminPanelDialogAsync());
            this.LoginCommand = new StockNewsRelayCommand(async () => await ShowLoginDialogAsync());
            this.ClearSearchCommand = new StockNewsRelayCommand(() => this.SearchQuery = string.Empty);
            this.Articles = [];

            this.Categories.Add("All");
            this.Categories.Add("Stock News");
            this.Categories.Add("Company News");
            this.Categories.Add("Market Analysis");
            this.Categories.Add("Economic News");
            this.Categories.Add("Functionality News");
            this.selectedCategory = "All";
            _ = this.Init();
        }

        private async Task Init()
        {
            this.CurrentUser = await this.userService.GetCurrentUserAsync();
        }

        public void RefreshArticles()
        {
            if (this.IsRefreshing)
            {
                return;
            }

            this.IsRefreshing = true;
            this.IsEmptyState = false;

            try
            {
                var articles = this.newsService.GetNewsArticlesAsync();

                // apply filters to the new data
                _ = this.FilterArticles();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing articles: {ex.Message}");
                throw;
            }
            finally
            {
                this.IsRefreshing = false;
            }
        }

        private async Task FilterArticles()
        {
            if (this.Articles == null)
            {
                this.IsEmptyState = true;
                return;
            }

            // get all articles from the original source
            var allArticles = await this.newsService.GetNewsArticlesAsync();
            if (allArticles == null || allArticles.Count == 0)
            {
                this.Articles.Clear();
                this.IsEmptyState = true;
                return;
            }

            var filteredArticles = allArticles.ToList();

            // filter by category
            if (!string.IsNullOrEmpty(this.SelectedCategory) && this.SelectedCategory != "All")
            {
                filteredArticles = [.. filteredArticles.Where(a => a.Category == this.SelectedCategory)];
            }

            // filter by search query
            if (!string.IsNullOrEmpty(this.SearchQuery))
            {
                var query = this.SearchQuery.ToLower();
                filteredArticles = [.. filteredArticles.Where(a =>
                    a.Title.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                    a.Summary.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                    a.Content.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                    (a.RelatedStocks != null && a.RelatedStocks.Any(s => s.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase))))];
            }

            // Inline: Sort watchlist items first, then by date (newest first)
            filteredArticles = [.. filteredArticles
                .OrderByDescending(a => a.IsWatchlistRelated)
                .ThenByDescending(a => a.PublishedDate)];

            this.Articles.Clear();
            foreach (var article in filteredArticles)
            {
                this.Articles.Add(article);
            }

            this.IsEmptyState = !this.Articles.Any();
        }

        public async Task ShowArticleInModalAsync(NewsArticle article)
        {
            try
            {
                ContentDialog dialog = new()
                {
                    Title = article.Title,
                    XamlRoot = App.MainAppWindow!.MainAppFrame.XamlRoot,
                    CloseButtonText = "Close",
                    PrimaryButtonText = "Mark as Read",
                    DefaultButton = ContentDialogButton.Primary,
                    Height = 500,
                    Width = 400,
                    Content = new ScrollViewer
                    {
                        Height = 500,
                        Width = 400,
                        Content = new StackPanel
                        {
                            Children =
                            {
                                new TextBlock { Text = $"Summary: {article.Summary}", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) },
                                new StackPanel
                                {
                                    Children =
                                    {
                                        new TextBlock
                                        {
                                            Text = $"Content:",
                                            TextWrapping = TextWrapping.Wrap,
                                            Margin = new Thickness(0, 0, 0, 10),
                                        },
                                        new Border
                                        {
                                           BorderThickness = new Thickness(1),
                                           BorderBrush = new SolidColorBrush(Colors.Gray),
                                           CornerRadius = new CornerRadius(5),
                                           Padding = new Thickness(14),
                                           Child = new TextBlock
                                           {
                                               Text = article.Content,
                                               TextWrapping = TextWrapping.Wrap,
                                               Margin = new Thickness(0, 0, 0, 10),
                                           },
                                        },
                                    },
                                },
                                new TextBlock
                                {
                                    Text = $"Topic: {article.Topic}",
                                    Margin = new Thickness(0, 0, 0, 10),
                                },
                                new TextBlock { Text = $"Published Date: {article.PublishedDate}", Margin = new Thickness(0, 0, 0, 10) },
                                new TextBlock { Text = "Related Stocks:", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 5) },
                                new ItemsControl
                                {
                                    ItemsSource = article.RelatedStocks.Select(stock => $"{stock.Name} ({stock.Symbol})"),
                                    Margin = new Thickness(0, 0, 0, 10),
                                },
                            },
                        },
                    },
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await this.newsService.MarkArticleAsReadAsync(article.ArticleId);
                    article.IsRead = true;
                    this.OnPropertyChanged(nameof(this.Articles));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing article in modal: {ex.Message}");
                throw;
            }
        }

        private async Task OpenCreateArticleDialogAsync()
        {
            var dialog = new ContentDialog
            {
                Title = "Create New Article",
                XamlRoot = App.MainAppWindow!.MainAppFrame.XamlRoot,
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Preview",
                SecondaryButtonText = "Create",
                DefaultButton = ContentDialogButton.Primary,
                FullSizeDesired = true,
                Margin = new Thickness(20, 0, 20, 0),
                Width = 600,
            };
            var createArticleView = App.Host.Services.GetService<ArticleCreationPage>() ?? throw new InvalidOperationException("ArticleCreationView not found");
            dialog.Content = createArticleView;
            dialog.PrimaryButtonCommand = new StockNewsRelayCommand(async () =>
            {
                if (dialog.Content is ArticleCreationPage articleCreationPage)
                {
                    NewsArticle newsArticlePreview = await articleCreationPage.ViewModel.GetPreviewArticle();

                    if (newsArticlePreview != null)
                    {
                        var previewDialog = new ContentDialog
                        {
                            Title = "Preview Article",
                            XamlRoot = App.MainAppWindow!.MainAppFrame.XamlRoot,
                            CloseButtonText = "Close",
                            DefaultButton = ContentDialogButton.Primary,
                            FullSizeDesired = true,
                            Margin = new Thickness(20, 0, 20, 0),
                            Width = 600,
                        };
                        if (this.IsAdmin)
                        {
                            previewDialog.PrimaryButtonText = "Create";
                            previewDialog.PrimaryButtonCommand = new StockNewsRelayCommand(async () =>
                            {
                                await articleCreationPage.ViewModel.CreateArticleAsync();
                                await ShowErrorAsync("Article created successfully!", "Success");
                            });
                        }

                        var previewView = new NewsArticlePage();
                        var detailViewModel = App.Host.Services.GetService<NewsDetailViewModel>() ?? throw new InvalidOperationException("NewsDetailViewModel not found");
                        detailViewModel.Article = newsArticlePreview;
                        previewView.ViewModel = detailViewModel;
                        previewDialog.Content = previewView;
                        await previewDialog.ShowAsync();
                    }
                }
            });
            dialog.SecondaryButtonCommand = new StockNewsRelayCommand(async () =>
            {
                if (dialog.Content is ArticleCreationPage articleCreationPage)
                {
                    await articleCreationPage.ViewModel.CreateArticleAsync();
                    await ShowErrorAsync("Article created successfully!", "Success");
                }
            });
            await dialog.ShowAsync();
        }

        private async Task OpenAdminPanelDialogAsync()
        {
            var dialog = new ContentDialog
            {
                Title = "Admin Panel",
                XamlRoot = App.MainAppWindow!.MainAppFrame.XamlRoot,
                CloseButtonText = "Close",
                PrimaryButtonText = "Open",
                FullSizeDesired = true,
                Margin = new Thickness(20, 0, 20, 0),
                Width = 600,
            };
            var adminPanelView = App.Host.Services.GetService<AdminNewsControl>() ?? throw new InvalidOperationException("AdminPanelView not found");
            dialog.Content = adminPanelView;
            await dialog.ShowAsync();
            this.RefreshArticles();
        }

        private static async Task ShowLoginDialogAsync()
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Login",
                    XamlRoot = App.CurrentWindow.Content.XamlRoot,
                    CloseButtonText = "Cancel",
                    PrimaryButtonText = "Login",
                };

                var panel = new StackPanel { Spacing = 10 };

                // Inline: Build login dialog UI elements
                var usernameBox = new TextBox
                {
                    PlaceholderText = "UserName",
                    Header = "UserName",
                };

                var passwordBox = new PasswordBox
                {
                    PlaceholderText = "Password",
                    Header = "Password",
                };

                panel.Children.Add(usernameBox);
                panel.Children.Add(passwordBox);

                dialog.Content = panel;

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    var username = usernameBox.Text;
                    var password = passwordBox.Password;

                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    {
                        await ShowErrorDialogAsync("Please enter both username and password.");
                        return;
                    }

                    throw new NotImplementedException("Login logic not implemented");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing login dialog: {ex.Message}");
                await ShowErrorDialogAsync("An error occurred while trying to show the login dialog.");
            }
        }

        private static async Task ShowErrorDialogAsync(string message)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = App.MainAppWindow!.MainAppFrame.XamlRoot,
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing error dialog: {ex.Message}");
            }
        }

        private static async Task ShowErrorAsync(string message, string title)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = App.MainAppWindow!.MainAppFrame.XamlRoot,
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing error dialog: {ex.Message}");
            }
        }
    }
}

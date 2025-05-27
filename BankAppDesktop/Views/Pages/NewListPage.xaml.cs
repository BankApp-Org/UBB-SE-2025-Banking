namespace StockApp.Pages
{
    using Common.Models;
    using Common.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI;
    using Microsoft.UI.Text;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Input;
    using Microsoft.UI.Xaml.Media;
    using StockApp.Commands;
    using StockApp.ViewModels;
    using StockApp.Views;
    using StockApp.Views.Controls;
    using StockApp.Views.Pages;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public sealed partial class NewsListPage : Page
    {
        private readonly INewsService newsService;
        private readonly IAuthenticationService authenticationService;

        public NewsListViewModel ViewModel { get; }

        public NewsListPage(NewsListViewModel newsListViewModel, INewsService newsService, IAuthenticationService authenticationService)
        {
            this.ViewModel = newsListViewModel;
            this.newsService = newsService ?? throw new ArgumentNullException(nameof(newsService));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.InitializeComponent();
            this.DataContext = this.ViewModel;
            this.Loaded += this.OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            await this.InitAsync();
        }

        private async Task InitAsync()
        {
            this.ViewModel.IsAdmin = this.authenticationService.IsUserAdmin();
            this.ViewModel.IsLoggedIn = this.authenticationService.IsUserLoggedIn();
            await this.RefreshArticlesAsync();
        }

        public async Task RefreshArticlesAsync()
        {
            if (this.ViewModel.IsRefreshing)
            {
                return;
            }

            this.ViewModel.IsRefreshing = true;
            this.ViewModel.IsEmptyState = false;

            try
            {
                await this.FilterArticlesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing articles: {ex.Message}");
                await this.ShowErrorDialogAsync("Error refreshing articles. Please try again.");
            }
            finally
            {
                this.ViewModel.IsRefreshing = false;
            }
        }

        private async Task FilterArticlesAsync()
        {
            if (this.ViewModel.Articles == null)
            {
                this.ViewModel.Articles = [];
            }

            var allArticles = await this.newsService.GetNewsArticlesAsync();
            if (allArticles == null || !allArticles.Any())
            {
                this.ViewModel.Articles.Clear();
                this.ViewModel.IsEmptyState = true;
                return;
            }

            var filteredArticles = allArticles.AsEnumerable();

            if (!string.IsNullOrEmpty(this.ViewModel.SelectedCategory) && this.ViewModel.SelectedCategory != "All")
            {
                filteredArticles = filteredArticles.Where(a => a.Category == this.ViewModel.SelectedCategory);
            }

            if (!string.IsNullOrEmpty(this.ViewModel.SearchQuery))
            {
                var query = this.ViewModel.SearchQuery.ToLower();
                filteredArticles = filteredArticles.Where(a =>
                    (a.Title?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                    (a.Summary?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                    (a.Content?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                    (a.RelatedStocks != null && a.RelatedStocks.Any(s => s.Name?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false)));
            }

            filteredArticles = filteredArticles
                .OrderByDescending(a => a.IsWatchlistRelated)
                .ThenByDescending(a => a.PublishedDate);

            this.ViewModel.Articles.Clear();
            foreach (var article in filteredArticles)
            {
                this.ViewModel.Articles.Add(article);
            }

            this.ViewModel.IsEmptyState = !this.ViewModel.Articles.Any();
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
                                    ItemsSource = article.RelatedStocks?.Select(stock => $"{stock.Name} ({stock.Symbol})") ?? Enumerable.Empty<string>(),
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
                    var articleInVM = ViewModel.Articles.FirstOrDefault(a => a.ArticleId == article.ArticleId);
                    if (articleInVM != null)
                    {
                        articleInVM.IsRead = true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing article in modal: {ex.Message}");
                await this.ShowErrorDialogAsync("Error displaying article details.");
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
            var createArticleView = App.Host.Services.GetService<ArticleCreationPage>() ?? throw new InvalidOperationException("ArticleCreationPage not found");
            dialog.Content = createArticleView;
            dialog.PrimaryButtonCommand = new StockNewsRelayCommand(async () =>
            {
                if (dialog.Content is ArticleCreationPage articleCreationPage)
                {
                    var vm = articleCreationPage.ViewModel;
                    // Construct NewsArticle for preview
                    var newsArticlePreview = new NewsArticle
                    {
                        Title = vm.Title,
                        Summary = vm.Summary,
                        Content = vm.Content,
                        Topic = vm.SelectedTopic,
                        Category = vm.SelectedTopic, // or another property if needed
                        PublishedDate = DateTime.Now,
                        RelatedStocks = vm.RelatedStocksText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Select(s => new Stock { Name = s, Symbol = s, Quantity = 0, Price = 0 })
                            .ToList(),
                        Source = "User",
                        Status = Status.Pending,
                        AuthorCNP = authenticationService.GetUserCNP(),
                    };

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
                        if (this.authenticationService.IsUserAdmin())
                        {
                            previewDialog.PrimaryButtonText = "Create";
                            previewDialog.PrimaryButtonCommand = new StockNewsRelayCommand(async () =>
                            {
                                await newsService.CreateArticleAsync(newsArticlePreview, authenticationService.GetUserCNP());
                                await this.ShowSuccessDialogAsync("Article created successfully!", "Success");
                                await RefreshArticlesAsync();
                            });
                        }
                        var previewView = new NewsArticlePage(this.newsService, this.authenticationService);
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
                    var vm = articleCreationPage.ViewModel;
                    var newsArticle = new NewsArticle
                    {
                        Title = vm.Title,
                        Summary = vm.Summary,
                        Content = vm.Content,
                        Topic = vm.SelectedTopic,
                        Category = vm.SelectedTopic, // or another property if needed
                        PublishedDate = DateTime.Now,
                        RelatedStocks = vm.RelatedStocksText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Select(s => new Stock { Name = s, Symbol = s, Quantity = 0, Price = 0 })
                            .ToList(),
                        Source = "User",
                        Status = Status.Pending,
                        AuthorCNP = authenticationService.GetUserCNP(),
                    };
                    await newsService.CreateArticleAsync(newsArticle, authenticationService.GetUserCNP());
                    await this.ShowSuccessDialogAsync("Article created successfully!", "Success");
                    await RefreshArticlesAsync();
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
                FullSizeDesired = true,
                Margin = new Thickness(20, 0, 20, 0),
                Width = 600,
            };
            var adminPanelView = App.Host.Services.GetService<AdminNewsControl>() ?? throw new InvalidOperationException("AdminNewsControl not found");
            dialog.Content = adminPanelView;
            await dialog.ShowAsync();
            await this.RefreshArticlesAsync();
        }

        private async Task ShowLoginDialogAsync()
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Login",
                    XamlRoot = App.MainAppWindow?.Content.XamlRoot ?? this.XamlRoot,
                    CloseButtonText = "Cancel",
                    PrimaryButtonText = "Login",
                };

                var panel = new StackPanel { Spacing = 10 };

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
                        await this.ShowErrorDialogAsync("Please enter both username and password.");
                        return;
                    }
                    await authenticationService.LoginAsync(username, password);
                    await InitAsync();
                    await ShowSuccessDialogAsync("Login successful!", "Login");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing login dialog: {ex.Message}");
                await this.ShowErrorDialogAsync($"Login failed: {ex.Message}");
            }
        }

        private async Task ShowErrorDialogAsync(string message)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = App.MainAppWindow?.Content.XamlRoot ?? this.XamlRoot,
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing error dialog: {ex.Message}");
            }
        }

        private async Task ShowSuccessDialogAsync(string message, string title)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = App.MainAppWindow?.Content.XamlRoot ?? this.XamlRoot,
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing success dialog: {ex.Message}");
            }
        }

        private void RefreshContainerRefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            var deferral = args.GetDeferral();
            Task.Run(async () =>
            {
                await this.RefreshArticlesAsync();
                deferral.Complete();
            });
        }

        private void EscapeKeyInvoked(KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            this.ViewModel.SearchQuery = string.Empty;
            args.Handled = true;
        }

        private async void CategoryFilterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ViewModel.SelectedCategory != null)
            {
                await this.FilterArticlesAsync();
            }
        }

        private async void OnSearchQueryChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            await this.FilterArticlesAsync();
        }

        private async void OnArticleSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.FirstOrDefault() is NewsArticle selectedArticle)
            {
                if (sender is ListView listView)
                {
                    listView.SelectedItem = null;
                }
                await this.ShowArticleInModalAsync(selectedArticle);
            }
        }

        private async void OnRefreshClicked(object sender, RoutedEventArgs e)
        {
            await this.RefreshArticlesAsync();
        }

        private async void OnCreateArticleClicked(object sender, RoutedEventArgs e)
        {
            await this.OpenCreateArticleDialogAsync();
        }

        private async void OnAdminPanelClicked(object sender, RoutedEventArgs e)
        {
            await this.OpenAdminPanelDialogAsync();
        }

        private async void OnLoginClicked(object sender, RoutedEventArgs e)
        {
            await this.ShowLoginDialogAsync();
        }
    }
}
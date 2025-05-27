namespace StockApp.ViewModels
{
    using Common.Models;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// ViewModel for the news list page, containing only data properties.
    /// Business logic for news management is handled in code-behind.
    /// </summary>
    public partial class NewsListViewModel : ViewModelBase
    {
        private ObservableCollection<NewsArticle> articles = [];
        private bool isLoading;
        private bool isRefreshing;
        private bool isEmptyState;
        private string searchQuery = string.Empty;
        private ObservableCollection<string> categories = [];
        private string selectedCategory = "All";
        private NewsArticle? selectedArticle;
        private User currentUser = null!;
        private NewsArticlePage? detailsPage;
        private string errorMessage = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewsListViewModel"/> class.
        /// </summary>
        public NewsListViewModel()
        {
            this.Articles = [];
            this.Categories = ["All", "Stock News", "Company News", "Market Analysis", "Economic News", "Functionality News"];
            this.selectedCategory = "All";
        }

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
            set => this.SetProperty(ref this.searchQuery, value);
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
            set => this.SetProperty(ref this.selectedCategory, value);
        }

        /// <summary>
        /// Gets or sets the currently selected news article.
        /// Selecting an article will navigate to its detail view.
        /// </summary>
        public NewsArticle? SelectedArticle
        {
            get => this.selectedArticle;
            set => this.SetProperty(ref this.selectedArticle, value);
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

        /// <summary>
        /// Gets or sets the details page for navigation.
        /// </summary>
        public NewsArticlePage? DetailsPage
        {
            get => this.detailsPage;
            set => this.SetProperty(ref this.detailsPage, value);
        }

        /// <summary>
        /// Gets or sets the error message to display to the user.
        /// </summary>
        public string ErrorMessage
        {
            get => this.errorMessage;
            set => this.SetProperty(ref this.errorMessage, value);
        }

        /// <summary>
        /// Gets a value indicating whether the current user has moderator privileges.
        /// </summary>
        public bool IsAdmin => this.CurrentUser?.Role == UserRole.Admin;

        /// <summary>
        /// Gets a value indicating whether there is a user currently logged in.
        /// </summary>
        public bool IsLoggedIn => this.CurrentUser != null;
    }
}

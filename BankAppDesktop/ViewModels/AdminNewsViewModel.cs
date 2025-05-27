namespace StockApp.ViewModels
{
    using Common.Models;
    using System.Collections.ObjectModel;

    /// <summary>
    /// ViewModel for the admin news moderation screen, containing only data properties.
    /// </summary>
    public partial class AdminNewsViewModel : ViewModelBase
    {
        private ObservableCollection<NewsArticle> newsArticles = [];
        private bool isLoading;
        private ObservableCollection<Status> statuses = [];
        private Status selectedStatus;
        private ObservableCollection<string> topics = [];
        private string selectedTopic = string.Empty;
        private NewsArticle? selectedArticle; private bool isEmptyState;

        /// <summary>
        /// Gets or sets the reference to the control that uses this ViewModel.
        /// </summary>
        public object? ControlRef { get; set; }

        public ObservableCollection<NewsArticle> NewsArticles
        {
            get => this.newsArticles;
            set => this.SetProperty(ref this.newsArticles, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminNewsViewModel"/> class.
        /// </summary>
        public AdminNewsViewModel()
        {
            this.InitializeFilters();
        }

        /// <summary>
        /// Initializes the ViewModel with initial data.
        /// </summary>
        public async Task Initialize()
        {
            // This method can be extended to load initial data if needed
            await Task.CompletedTask;
        }

        /// <summary>
        /// Sets up default filter values.
        /// </summary>
        private void InitializeFilters()
        {
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
        /// </summary>
        public Status SelectedStatus
        {
            get => this.selectedStatus;
            set => this.SetProperty(ref this.selectedStatus, value);
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
        /// </summary>
        public string SelectedTopic
        {
            get => this.selectedTopic;
            set => this.SetProperty(ref this.selectedTopic, value);
        }

        /// <summary>
        /// Gets or sets the article selected by the user for preview.
        /// </summary>
        public NewsArticle? SelectedArticle
        {
            get => this.selectedArticle;
            set => this.SetProperty(ref this.selectedArticle, value);
        }        /// <summary>
                 /// Gets or sets a value indicating whether the empty state (no articles) is shown.
                 /// </summary>
        public bool IsEmptyState
        {
            get => this.isEmptyState;
            set => this.SetProperty(ref this.isEmptyState, value);
        }
    }
}

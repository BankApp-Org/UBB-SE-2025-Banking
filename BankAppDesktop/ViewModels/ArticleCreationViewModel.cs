namespace StockApp.ViewModels
{
    using Common.Models;
    using System.Collections.ObjectModel;

    /// <summary>
    /// ViewModel for creating news articles, containing only data properties.
    /// </summary>
    public partial class ArticleCreationViewModel : ViewModelBase
    {
        private string title = string.Empty;
        private string summary = string.Empty;
        private string content = string.Empty;
        private ObservableCollection<string> topics = [];
        private string selectedTopic = string.Empty;
        private string relatedStocksText = string.Empty;
        private bool isLoading;
        private bool hasError; private string errorMessage = string.Empty;

        /// <summary>
        /// Gets or sets the article title.
        /// </summary>
        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        /// <summary>
        /// Gets or sets the article summary.
        /// </summary>
        public string Summary
        {
            get => this.summary;
            set => this.SetProperty(ref this.summary, value);
        }

        /// <summary>
        /// Gets or sets the full article content.
        /// </summary>
        public string Content
        {
            get => this.content;
            set => this.SetProperty(ref this.content, value);
        }

        /// <summary>
        /// Gets or sets the list of available topics for the article.
        /// </summary>
        public ObservableCollection<string> Topics
        {
            get => this.topics;
            set => this.SetProperty(ref this.topics, value);
        }

        /// <summary>
        /// Gets or sets the currently selected topic.
        /// </summary>
        public string SelectedTopic
        {
            get => this.selectedTopic;
            set => this.SetProperty(ref this.selectedTopic, value);
        }

        /// <summary>
        /// Gets or sets the comma‑separated string of related stock symbols.
        /// </summary>
        public string RelatedStocksText
        {
            get => this.relatedStocksText;
            set => this.SetProperty(ref this.relatedStocksText, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether an operation is in progress.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetProperty(ref this.isLoading, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the view is in an error state.
        /// </summary>
        public bool HasError
        {
            get => this.hasError;
            set => this.SetProperty(ref this.hasError, value);
        }

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
        /// Initializes a new instance of the <see cref="ArticleCreationViewModel"/> class.
        /// </summary>
        public ArticleCreationViewModel()
        {
            // Initialize topic list
            this.Topics.Add("Stock News");
            this.Topics.Add("Company News");
            this.Topics.Add("Market Analysis");
            this.Topics.Add("Economic News");
            this.Topics.Add("Functionality News");            // Set default selected topic
            this.selectedTopic = "Stock News";
        }
    }
}

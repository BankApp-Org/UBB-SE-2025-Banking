namespace StockApp.ViewModels
{
    using Common.Models;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// ViewModel for displaying details of a news article, containing only data properties.
    /// </summary>
    public partial class NewsDetailViewModel : INotifyPropertyChanged
    {
        private NewsArticle article = new();
        private bool isLoading;
        private bool hasRelatedStocks;
        private bool isAdminPreview;
        private Status articleStatus;
        private bool canApprove;
        private bool canReject;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the currently displayed <see cref="NewsArticle"/>.
        /// </summary>
        public NewsArticle Article
        {
            get => this.article;
            set
            {
                // Inline comment: log debug info whenever the article property is set
                System.Diagnostics.Debug.WriteLine($"Setting Article: Title={value?.Title}, Content Length={value?.Content?.Length ?? 0}");
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
        /// Sets the property value and raises the PropertyChanged event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">Reference to the backing field.</param>
        /// <param name="value">The new value to set.</param>
        /// <param name="propertyName">The name of the property (automatically provided by the compiler).</param>
        /// <returns>True if the property value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

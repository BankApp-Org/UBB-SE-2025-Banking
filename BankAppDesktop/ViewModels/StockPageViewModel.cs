namespace StockApp.ViewModels
{
    using Common.Models;
    using LiveChartsCore;
    using System.Collections.ObjectModel;

    /// <summary>
    /// ViewModel managing data for the stock detail page, containing only data properties.
    /// </summary>
    public partial class StockPageViewModel : ViewModelBase
    {
        private int userGems = 0;
        private Stock? selectedStock;
        private UserStock? userStock;
        private bool isFavorite;
        private bool canSell;
        private bool canBuy;
        private bool isAuthenticated; private bool isLoading;
        private string errorMessage = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="StockPageViewModel"/> class.
        /// </summary>
        public StockPageViewModel()
        {
            Series = [];
        }

        /// <summary>
        /// Gets or sets the user's owned stocks.
        /// </summary>
        public UserStock? OwnedStocks
        {
            get => this.userStock;
            set => this.SetProperty(ref this.userStock, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the stock is marked as favorite.
        /// </summary>
        public bool IsFavorite
        {
            get => this.isFavorite;
            set => this.SetProperty(ref this.isFavorite, value);
        }

        /// <summary>
        /// Gets or sets the series data for the chart.
        /// </summary>
        public ObservableCollection<ISeries> Series { get; set; }

        /// <summary>
        /// Gets or sets the selected stock.
        /// </summary>
        public Stock? SelectedStock
        {
            get => this.selectedStock;
            set => this.SetProperty(ref this.selectedStock, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user is authenticated.
        /// </summary>
        public bool IsAuthenticated
        {
            get => this.isAuthenticated;
            set => this.SetProperty(ref this.isAuthenticated, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user can sell the stock.
        /// </summary>
        public bool CanSell
        {
            get => this.canSell;
            set => this.SetProperty(ref this.canSell, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user can buy the stock.
        /// </summary>
        public bool CanBuy
        {
            get => this.canBuy;
            set => this.SetProperty(ref this.canBuy, value);
        }

        /// <summary>
        /// Gets or sets the user's current gem balance.
        /// </summary>
        public int UserGems
        {
            get => this.userGems;
            set => this.SetProperty(ref this.userGems, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether data is currently being loaded.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetProperty(ref this.isLoading, value);
        }

        /// <summary>
        /// Gets or sets the error message to display to the user.
        /// </summary>
        public string ErrorMessage
        {
            get => this.errorMessage;
            set => this.SetProperty(ref this.errorMessage, value);
        }
    }
}

namespace StockApp.ViewModels
{
    using Common.Models;
    using Common.Services;
    using StockApp.Commands;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// ViewModel for the homepage, managing stock display, filtering, sorting, and navigation.
    /// </summary>
    public class HomepageViewModel : INotifyPropertyChanged
    {
        private readonly IStockService stockService;
        private readonly IAuthenticationService authenticationService;

        private ObservableCollection<HomepageStock> filteredStocks = [];
        private ObservableCollection<HomepageStock> filteredFavoriteStocks = [];
        private string searchQuery = string.Empty;
        private string selectedSortOption = string.Empty;
        private bool isGuestUser;

        public HomepageViewModel(IStockService stockService, IAuthenticationService authenticationService)
        {
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
            this.IsGuestUser = !this.authenticationService.IsUserLoggedIn();
            if (!this.isGuestUser)
            {
                _ = this.LoadStocksAsync();
            }

            this.FavoriteCommand = new RelayCommand(async obj => await this.ToggleFavoriteAsync(obj as HomepageStock));
            this.SearchCommand = new RelayCommand(async _ => await this.ApplyFilterAndSortAsync());
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand FavoriteCommand { get; }

        public bool CanModifyFavorites => this.authenticationService.IsUserLoggedIn() == false;

        public ICommand SearchCommand { get; }

        public ObservableCollection<HomepageStock> FilteredStocks
        {
            get => this.filteredStocks;
            private set
            {
                this.filteredStocks = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<HomepageStock> FilteredFavoriteStocks
        {
            get => this.filteredFavoriteStocks;
            private set
            {
                this.filteredFavoriteStocks = value;
                this.OnPropertyChanged();
            }
        }

        public string SearchQuery
        {
            get => this.searchQuery;
            set
            {
                this.searchQuery = value;
                this.OnPropertyChanged();
            }
        }

        public string SelectedSortOption
        {
            get => this.selectedSortOption;
            set
            {
                this.selectedSortOption = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsGuestUser
        {
            get => this.isGuestUser;
            private set
            {
                this.isGuestUser = value;
                this.OnPropertyChanged();
            }
        }

        public async Task LoadStocksAsync()
        {
            var stocks = await this.stockService.GetFilteredAndSortedStocksAsync(this.SearchQuery, this.SelectedSortOption, false);
            this.filteredStocks.Clear();
            stocks.ForEach(this.filteredStocks.Add);

            var favoriteStocks = await this.stockService.GetFilteredAndSortedStocksAsync(this.SearchQuery, this.SelectedSortOption, true);
            this.filteredFavoriteStocks.Clear();
            favoriteStocks.ForEach(this.filteredFavoriteStocks.Add);
        }

        private async Task ApplyFilterAndSortAsync()
        {
            var stocks = await this.stockService.GetFilteredAndSortedStocksAsync(this.SearchQuery, this.SelectedSortOption, false);
            this.FilteredStocks.Clear();
            foreach (var stock in stocks)
            {
                this.FilteredStocks.Add(stock);
            }

            var favoriteStocks = await this.stockService.GetFilteredAndSortedStocksAsync(this.SearchQuery, this.SelectedSortOption, true);
            this.FilteredFavoriteStocks.Clear();
            foreach (var stock in favoriteStocks)
            {
                this.FilteredFavoriteStocks.Add(stock);
            }
        }

        private async Task ToggleFavoriteAsync(HomepageStock? stock)
        {
            if (stock == null)
            {
                return;
            }

            if (stock.IsFavorite)
            {
                await this.stockService.RemoveFromFavoritesAsync(stock);
            }
            else
            {
                await this.stockService.AddToFavoritesAsync(stock);
            }

            await this.ApplyFilterAndSortAsync();
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

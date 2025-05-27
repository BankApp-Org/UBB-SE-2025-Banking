namespace StockApp.ViewModels
{
    using Common.Models;
    using Common.Services;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    /// <summary>
    /// ViewModel for the store page, managing gem deals and user gem balance.
    /// </summary>
    public partial class StoreViewModel : INotifyPropertyChanged
    {
        private readonly IStoreService _storeService;
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;
        private readonly bool _testMode = false; // Set to true for testing without the database

        private int _userGems;
        private ObservableCollection<GemDeal> _availableDeals = [];
        private List<GemDeal> _possibleDeals = [];

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreViewModel"/> class with the specified stockService.
        /// </summary>
        /// <param name="storeService">Service used to retrieve and update store data.</param>
        /// <param name="gemStoreRepository">Repository for gem store operations.</param>
        public StoreViewModel(IStoreService storeService, IUserService userService, IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            InitializeAsync();
        }

        /// <summary>
        /// Initializes user data and gem deals.
        /// </summary>
        private async void InitializeAsync()
        {
            await LoadUserDataAsync();
            LoadGemDeals();
            LoadPossibleDeals();
            await GenerateRandomDealsAsync();
        }

        /// <summary>
        /// Gets or sets the user's current gem balance.
        /// </summary>
        public int UserGems
        {
            get => _userGems;
            set
            {
                _userGems = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the user is a guest.
        /// </summary>
        public bool IsGuest => !this._authenticationService.IsUserLoggedIn();

        /// <summary>
        /// Gets or sets the collection of available gem deals.
        /// </summary>
        public ObservableCollection<GemDeal> AvailableDeals
        {
            get => _availableDeals;
            set
            {
                _availableDeals = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Loads the user gem balance asynchronously.
        /// </summary>
        public async Task LoadUserDataAsync()
        {
            if (_testMode)
            {
                // Inline: use mocked balance in test mode
                UserGems = 1234;
            }
            else
            {
                if (this._authenticationService.IsUserLoggedIn())
                {
                    UserGems = 0;
                }
                else
                {
                    UserGems = await _storeService.GetUserGemBalanceAsync();
                }
            }
        }

        /// <summary>
        /// Buys gems asynchronously using the specified deal and bank account.
        /// </summary>
        /// <param name="deal">The gem deal to purchase.</param>
        /// <param name="selectedBankAccount">The bank account identifier to use.</param>
        /// <returns>A result message indicating success or failure.</returns>
        public async Task<string> BuyGemsAsync(GemDeal deal, string selectedBankAccount)
        {
            if (string.IsNullOrEmpty(selectedBankAccount))
            {
                return "No bank account selected.";
            }

            if (_testMode)
            {
                // Inline: simulate purchase in test mode
                UserGems += deal.GemAmount;
                if (deal.IsSpecial)
                {
                    AvailableDeals.Remove(deal);
                }

                OnPropertyChanged(nameof(UserGems));
                OnPropertyChanged(nameof(AvailableDeals));
                return $"(TEST) Bought {deal.GemAmount} gems.";
            }

            var result = await _storeService.BuyGems(deal, selectedBankAccount);
            if (result.StartsWith("Successfully"))
            {
                UserGems += deal.GemAmount;
                if (deal.IsSpecial)
                {
                    AvailableDeals.Remove(deal);
                }

                OnPropertyChanged(nameof(UserGems));
                OnPropertyChanged(nameof(AvailableDeals));
            }

            return result;
        }

        /// <summary>
        /// Sells a specified amount of gems asynchronously using the given bank account.
        /// </summary>
        /// <param name="amount">The number of gems to sell.</param>
        /// <param name="selectedBankAccount">The bank account identifier to receive funds.</param>
        /// <returns>A result message indicating success or failure.</returns>
        public async Task<string> SellGemsAsync(int amount, string selectedBankAccount)
        {
            if (string.IsNullOrEmpty(selectedBankAccount))
            {
                return "No bank account selected.";
            }

            if (amount <= 0)
            {
                return "Invalid amount.";
            }

            if (amount > UserGems)
            {
                return "Not enough Gems.";
            }

            if (_testMode)
            {
                // Inline: simulate sell in test mode
                UserGems -= amount;
                OnPropertyChanged(nameof(UserGems));
                return $"(TEST) Sold {amount} gems for {amount / 100.0}€.";
            }

            var result = await _storeService.SellGems(amount, selectedBankAccount);
            if (result.StartsWith("Successfully"))
            {
                UserGems -= amount;
                OnPropertyChanged(nameof(UserGems));
            }

            return result;
        }

        /// <summary>
        /// Gets a list of the user's linked bank accounts.
        /// </summary>
        /// <returns>A list of bank account names.</returns>
        public static List<string> GetUserBankAccounts()
        {
            // TODO: Replace with real data source
            return ["Account 1", "Account 2", "Account 3"];
        }

        /// <summary>
        /// Populates the initial set of gem deals.
        /// </summary>
        private void LoadGemDeals()
        {
            AvailableDeals =
            [
                new GemDeal("LEGENDARY DEAL!!!!", 4999, 100.0),
                new GemDeal("MYTHIC DEAL!!!!", 3999, 90.0),
                new GemDeal("INSANE DEAL!!!!", 3499, 85.0),
                new GemDeal("GIGA DEAL!!!!", 3249, 82.0),
                new GemDeal("WOW DEAL!!!!", 3000, 80.0),
                new GemDeal("YAY DEAL!!!!", 2500, 50.0),
                new GemDeal("YUPY DEAL!!!!", 2000, 49.0),
                new GemDeal("HELL NAH DEAL!!!", 1999, 48.0),
                new GemDeal("BAD DEAL!!!!", 1000, 45.0),
                new GemDeal("MEGA BAD DEAL!!!!", 500, 40.0),
                new GemDeal("BAD DEAL!!!!", 1, 35.0),
                new GemDeal("🔥 SPECIAL DEAL", 2, 2.0, true, 1),
            ];
            SortDeals();
        }

        /// <summary>
        /// Loads the pool of possible special deals.
        /// </summary>
        private void LoadPossibleDeals()
        {
            _possibleDeals =
            [
                new GemDeal("🔥 SPECIAL DEAL", 2, 2.0, true, 1),
                new GemDeal("🔥 SPECIAL DEAL", 3, 3.0, true, 1),
                new GemDeal("🔥 SPECIAL DEAL", 4, 4.0, true, 1),
                new GemDeal("🔥 SPECIAL DEAL", 5, 5.0, true, 1),
                new GemDeal("🔥 SPECIAL DEAL", 6, 6.0, true, 1),
                new GemDeal("🔥 SPECIAL DEAL", 7, 7.0, true, 1),
                new GemDeal("🔥 SPECIAL DEAL", 8, 8.0, true, 1),
                new GemDeal("🔥 SPECIAL DEAL", 9, 9.0, true, 1),
                new GemDeal("🔥 SPECIAL DEAL", 10, 10.0, true, 1),
            ];
        }

        private async Task GenerateRandomDealsAsync()
        {
            if (_testMode)
            {
                return;
            }

            var random = new Random();
            var deals = _possibleDeals.OrderBy(x => random.Next()).Take(3).ToList();
            foreach (var deal in deals)
            {
                AvailableDeals.Add(deal);
            }

            SortDeals();
            await CheckAndRemoveExpiredDealsAsync();
        }

        private async Task CheckAndRemoveExpiredDealsAsync()
        {
            if (_testMode)
            {
                return;
            }

            var expiredDeals = AvailableDeals.Where(d => d.IsSpecial && !d.IsAvailable).ToList();
            foreach (var deal in expiredDeals)
            {
                AvailableDeals.Remove(deal);
            }

            if (expiredDeals.Count > 0)
            {
                await GenerateRandomDealsAsync();
            }
        }

        private void SortDeals()
        {
            var sortedDeals = AvailableDeals.OrderByDescending(d => d.GemAmount).ToList();
            AvailableDeals.Clear();
            foreach (var deal in sortedDeals)
            {
                AvailableDeals.Add(deal);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

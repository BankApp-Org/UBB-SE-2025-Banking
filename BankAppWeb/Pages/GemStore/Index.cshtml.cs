using Common.Models.Trading;
using Common.Services.Stock;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BankAppWeb.Pages.GemStore
{
    public class IndexModel : PageModel
    {
        private readonly IStoreService _storeService;
        public IndexModel(IStoreService storeService)
        {
            _storeService = storeService;
        }


        public async Task<IActionResult> OnGetAsync()
        {
            // Load data asynchronously
            await InitializeAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostBuyAsync(string dealTitle)
        {
            // Reset messages
            ErrorMessage = null;
            SuccessMessage = null;

            // Initialize to get the current available deals
            await InitializeAsync();

            // Find the selected deal
            var deal = AvailableDeals.Find(d => d.Title == dealTitle);
            if (deal == null)
            {
                ErrorMessage = "Please select a deal before buying.";
                return Page();
            }

            // Check if user is a guest
            if (IsGuest)
            {
                ErrorMessage = "Guests are not allowed to buy gems.";
                return Page();
            }

            // Check if bank account is selected
            if (string.IsNullOrEmpty(SelectedBankAccount))
            {
                ErrorMessage = "No bank account selected.";
                return Page();
            }

            // Process the purchase asynchronously
            string result = await BuyGemsAsync(deal, SelectedBankAccount);
            if (!string.IsNullOrEmpty(result))
            {
                SuccessMessage = result;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSellAsync(int gemsToSell)
        {
            // Reset messages
            ErrorMessage = null;
            SuccessMessage = null;

            await InitializeAsync();

            // Check if user is a guest
            if (IsGuest)
            {
                ErrorMessage = "Guests are not allowed to sell gems.";
                return Page();
            }

            // Validate gem amount
            if (gemsToSell <= 0)
            {
                ErrorMessage = "Enter a valid number of Gems.";
                return Page();
            }

            // Check if user has enough gems
            if (gemsToSell > UserGems)
            {
                ErrorMessage = "Not enough Gems to sell.";
                return Page();
            }

            // Check if bank account is selected
            if (string.IsNullOrEmpty(SelectedBankAccount))
            {
                ErrorMessage = "No bank account selected.";
                return Page();
            }

            // Process the sale asynchronously
            string result = await SellGemsAsync(gemsToSell, SelectedBankAccount);
            UserGems = await _storeService.GetUserGemBalanceAsync();
            if (!string.IsNullOrEmpty(result))
            {
                SuccessMessage = result;
                GemsToSell = 0;
            }

            return Page();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private List<GemDeal> _availableDeals = [];
        private List<string> _userBankAccounts = [];
        private int _userGems;
        private string? _selectedBankAccount;
        private string? _errorMessage;
        private string? _successMessage;
        private bool _isLoading;
        private bool _isGuest;
        private int _gemsToSell;

        public List<GemDeal> AvailableDeals
        {
            get => _availableDeals;
            set
            {
                _availableDeals = value;
                OnPropertyChanged();
            }
        }

        public List<string> UserBankAccounts
        {
            get => _userBankAccounts;
            set
            {
                _userBankAccounts = value;
                OnPropertyChanged();
            }
        }

        public int UserGems
        {
            get => _userGems;
            set
            {
                _userGems = value;
                OnPropertyChanged();
            }
        }

        public string? SelectedBankAccount
        {
            get => _selectedBankAccount;
            set
            {
                _selectedBankAccount = value;
                OnPropertyChanged();
            }
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public string? SuccessMessage
        {
            get => _successMessage;
            set
            {
                _successMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public bool IsGuest
        {
            get => _isGuest;
            set
            {
                _isGuest = value;
                OnPropertyChanged();
            }
        }

        public int GemsToSell
        {
            get => _gemsToSell;
            set
            {
                _gemsToSell = value;
                OnPropertyChanged();
            }
        }

        // Asynchronous version of initialize
        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                SuccessMessage = null;

                if (_storeService != null)
                {
                    UserGems = await _storeService.GetUserGemBalanceAsync();
                }

                AvailableDeals =
                [
                    new ("LEGENDARY DEAL!!!!", 4999, 100.0),
                    new ("MYTHIC DEAL!!!!", 3999, 90.0),
                    new ("INSANE DEAL!!!!", 3499, 85.0),
                    new ("GIGA DEAL!!!!", 3249, 82.0),
                    new ("WOW DEAL!!!!", 3000, 80.0),
                    new ("YAY DEAL!!!!", 2500, 50.0),
                    new ("YUPY DEAL!!!!", 2000, 49.0),
                    new ("HELL NAH DEAL!!!", 1999, 48.0),
                    new ("BAD DEAL!!!!", 1000, 45.0),
                    new ("MEGA BAD DEAL!!!!", 500, 40.0),
                    new ("BAD DEAL!!!!", 1, 35.0),
                    new ("🔥 SPECIAL DEAL", 2, 2.0, true, 1),
                ];
                UserBankAccounts = ["Account1", "Account2", "Account3"];
                SelectedBankAccount ??= UserBankAccounts.FirstOrDefault();
                IsGuest = false;
                GemsToSell = 0;
                ErrorMessage = null;
                SuccessMessage = null;
                IsLoading = false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Asynchronous buy method
        public async Task<string> BuyGemsAsync(GemDeal deal, string selectedBankAccount)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                UserGems += deal.GemAmount;
                if (_storeService != null)
                {
                    return await _storeService.BuyGems(deal, selectedBankAccount);
                }

                return $"Successfully bought {deal.GemAmount} gems!";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to buy gems: {ex.Message}";
                return string.Empty;
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Asynchronous sell method
        public async Task<string> SellGemsAsync(int gemsToSell, string selectedBankAccount)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                UserGems -= gemsToSell;
                return await _storeService.SellGems(gemsToSell, selectedBankAccount);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to sell gems: {ex.Message}";
                return string.Empty;
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Static method to get user bank accounts similar to desktop app
        public static List<string> GetUserBankAccounts()
        {
            // In a real app, this would fetch from a service
            return ["Account1", "Account2", "Account3"];
        }

        // Property change notification helper
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

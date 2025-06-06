namespace BankAppDesktop.Views.Pages
{
    using BankAppDesktop.ViewModels;
    using Common.Models;
    using Common.Models.Bank;
    using Common.Models.Trading;
    using Common.Services;
    using Common.Services.Bank;
    using Common.Services.Trading;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    public sealed partial class GemStorePage : Page
    {
        private readonly StoreViewModel _viewModel;
        private readonly IStoreService _storeService;
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authService;
        private readonly IBankAccountService _bankAccountService;

        public GemStorePage(StoreViewModel storeViewModel, IStoreService storeService, IUserService userService, IBankAccountService bankAccountService, IAuthenticationService authService)
        {
            _viewModel = storeViewModel ?? throw new ArgumentNullException(nameof(storeViewModel));
            _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _bankAccountService = bankAccountService ?? throw new ArgumentNullException(nameof(bankAccountService));
            this.DataContext = _viewModel;
            this.InitializeComponent();
            this.Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            await LoadInitialDataAsync();
        }

        private async Task LoadInitialDataAsync()
        {
            _viewModel.IsLoading = true;
            try
            {
                // Populate the deals with predefined values
                _viewModel.AvailableDeals = new ObservableCollection<GemDeal>(
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
                ]);

                string? userCnp = GetCurrentUserCnp();
                if (userCnp != null)
                {
                    _viewModel.UserGems = await _storeService.GetUserGemBalanceAsync(userCnp);
                }
                else
                {
                    _viewModel.UserGems = 0;
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog($"Error loading store data: {ex.Message}");
            }
            finally
            {
                _viewModel.IsLoading = false;
            }
        }

        private string? GetCurrentUserCnp()
        {
            return _authService.IsUserLoggedIn() ? _authService.GetUserCNP() : null;
        }

        private async Task GetUserBankAccountsAsync()
        {
            if (!_authService.IsUserLoggedIn())
            {
                return;
            }
            string userCnp = _authService.GetCurrentUserSession().UserId;

            _viewModel.UserBankAccounts = await _bankAccountService.GetUserBankAccounts(int.Parse(userCnp));
        }

        private async void OnBuyClicked(object sender, RoutedEventArgs e)
        {
            if (!_authService.IsUserLoggedIn())
            {
                this.ShowErrorDialog("Guests are not allowed to buy gems.");
                return;
            }

            await GetUserBankAccountsAsync();

            if (sender is Button button && button.CommandParameter is GemDeal selectedDeal)
            {
                if (_viewModel.UserBankAccounts.Count == 0)
                {
                    this.ShowErrorDialog("No bank accounts available for purchase.");
                    return;
                }

                ComboBox bankAccountDropdown = new()
                {
                    ItemsSource = _viewModel.UserBankAccounts,
                    SelectedIndex = 0,
                    DisplayMemberPath = "Name",
                };

                StackPanel dialogContent = new();
                dialogContent.Children.Add(new TextBlock { Text = $"You are about to buy {selectedDeal.GemAmount} Gems for {selectedDeal.Price}€.\n\nSelect a Bank Account:" });
                dialogContent.Children.Add(bankAccountDropdown);

                ContentDialog confirmDialog = new()
                {
                    Title = "Confirm Purchase",
                    Content = dialogContent,
                    PrimaryButtonText = "Buy",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.rootGrid.XamlRoot,
                };

                ContentDialogResult result = await confirmDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    if (bankAccountDropdown.SelectedItem is not BankAccount selectedAccount)
                    {
                        this.ShowErrorDialog("No bank account selected.");
                        return;
                    }
                    _viewModel.IsLoading = true;
                    try
                    {
                        string? userCnp = GetCurrentUserCnp();
                        if (userCnp == null)
                        {
                            ShowErrorDialog("User session error.");
                            return;
                        }
                        string purchaseResult = await _storeService.BuyGems(selectedDeal, selectedAccount.Id.ToString(), userCnp);
                        this.ShowSuccessDialog(purchaseResult);
                        _viewModel.UserGems = await _storeService.GetUserGemBalanceAsync(userCnp);
                    }
                    catch (Exception ex)
                    {
                        ShowErrorDialog($"Purchase failed: {ex.Message}");
                    }
                    finally
                    {
                        _viewModel.IsLoading = false;
                    }
                }
            }
            else
            {
                this.ShowErrorDialog("Please select a deal before buying.");
            }
        }

        private async void ShowErrorDialog(string message)
        {
            ContentDialog errorDialog = new()
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.rootGrid.XamlRoot,
            };
            await errorDialog.ShowAsync();
        }

        private async void ShowSuccessDialog(string message)
        {
            ContentDialog successDialog = new()
            {
                Title = "Success",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.rootGrid.XamlRoot,
            };
            await successDialog.ShowAsync();
        }

        private async void OnSellClicked(object sender, RoutedEventArgs e)
        {
            if (!_authService.IsUserLoggedIn())
            {
                this.ShowErrorDialog("Guests are not allowed to sell gems.");
                return;
            }

            if (!int.TryParse(this.sellInput.Text, out int gemsToSell) || gemsToSell <= 0)
            {
                this.ShowErrorDialog("Enter a valid number of Gems.");
                return;
            }

            string? currentUserCnp = GetCurrentUserCnp();
            if (currentUserCnp == null)
            {
                ShowErrorDialog("User session error.");
                return;
            }

            int currentUserGems = await _storeService.GetUserGemBalanceAsync(currentUserCnp);
            if (gemsToSell > currentUserGems)
            {
                this.ShowErrorDialog("Not enough Gems to sell.");
                return;
            }

            if (_viewModel.UserBankAccounts.Count == 0)
            {
                this.ShowErrorDialog("No bank accounts available for selling.");
                return;
            }

            ComboBox bankAccountDropdown = new()
            {
                ItemsSource = _viewModel.UserBankAccounts,
                SelectedIndex = 0,
            };

            StackPanel dialogContent = new();
            dialogContent.Children.Add(new TextBlock { Text = $"You are about to sell {gemsToSell} Gems for {gemsToSell / 100.0:F2}€.\n\nSelect a Bank Account from below:\n" });
            dialogContent.Children.Add(bankAccountDropdown);

            ContentDialog sellDialog = new()
            {
                Title = "Confirm Sale",
                Content = dialogContent,
                PrimaryButtonText = "Sell",
                CloseButtonText = "Cancel",
                XamlRoot = this.rootGrid.XamlRoot,
            };

            ContentDialogResult result = await sellDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                if (bankAccountDropdown.SelectedItem is not string selectedAccount)
                {
                    this.ShowErrorDialog("No bank account selected.");
                    return;
                }
                _viewModel.IsLoading = true;
                try
                {
                    string sellResult = await _storeService.SellGems(gemsToSell, selectedAccount, currentUserCnp);
                    this.ShowSuccessDialog(sellResult);
                    _viewModel.UserGems = await _storeService.GetUserGemBalanceAsync(currentUserCnp);
                }
                catch (Exception ex)
                {
                    ShowErrorDialog($"Sale failed: {ex.Message}");
                }
                finally
                {
                    _viewModel.IsLoading = false;
                }
            }
        }
    }
}
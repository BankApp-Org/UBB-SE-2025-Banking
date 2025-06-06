using Common.Models.Bank;
using Common.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using BankAppDesktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace BankAppDesktop.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public event EventHandler? LogOut;

        public MainPageViewModel ViewModel { get; private set; }

        public MainPage()
        {
            this.InitializeComponent();
            this.ViewModel = App.Services.GetRequiredService<MainPageViewModel>();
            this.DataContext = ViewModel;
            // WindowManager.RegisterWindow(this);

            // Set the welcome text from ViewModel
            centeredTextField.Text = this.ViewModel.WelcomeText;
        }

        // Helper methods for credit score styling
        public Brush GetCreditScoreBorderBrush(int score)
        {
            return score switch
            {
                >= 750 => new SolidColorBrush(Microsoft.UI.Colors.Green),
                >= 700 => new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue),
                >= 650 => new SolidColorBrush(Microsoft.UI.Colors.Blue),
                >= 600 => new SolidColorBrush(Microsoft.UI.Colors.Orange),
                _ => new SolidColorBrush(Microsoft.UI.Colors.Red),
            };
        }

        public Brush GetCreditScoreTextBrush(int score)
        {
            return score switch
            {
                >= 750 => new SolidColorBrush(Microsoft.UI.Colors.Green),
                >= 700 => new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue),
                >= 650 => new SolidColorBrush(Microsoft.UI.Colors.Blue),
                >= 600 => new SolidColorBrush(Microsoft.UI.Colors.Orange),
                _ => new SolidColorBrush(Microsoft.UI.Colors.Red),
            };
        }

        public Brush GetCreditScoreBackgroundBrush(int score)
        {
            return score switch
            {
                >= 750 => new SolidColorBrush(Microsoft.UI.Colors.Green),
                >= 700 => new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue),
                >= 650 => new SolidColorBrush(Microsoft.UI.Colors.Blue),
                >= 600 => new SolidColorBrush(Microsoft.UI.Colors.Orange),
                _ => new SolidColorBrush(Microsoft.UI.Colors.Red),
            };
        }

        public Brush GetCreditScoreProgressBrush(int score)
        {
            return score switch
            {
                >= 750 => new SolidColorBrush(Microsoft.UI.Colors.Green),
                >= 700 => new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue),
                >= 650 => new SolidColorBrush(Microsoft.UI.Colors.Blue),
                >= 600 => new SolidColorBrush(Microsoft.UI.Colors.Orange),
                _ => new SolidColorBrush(Microsoft.UI.Colors.Red),
            };
        }

        public async void CheckBalanceButtonHandler(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.CheckBalanceButtonHandler();
        }

        public async void LoanButtonHandler(object sender, RoutedEventArgs e)
        {
            var errorMessage = await this.ViewModel.LoanButtonHandler();
            if (errorMessage != null)
            {
                await this.ShowDialog(errorMessage);
            }
        }

        private async void CreditHistoryButtonHandler(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.ViewCreditHistoryButtonHandler();
        }

        private async void TransactionButtonHandler(object sender, RoutedEventArgs e)
        {
            var errorMessage = await this.ViewModel.TransactionButtonHandler();
            if (errorMessage != null)
            {
                await this.ShowDialog(errorMessage);
            }
        }

        private async void TransactionHistoryButtonHandler(object sender, RoutedEventArgs e)
        {
            var errorMessage = await this.ViewModel.TransactionHistoryButtonHandler();
            if (errorMessage != null)
            {
                await this.ShowDialog(errorMessage);
            }
        }

        private void AccountsFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ViewModel.ResetBalanceButtonContent();
            var flipView = sender as FlipView;
            if (flipView?.SelectedItem is BankAccount selectedAccount)
            {
                // Optional: You can handle the selection change here
                Debug.Print($"Selected account: {selectedAccount.Name}, IBAN: {selectedAccount.Iban}");

                try
                {
                    // UserSession.Instance.SetUserData("current_bank_account_iban", selectedAccount.Iban);
                    this.ViewModel.SelectedIban = selectedAccount.Iban;
                }
                catch (Exception ex)
                {
                    Debug.Print($"Error setting current bank account: {ex.Message}");
                }
            }
        }

        private void BankAccountCreateButtonHandler(object sender, RoutedEventArgs e)
        {
            this.ViewModel.BankAccountCreateButtonHandler();
        }

        private async void BankAccountDetailsViewButtonHandler(object sender, RoutedEventArgs e)
        {
            var errorMessage = await this.ViewModel.BankAccountDetailsButtonHandler();
            if (errorMessage != null)
            {
                await this.ShowDialog(errorMessage);
            }
        }

        private async void BankAccountSettingsButtonHandler(object sender, RoutedEventArgs e)
        {
            var errorMessage = await this.ViewModel.BankAccountSettingsButtonHandler();
            if (errorMessage != null)
            {
                await this.ShowDialog(errorMessage);
            }
        }

        public async Task RefreshBankAccounts()
        {
            await this.ViewModel.RefreshBankAccounts();
        }

        private async Task ShowDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}

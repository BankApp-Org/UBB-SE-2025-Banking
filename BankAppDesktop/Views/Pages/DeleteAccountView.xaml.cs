using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BankAppDesktop.ViewModels;
using System;
using System.Threading.Tasks;

namespace BankAppDesktop.Views.Pages
{
    public sealed partial class DeleteAccountView : Window
    {
        private readonly DeleteAccountViewModel _viewModel;

        public DeleteAccountView(DeleteAccountViewModel viewModel)
        {
            this.InitializeComponent();
            _viewModel = viewModel;
            MainGrid.DataContext = _viewModel;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordInput.Password))
            {
                await ShowErrorDialog("Please enter your password.");
                return;
            }

            try
            {
                var success = await _viewModel.DeleteAccount(PasswordInput.Password);
                if (success)
                {
                    await ShowSuccessDialog("Account deleted successfully.");
                    _viewModel.OnClose?.Invoke();
                }
                else
                {
                    await ShowErrorDialog(_viewModel.ErrorMessage ?? "Failed to delete account. Please try again.");
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Error: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnClose?.Invoke();
        }

        private async Task ShowErrorDialog(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private async Task ShowSuccessDialog(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Success",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
} 
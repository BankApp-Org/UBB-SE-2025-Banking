namespace BankAppDesktop.Views.Pages
{
    using BankAppDesktop.ViewModels;
    using Common.Models.Trading;
    using Common.Services.Trading;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    public sealed partial class BankAccountUpdateView : Page
    {
        private BankAccountUpdateViewModel? viewModel;

        public BankAccountUpdateView()
        {
            try
            {
                this.InitializeComponent();

                viewModel = App.Services.GetRequiredService<BankAccountUpdateViewModel>();

                MainGrid.DataContext = viewModel;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing BankAccountUpdateView: {ex.Message}");
                ShowErrorDialog("Initialization Error", $"Failed to initialize the bank account update view: {ex.Message}");
            }
        }

        // handles the Update Button Click
        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (viewModel == null)
                {
                    await ShowDialog("Error", "ViewModel not initialized properly.", "OK");
                    return;
                }

                string result = await viewModel.UpdateBankAccount();

                if (result == "Success")
                {
                    await ShowDialog("Success", "Bank account updated successfully.", "OK");
                    viewModel.OnUpdateSuccess?.Invoke();
                }
                else
                {
                    await ShowDialog("Error", result, "OK");
                }
            }
            catch (Exception ex)
            {
                await ShowDialog("Error", $"Failed to update bank account: {ex.Message}", "OK");
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                viewModel.DeleteBankAccount();
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Error", $"Failed to delete bank account: {ex.Message}");
            }
        }

        private async Task ShowDialog(string title, string content, string closeButtonText)
        {
            try
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = title,
                    Content = content,
                    CloseButtonText = closeButtonText,
                    XamlRoot = this.Content.XamlRoot
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing dialog: {ex.Message}");
            }
        }
        private void ShowErrorDialog(string title, string message)
        {
            // Create a separate method to show error dialogs outside the normal UI thread
            DispatcherQueue.TryEnqueue(() =>
            {
                var errorDialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK"
                };

                if (Content != null && Content.XamlRoot != null)
                {
                    errorDialog.XamlRoot = Content.XamlRoot;
                    _ = errorDialog.ShowAsync();
                }
            });
        }
    }
}

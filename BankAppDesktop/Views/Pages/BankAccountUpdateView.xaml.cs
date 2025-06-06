namespace BankAppDesktop.Views.Pages
{
    using BankAppDesktop.ViewModels;
    using Common.Models.Trading;
    using Common.Services.Trading;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using System;
    using System.Threading.Tasks;
    public sealed partial class BankAccountUpdateView : Window
    {
        private BankAccountUpdateViewModel? viewModel;

        public BankAccountUpdateView()
        {
            try
            {
                this.InitializeComponent();

                // Initialize the ViewModel after the component is initialized
                viewModel = App.Services.GetRequiredService<BankAccountUpdateViewModel>();

                AppWindow.Resize(new Windows.Graphics.SizeInt32(800, 1400));
                MainGrid.DataContext = viewModel;

                viewModel.OnUpdateSuccess = () =>
                {
                    this.Close();
                };

                viewModel.OnClose = () =>
                {
                    this.Close();
                };

                WindowManager.RegisterWindow(this);
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
                    WindowManager.ShouldReloadBankAccounts = true;
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

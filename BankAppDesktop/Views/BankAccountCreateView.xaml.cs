using System.Threading.Tasks;
using System;
using BankAppDesktop.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Common.Services;

namespace BankAppDesktop.Views
{
    public sealed partial class BankAccountCreateView : Page
    {
        private BankAccountCreateViewModel viewModel;

        public BankAccountCreateView()
        {
            this.InitializeComponent();
            viewModel = App.Services.GetRequiredService<BankAccountCreateViewModel>();
            var authService = App.Services.GetRequiredService<IAuthenticationService>();
            if (int.TryParse(authService.GetUserCNP(), out int userId))
            {
                viewModel.UserID = userId;
            }
            else
            {
                viewModel.UserID = 0;
            }
            MainGrid.DataContext = viewModel;

            viewModel.OnClose = () => { /* Implement navigation if needed */ };
            viewModel.OnSuccess = async () => await this.ShowSuccessMessage();
        }

        public void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                viewModel.SelectedItem = new CurrencyItem { Name = radioButton.Content.ToString() ?? string.Empty, IsChecked = true };
            }
        }

        private async Task ShowSuccessMessage()
        {
            var dialog = new ContentDialog
            {
                Title = "Success",
                Content = "Bank account creation was successful!",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
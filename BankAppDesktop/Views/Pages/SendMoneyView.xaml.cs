using BankAppDesktop.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace BankAppDesktop.Views.Pages
{
    public sealed partial class SendMoneyView : Page
    {
        public SendMoneyViewModel ViewModel { get; private set; }

        public SendMoneyView(SendMoneyViewModel viewModel)
        {
            this.InitializeComponent();
            this.ViewModel = viewModel;
            MainGrid.DataContext = this.ViewModel;
            this.ViewModel.CloseAction = CloseWindow;
        }
        public async void SendMoneyButtonHandler(object sender, RoutedEventArgs e)
        {
            string result = await ViewModel.ProcessPaymentAsync();
            await ShowDialog(result);
        }

        private async Task ShowDialog(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Transaction Result",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = App.MainAppWindow!.MainAppFrame.XamlRoot,
            };

            await dialog.ShowAsync();
        }

        private void CloseWindow()
        {
            App.MainAppWindow!.MainAppFrame.Content = this.ViewModel.PreviousPage;
        }
    }
}

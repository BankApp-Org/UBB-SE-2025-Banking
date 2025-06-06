using BankAppDesktop.ViewModels;
using Microsoft.UI.Xaml;
using Common.Models.Trading;
using Common.Services.Trading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BankAppDesktop.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BankAccountVerifyView : Page
    {
        private BankAccountVerifyViewModel viewModel;
        public BankAccountVerifyView()
        {
            // this.InitializeComponent();
            viewModel = App.Services.GetRequiredService<BankAccountVerifyViewModel>();

            viewModel.OnSuccess = async () => await ShowSuccessMessage();
            viewModel.OnFailure = async () => await ShowFailureMessage();

            // WindowManager.RegisterWindow(this);
        }

        private async Task ShowSuccessMessage()
        {
            var dialog = new ContentDialog
            {
                Title = "Success",
                Content = "Bank account deleted!",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync().AsTask();

            viewModel.OnClose?.Invoke();
            // update main page view model
        }
        private async Task ShowFailureMessage()
        {
            var dialog = new ContentDialog
            {
                Title = "Failure",
                Content = "Wrong credentials. Try again.",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync().AsTask();
        }
    }
}

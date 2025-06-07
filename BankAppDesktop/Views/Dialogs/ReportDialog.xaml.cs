using Microsoft.UI.Xaml.Controls;
using BankAppDesktop.ViewModels;
using Microsoft.UI.Xaml;
using System;
using Common.Services.Social;

namespace BankAppDesktop.Views.Dialogs
{
    public sealed partial class ReportDialog : ContentDialog
    {
        public IReportViewModel ViewModel { get; private set; }

        public ReportDialog(ReportViewModel viewModel)
        {
            this.InitializeComponent();

            ViewModel = viewModel;
            DataContext = ViewModel;

            // Subscribe to ViewModel events
            viewModel.ShowErrorDialog += OnShowErrorDialog;
            viewModel.ShowSuccessDialog += OnShowSuccessDialog;
            viewModel.CloseView += OnCloseView;
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Implementation remains the same as original ReportView
            if (sender is ComboBox comboBox && comboBox.SelectedItem != null)
            {
                if (comboBox.SelectedItem is ReportReason reason)
                {
                    ViewModel.SelectedReportReason = reason;
                }
            }
        }

        private async void OnShowErrorDialog(string message)
        {
            var errorDialog = new ContentDialog()
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await errorDialog.ShowAsync();
        }

        private async void OnShowSuccessDialog(string message)
        {
            var successDialog = new ContentDialog()
            {
                Title = "Success",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await successDialog.ShowAsync();

            // Close the report dialog after success
            this.Hide();
        }

        private void OnCloseView()
        {
            this.Hide();
        }
    }
}

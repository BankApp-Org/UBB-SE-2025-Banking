// <copyright file="ReportView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BankAppDesktop.Views.Pages
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using BankAppDesktop.ViewModels;
    using Common.Services.Social;

    public sealed partial class ReportView : Window
    {
        public ReportViewModel ViewModel { get; }

        public ReportView(ReportViewModel viewModel)
        {
            this.InitializeComponent();

            ViewModel = viewModel;
            if (this.Content is FrameworkElement content)
            {
                content.DataContext = ViewModel;
            }

            ViewModel.ShowErrorDialog += OnShowErrorDialog;
            ViewModel.ShowSuccessDialog += OnShowSuccessDialog;
            ViewModel.CloseView += OnCloseView;
        }

        private async void OnShowErrorDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot,
            };
            await dialog.ShowAsync();
        }

        private async void OnShowSuccessDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Success",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot,
            };
            await dialog.ShowAsync();
            this.Close();
        }

        private void OnCloseView()
        {
            this.Close();
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem is ReportReason selectedReason)
            {
                ViewModel.SelectedReportReason = selectedReason;
            }
        }
    }
}

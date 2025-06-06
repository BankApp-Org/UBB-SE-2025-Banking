// <copyright file="ReportView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BankAppDesktop.Views.Pages
{
    using BankAppDesktop.ViewModels;
    using Common.Services.Social;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using System;

    public sealed partial class ReportView : Window
    {
        public IReportViewModel ViewModel { get; private set; }

        public ReportView(ReportViewModel viewModel)
        {
            this.InitializeComponent();

            ViewModel = viewModel;
            if (this.Content is FrameworkElement content)
            {
                content.DataContext = ViewModel;
            }

            viewModel.ShowErrorDialog += OnShowErrorDialog;
            viewModel.ShowSuccessDialog += OnShowSuccessDialog;
            viewModel.CloseView += OnCloseView;
        }

        // Parameterless constructor for demo
        public ReportView()
        {
            this.InitializeComponent();
        }

        // Initialize with demo data
        public void InitializeDemoData(int chatId, int messageId, int reportedUserId)
        {
            var demoViewModel = new ReportViewModelDemo(chatId, messageId, reportedUserId);
            ViewModel = demoViewModel;

            if (this.Content is FrameworkElement content)
            {
                content.DataContext = ViewModel;
            }

            demoViewModel.ShowErrorDialog += OnShowErrorDialog;
            demoViewModel.ShowSuccessDialog += OnShowSuccessDialog;
            demoViewModel.CloseView += OnCloseView;
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

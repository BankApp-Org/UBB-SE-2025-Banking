// <copyright file="ReportViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Common.Services;
using Common.Services.Social;
using System.Collections.ObjectModel;

namespace BankAppDesktop.ViewModels
{
    using BankAppDesktop.Commands;
    using Common.Models;
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;

    /// <summary>
    /// Interface for report view models to ensure common properties and events.
    /// </summary>
    public interface IReportViewModel : INotifyPropertyChanged
    {
        event Action<string> ShowSuccessDialog;
        event Action<string> ShowErrorDialog;
        event Action CloseView;

        ObservableCollection<ReportReason> ReportReasons { get; }
        ReportReason SelectedReportReason { get; set; }
        bool IsOtherCategorySelected { get; }
        string OtherReason { get; set; }
        ICommand SubmitCommand { get; }
        ICommand CancelCommand { get; }
    }

    /// <summary>
    /// ViewModel for handling report functionality.
    /// </summary>
    public class ReportViewModel : IReportViewModel
    {
        public event Action<string> ShowSuccessDialog = message => { };
        public event Action<string> ShowErrorDialog = message => { };
        public event Action CloseView = () => { };

        private readonly IMessageService messageService;
        private readonly IAuthenticationService authenticationService;
        protected readonly int reportedUserId;
        protected readonly int messageId;
        protected readonly int chatId;

        private ReportReason selectedReportReason = ReportReason.Other;
        private string otherReason = string.Empty;

        /// <summary>
        /// Gets the available report reasons from the enum.
        /// </summary>
        public ObservableCollection<ReportReason> ReportReasons { get; }

        /// <summary>
        /// Gets or sets the selected report reason.
        /// </summary>
        public ReportReason SelectedReportReason
        {
            get => this.selectedReportReason;
            set
            {
                if (this.selectedReportReason != value)
                {
                    this.selectedReportReason = value;
                    this.OnPropertyChanged(nameof(this.SelectedReportReason));
                    this.OnPropertyChanged(nameof(this.IsOtherCategorySelected));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the selected category is "Other".
        /// </summary>
        public bool IsOtherCategorySelected => this.SelectedReportReason == ReportReason.Other;

        /// <summary>
        /// Gets or sets the reason for reporting when the selected category is "Other".
        /// </summary>
        public string OtherReason
        {
            get => this.otherReason;
            set
            {
                if (this.otherReason != value)
                {
                    this.otherReason = value;
                    this.OnPropertyChanged(nameof(this.OtherReason));
                }
            }
        }

        /// <summary>
        /// Gets the command to submit the report.
        /// </summary>
        public ICommand SubmitCommand { get; }

        /// <summary>
        /// Gets the command to cancel the report.
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportViewModel"/> class.
        /// </summary>
        /// <param name="messageService">The message service.</param>
        /// <param name="authenticationService">The authentication service for user access.</param>
        /// <param name="chatId">The ID of the chat containing the message.</param>
        /// <param name="messageId">The ID of the message being reported.</param>
        /// <param name="reportedUserId">The ID of the reported user.</param>
        public ReportViewModel(IMessageService messageService, IAuthenticationService authenticationService, int chatId, int messageId, int reportedUserId)
        {
            this.messageService = messageService;
            this.authenticationService = authenticationService;
            this.chatId = chatId;
            this.messageId = messageId;
            this.reportedUserId = reportedUserId;

            // Initialize the collection with all enum values
            this.ReportReasons = new ObservableCollection<ReportReason>(
                Enum.GetValues<ReportReason>().ToList());

            this.SubmitCommand = new RelayCommand(_ => this.SubmitReport());
            this.CancelCommand = new RelayCommand(_ => this.CancelReport());
        }

        /// <summary>
        /// Submits the report.
        /// </summary>
        protected virtual async void SubmitReport()
        {
            try
            {
                if (this.SelectedReportReason == ReportReason.Other && string.IsNullOrWhiteSpace(this.OtherReason))
                {
                    this.OnShowErrorDialog("Please provide a reason when selecting 'Other'.");
                    return;
                }

                // Check if user is logged in
                if (!this.authenticationService.IsUserLoggedIn())
                {
                    this.OnShowErrorDialog("You must be logged in to report content.");
                    return;
                }

                // Get current user CNP
                var userCnp = this.authenticationService.GetUserCNP();

                // Create a User object for the service call
                var user = new User
                {
                    CNP = userCnp
                };

                // Submit the report using the message service
                await this.messageService.ReportMessage(this.chatId, this.messageId, user, this.SelectedReportReason);

                // Show success message and close
                this.OnShowSuccessDialog("Report submitted successfully.");
            }
            catch (Exception ex)
            {
                this.OnShowErrorDialog($"An error occurred while submitting the report: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancels the report.
        /// </summary>
        private void CancelReport()
        {
            this.CloseView?.Invoke();
        }

        /// <summary>
        /// Raises the <see cref="ShowErrorDialog"/> event.
        /// </summary>
        /// <param name="message">The error message to show.</param>
        protected virtual void OnShowErrorDialog(string message)
        {
            this.ShowErrorDialog?.Invoke(message);
        }

        /// <summary>
        /// Raises the <see cref="ShowSuccessDialog"/> event.
        /// </summary>
        /// <param name="message">The success message to show.</param>
        protected virtual void OnShowSuccessDialog(string message)
        {
            this.ShowSuccessDialog?.Invoke(message);
        }

        /// <summary>
        /// Raises the <see cref="CloseView"/> event.
        /// </summary>
        protected virtual void OnCloseView()
        {
            this.CloseView?.Invoke();
        }
    }

    public class ReportViewModelDemo : IReportViewModel
    {
        public event Action<string> ShowSuccessDialog = message => { };
        public event Action<string> ShowErrorDialog = message => { };
        public event Action CloseView = () => { };

        protected readonly int reportedUserId;
        protected readonly int messageId;
        protected readonly int chatId;

        private ReportReason selectedReportReason = ReportReason.Other;
        private string otherReason = string.Empty;

        /// <summary>
        /// Gets the available report reasons from the enum.
        /// </summary>
        public ObservableCollection<ReportReason> ReportReasons { get; }

        /// <summary>
        /// Gets or sets the selected report reason.
        /// </summary>
        public ReportReason SelectedReportReason
        {
            get => this.selectedReportReason;
            set
            {
                if (this.selectedReportReason != value)
                {
                    this.selectedReportReason = value;
                    this.OnPropertyChanged(nameof(this.SelectedReportReason));
                    this.OnPropertyChanged(nameof(this.IsOtherCategorySelected));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the selected category is "Other".
        /// </summary>
        public bool IsOtherCategorySelected => this.SelectedReportReason == ReportReason.Other;

        /// <summary>
        /// Gets or sets the reason for reporting when the selected category is "Other".
        /// </summary>
        public string OtherReason
        {
            get => this.otherReason;
            set
            {
                if (this.otherReason != value)
                {
                    this.otherReason = value;
                    this.OnPropertyChanged(nameof(this.OtherReason));
                }
            }
        }

        /// <summary>
        /// Gets the command to submit the report.
        /// </summary>
        public ICommand SubmitCommand { get; }

        /// <summary>
        /// Gets the command to cancel the report.
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        public ReportViewModelDemo(int chatId, int messageId, int reportedUserId)
        {
            this.chatId = chatId;
            this.messageId = messageId;
            this.reportedUserId = reportedUserId;

            // Initialize the collection with all enum values
            this.ReportReasons = new ObservableCollection<ReportReason>(
                Enum.GetValues<ReportReason>().ToList());

            this.SubmitCommand = new RelayCommand(_ => this.SubmitReport());
            this.CancelCommand = new RelayCommand(_ => this.CancelReport());
        }

        /// <summary>
        /// Demo submit - just shows success popup without real logic.
        /// </summary>
        private async void SubmitReport()
        {
            // Simulate validation
            if (this.SelectedReportReason == ReportReason.Other && string.IsNullOrWhiteSpace(this.OtherReason))
            {
                this.ShowErrorDialog?.Invoke("Please provide a reason when selecting 'Other'.");
                return;
            }

            string successMessage = $"🎯 Report submitted successfully!\n\n" +
                                  $"Chat ID: {this.chatId}\n" +
                                  $"Message ID: {this.messageId}\n" +
                                  $"Reported User ID: {this.reportedUserId}\n" +
                                  $"Reason: {this.SelectedReportReason}\n" +
                                  (this.SelectedReportReason == ReportReason.Other ? $"Description: {this.OtherReason}" : string.Empty);

            this.ShowSuccessDialog?.Invoke(successMessage);
        }

        /// <summary>
        /// Cancels the report.
        /// </summary>
        private void CancelReport()
        {
            this.CloseView?.Invoke();
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

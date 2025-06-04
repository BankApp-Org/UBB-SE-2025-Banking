// <copyright file="ReportViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using Common.Services.Social;
using Common.Services;

namespace BankAppDesktop.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;
    using Common.Models;
    using BankAppDesktop.Commands;

    /// <summary>
    /// ViewModel for handling report functionality.
    /// </summary>
    public class ReportViewModel : INotifyPropertyChanged
    {
        public event Action<string> ShowSuccessDialog = message => { };
        public event Action<string> ShowErrorDialog = message => { };
        public event Action CloseView = () => { };

        private readonly IMessageService messageService;
        private readonly IAuthenticationService authenticationService;
        private readonly int reportedUserId;
        private readonly int messageId;
        private readonly int chatId;

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
        private async void SubmitReport()
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
            this.OnCloseView();
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
}

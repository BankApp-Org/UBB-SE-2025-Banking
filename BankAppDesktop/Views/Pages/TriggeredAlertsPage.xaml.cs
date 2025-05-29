namespace BankAppDesktop.Views.Pages
{
    using Common.Models;
    using Common.Services;
    using Common.Services.Stock;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using System;
    using System.Collections.Generic;

    public sealed partial class TriggeredAlertsPage : Page
    {
        private readonly IAlertService _alertService;
        private readonly IAuthenticationService? _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggeredAlertsPage"/> class.
        /// </summary>
        public TriggeredAlertsPage(IAlertService alertService)
        {
            this.InitializeComponent();
            this._alertService = alertService;
            LoadAlertsAsync();
        }

        // Constructor for DI (if App.xaml.cs gets updated)
        public TriggeredAlertsPage(IAlertService alertService, IAuthenticationService authService)
        {
            this.InitializeComponent();
            _alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            LoadAlertsAsync();
        }

        private async void LoadAlertsAsync()
        {
            try
            {
                // Check if user is authenticated (if auth service is available)
                bool isLoggedIn = _authService != null && _authService.IsUserLoggedIn();

                if (_authService == null || isLoggedIn)
                {
                    var triggeredAlerts = await _alertService.GetTriggeredAlertsAsync();
                    AlertListView.ItemsSource = triggeredAlerts;

                    // Show or hide "no alerts" message
                    NoAlertsMessage.Visibility = triggeredAlerts.Count > 0
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                }
                else
                {
                    // Handle not logged in state
                    AlertListView.ItemsSource = new List<TriggeredAlert>();
                    NoAlertsMessage.Visibility = Visibility.Visible;
                    NoAlertsMessage.Text = "Please log in to view alerts.";
                }
            }
            catch (Exception ex)
            {
                // Handle errors
                ErrorMessage.Text = $"Error loading alerts: {ex.Message}";
                ErrorMessage.Visibility = Visibility.Visible;
            }
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Visibility = Visibility.Collapsed;
            LoadAlertsAsync();
        }
    }
}

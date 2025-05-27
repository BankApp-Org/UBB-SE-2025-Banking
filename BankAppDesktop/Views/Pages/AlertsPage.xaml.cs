namespace StockApp.Views.Pages
{
    using Microsoft.UI.Xaml.Controls;
    using StockApp.ViewModels;
    using System;

    public sealed partial class AlertsPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlertsPage"/> class.
        /// </summary>
        public AlertsPage(AlertViewModel viewModel)
        {
            this.ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            this.InitializeComponent();
            this.DataContext = this.ViewModel;
        }

        /// <summary>
        /// Gets or Sets the ViewModel for managing alerts.
        /// </summary>
        public AlertViewModel ViewModel { get; set; }
    }
}

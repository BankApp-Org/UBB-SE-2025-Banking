namespace StockApp.Views.Controls
{
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.ViewModels;
    using System;

    public sealed partial class AdminNewsControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminNewsControl"/> class.
        /// </summary>
        public AdminNewsControl(AdminNewsViewModel adminNewsViewModel)
        {
            this.ViewModel = adminNewsViewModel ?? throw new ArgumentNullException(nameof(adminNewsViewModel));
            this.DataContext = this.ViewModel;
            this.InitializeComponent();
            this.ViewModel.ControlRef = this;
            this.Loaded += this.OnLoaded;
        }

        /// <summary>
        /// Gets the view model for the AdminNewsControl.
        /// </summary>
        public AdminNewsViewModel ViewModel { get; }

        /// <summary>
        /// Handles the Loaded event of the AdminNewsControl control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Initialize();
        }
    }
}

namespace BankAppDesktop.Views
{
    using Microsoft.UI.Xaml.Controls;
    using BankAppDesktop.ViewModels;
    using System;

    public sealed partial class ArticleCreationPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArticleCreationPage"/> class.
        /// </summary>
        public ArticleCreationPage(ArticleCreationViewModel viewModel)
        {
            this.ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            this.DataContext = this.ViewModel;
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the view model for the article creation view.
        /// </summary>
        public ArticleCreationViewModel ViewModel { get; }
    }
}
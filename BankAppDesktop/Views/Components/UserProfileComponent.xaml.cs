// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StockApp.Views.Components
{
    using System;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.ViewModels;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserProfileComponent : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileComponent"/> class.
        /// </summary>
        /// <param name="viewModel">The view model to bind to this component.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="viewModel"/> is null.</exception>
        public UserProfileComponent(UserProfileComponentViewModel viewModel)
        {
            this.ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            this.DataContext = this.ViewModel;
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the view model associated with this component.
        /// </summary>
        public UserProfileComponentViewModel ViewModel { get; private set; }
    }
}

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StockApp.Views.Pages
{
    using Microsoft.UI.Xaml.Controls;
    using StockApp.ViewModels;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateProfilePage : Page
    {
        private readonly CreateProfilePageViewModel viewModel;

        public CreateProfilePage(CreateProfilePageViewModel createProfilePageViewModel)
        {
            this.InitializeComponent();
            this.viewModel = createProfilePageViewModel;
            this.DataContext = this.viewModel;
        }
    }
}

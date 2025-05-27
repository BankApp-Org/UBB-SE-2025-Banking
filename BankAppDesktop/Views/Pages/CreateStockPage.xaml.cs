namespace StockApp.Pages
{
    using Microsoft.UI.Xaml.Controls;
    using StockApp.ViewModels;

    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateStockPage : Page
    {
        public CreateStockPage(CreateStockViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}

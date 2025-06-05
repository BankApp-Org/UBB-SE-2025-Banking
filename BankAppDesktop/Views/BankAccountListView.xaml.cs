using System;
using BankAppDesktop.ViewModels;
using Microsoft.UI.Xaml;
using BankAppDesktop.Converters;
using Microsoft.Extensions.DependencyInjection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace BankAppDesktop.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BankAccountListView : Window
    {
        private BankAccountListViewModel viewModel;
        private bool isDoubleClicked = false;
        public BankAccountListView()
        {
            this.InitializeComponent();
            viewModel = App.Services.GetRequiredService<BankAccountListViewModel>();
            MainGrid.DataContext = viewModel;

            viewModel.OnClose = () => this.Close();

            WindowManager.RegisterWindow(this);
        }
    }
}

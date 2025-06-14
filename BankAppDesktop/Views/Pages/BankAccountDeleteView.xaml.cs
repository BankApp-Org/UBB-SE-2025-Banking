using Microsoft.UI.Xaml;
using BankAppDesktop.ViewModels;
using BankAppDesktop.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using BankAppDesktop.Commands;
using BankApi.Data;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace BankAppDesktop.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BankAccountDeleteView : Window
    {
        private readonly BankAccountDeleteViewModel viewModel;

        public BankAccountDeleteView(string iban)
        {
            this.viewModel = App.Services.GetRequiredService<BankAccountDeleteViewModel>();
            viewModel.Initialize(iban);
            this.InitializeComponent();

            MainGrid.DataContext = viewModel;

            viewModel.OnClose = () => this.Close();
        }
    }
}

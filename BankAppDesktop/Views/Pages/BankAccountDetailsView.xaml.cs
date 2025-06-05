namespace BankAppDesktop.Views.Pages
{
    using BankAppDesktop.ViewModels;
    using Common.Models.Trading;
    using Common.Services.Trading;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using System;
    using System.Threading.Tasks;
    public partial class BankAccountDetailsView : Window
    {
        private BankAccountDetailsViewModel viewModel;
        public BankAccountDetailsView(BankAccountDetailsViewModel viewModel)
        {
            this.InitializeComponent();
            this.Activate();

            this.viewModel = viewModel;
            MainGrid.DataContext = viewModel;

            viewModel.OnClose = () => this.Close();
        }
    }
}

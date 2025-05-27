namespace StockApp.Views
{
    using Common.Models;
    using Microsoft.UI.Xaml;
    using StockApp.ViewModels;

    public sealed partial class TipHistoryWindow : Window
    {
        private readonly TipHistoryViewModel viewModel;

        public TipHistoryWindow(TipHistoryViewModel tipHistoryViewModel)
        {
            this.viewModel = tipHistoryViewModel;
            this.InitializeComponent();
            this.MainPannel.DataContext = this.viewModel;
        }

        public async void LoadUser(User user)
        {
            await this.viewModel.LoadUserData(user);
        }
    }
}

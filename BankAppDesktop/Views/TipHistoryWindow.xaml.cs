namespace BankAppDesktop.Views
{
    using Common.Models;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using BankAppDesktop.ViewModels;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Services.Social;

    public sealed partial class TipHistoryWindow : Window
    {
        private readonly TipHistoryViewModel viewModel;
        private readonly ITipsService tipsService;
        private readonly IMessagesService messagesService;

        public TipHistoryWindow(TipHistoryViewModel tipHistoryViewModel)
        {
            this.viewModel = tipHistoryViewModel;
            this.tipsService = App.Host.Services.GetRequiredService<ITipsService>();
            this.messagesService = App.Host.Services.GetRequiredService<IMessagesService>();
            this.InitializeComponent();
            this.MainPannel.DataContext = this.viewModel;
        }

        public async void LoadUser(User user)
        {
            this.viewModel.SelectedUser = user;
            this.viewModel.IsLoading = true;
            try
            {
                var tips = await this.tipsService.GetTipsForUserAsync(user.CNP);
                var messages = await this.messagesService.GetMessagesForUserAsync(user.CNP);
                this.viewModel.TipHistory.Clear();
                foreach (var tip in tips)
                {
                    this.viewModel.TipHistory.Add(tip);
                }
                this.viewModel.MessageHistory.Clear();
                foreach (var message in messages)
                {
                    this.viewModel.MessageHistory.Add(message);
                }
            }
            finally
            {
                this.viewModel.IsLoading = false;
            }
        }
    }
}

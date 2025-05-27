using Common.Models;
using Common.Services;
using StockApp.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StockApp.ViewModels
{
    public class TipHistoryViewModel
    {
        private readonly IMessagesService messagesService;
        private readonly ITipsService tipsService;
        private User? selectedUser;

        public ObservableCollection<Message> MessageHistory { get; private set; }

        public ObservableCollection<Tip> TipHistory { get; private set; }

        public ICommand AddTipCommand { get; private set; }

        public TipHistoryViewModel(IMessagesService messagesRepository, ITipsService tipsRepository)
        {
            this.messagesService = messagesRepository;
            this.tipsService = tipsRepository;
            this.MessageHistory = [];
            this.TipHistory = [];
            this.AddTipCommand = new RelayCommand(sender => { _ = this.AddTip(); }, sender => true);
        }

        private async Task AddTip()
        {
            if (this.selectedUser == null)
            {
                return;
            }

            await this.tipsService.GiveTipToUserAsync(this.selectedUser.CNP);
            await this.LoadUserData(this.selectedUser);
        }

        public async Task LoadUserData(User user)
        {
            if (user == null)
            {
                return;
            }

            try
            {
                List<Message> messages = await this.messagesService.GetMessagesForUserAsync(user.CNP);
                List<Tip> tips = await this.tipsService.GetTipsForUserAsync(user.CNP);

                this.LoadHistory(messages);
                this.LoadHistory(tips);
                this.selectedUser = user;
            }
            catch
            {
                this.MessageHistory.Clear();
                this.TipHistory.Clear();
                // FIXME: Add 404 dialog or error handling
            }
        }

        private void LoadHistory(List<Message> messages)
        {
            this.MessageHistory.Clear();
            foreach (Message message in messages)
            {
                this.MessageHistory.Add(message);
            }
        }

        private void LoadHistory(List<Tip> tips)
        {
            this.TipHistory.Clear();
            foreach (Tip tip in tips)
            {
                this.TipHistory.Add(tip);
            }
        }
    }
}

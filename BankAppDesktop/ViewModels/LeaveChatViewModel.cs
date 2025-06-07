using BankAppDesktop.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAppDesktop.ViewModels
{
    using Common.Services;
    using Common.Services.Social;
    using System.ComponentModel;
    using System.Windows.Input;
    public partial class LeaveChatViewModel : INotifyPropertyChanged
    {
        public ICommand LeaveChatCommand { get; set; }

        private IUserService userService;
        private IChatService chatService;
        private ChatListViewModel lastViewModel;
        private int chatID;
        private string chatName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string ChatName
        {
            get => chatName;
            set
            {
                if (chatName != value)
                {
                    chatName = value;
                    OnPropertyChanged(nameof(ChatName));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public LeaveChatViewModel(IUserService userService, IChatService chatService, ChatListViewModel chatMessagesViewModel, int chatID)
        {
            this.LeaveChatCommand = new RelayCommand(parameter => this.LeaveChat());

            this.chatID = chatID;
            this.userService = userService;
            this.chatService = chatService;
            this.lastViewModel = chatMessagesViewModel;
            this.chatName = string.Empty;

            // Load the chat name asynchronously
            LoadChatName();
        }

        private async void LoadChatName()
        {
            try
            {
                var chat = await this.chatService.GetChatById(this.chatID);
                this.ChatName = chat.ChatName;
            }
            catch
            {
                this.ChatName = "Unknown Chat";
            }
        }
        public async void LeaveChat()
        {
            var currentUser = await this.userService.GetCurrentUserAsync();
            await this.chatService.RemoveUserFromChat(this.chatID, currentUser);

            await this.lastViewModel.LoadChats();
        }
    }
}

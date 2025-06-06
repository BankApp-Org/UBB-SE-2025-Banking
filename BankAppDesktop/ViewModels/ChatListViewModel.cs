using BankAppDesktop.Views.Converters;
using Common.Models;
using Common.Models.Social;
using Common.Services;
using Common.Services.Social;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAppDesktop.ViewModels
{
    public class ChatListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string searchQuery = string.Empty;

        public ObservableCollection<Chat> ChatList { get; set; }

        public List<Chat> CurrentUserChats;
        public IChatService ChatService;
        public IUserService UserService;

        public CountToVisibilityConverter CountToVisibilityConverter { get; set; }

        public string SearchQuery
        {
            get => this.searchQuery;
            set
            {
                if (this.searchQuery != value)
                {
                    this.searchQuery = value;
                    this.OnPropertyChanged(nameof(this.SearchQuery));
                    this.FilterChats();
                }
            }
        }

        public ChatListViewModel(IChatService chatS, IUserService userS)
        {
            this.ChatList = [];
            this.ChatService = chatS;
            this.UserService = userS;
            this.CurrentUserChats = [];
            LoadCurrentUserChats();
            this.CountToVisibilityConverter = new CountToVisibilityConverter();
        }

        public async void LoadCurrentUserChats()
        {
            this.ChatList.Clear();
            var currentUser = await this.UserService.GetCurrentUserAsync();
            this.CurrentUserChats = await this.ChatService.GetChatsForUser(currentUser.Id);
            foreach (var chat in this.CurrentUserChats)
            {
                this.ChatList.Add(chat);
            }
        }

        public async Task LoadChats()
        {
            this.FilterChats();
        }

        public async void FilterChats()
        {
            this.ChatList.Clear();
            LoadCurrentUserChats();
            foreach (var chat in this.CurrentUserChats)
            {
                if (string.IsNullOrEmpty(this.SearchQuery) ||
                    chat.ChatName.IndexOf(this.SearchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    this.ChatList.Add(chat);
                }
            }

            // sort chats by last message time
            // this.ChatList = new ObservableCollection<Chat>(this.ChatList.OrderByDescending(async chat => await this.ChatService.GetLastMessageTimeStamp(chat.getChatID())));
        }
    }
}

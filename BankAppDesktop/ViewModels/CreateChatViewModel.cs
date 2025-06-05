using BankAppDesktop.Commands;
using Common.Models;
using Common.Models.Social;
using Common.Services;
using Common.Services.Social;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BankAppDesktop.ViewModels
{
    public class CreateChatViewModel
    {
        private string groupName;
        private string searchQuery;
        private IUserService userService;
        private IChatService chatService;
        private ChatListViewModel chatListViewModel;

        public ICommand AddToSelectedList { get; }

        public ICommand CreateGroupChat { get; }

        public ObservableCollection<User> Friends { get; set; }

        private List<User> allFriends;

        public ObservableCollection<User> SelectedFriends { get; set; }

        public string GroupName
        {
            get => this.groupName;
            set
            {
                if (this.groupName != value)
                {
                    this.groupName = value;
                    this.OnPropertyChanged(nameof(this.GroupName));
                }
            }
        }

        public string SearchQuery
        {
            get => this.searchQuery;
            set
            {
                if (this.searchQuery != value)
                {
                    this.searchQuery = value;
                    this.OnPropertyChanged(nameof(this.searchQuery));
                    this.FilterFriends();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CreateChatViewModel(ChatListViewModel chatListViewModel, IChatService chatService, IUserService userService)
        {
            this.chatListViewModel = chatListViewModel;
            this.AddToSelectedList = new RelayCommandGeneric<object>(this.AddFriendToSelectedList);
            this.CreateGroupChat = new RelayCommandGeneric<object>(_ => this.AddNewGroupChat());
            this.Friends = [];
            this.SelectedFriends = [];
            this.chatService = chatService;
            this.userService = userService;
            LoadAllFriends();

            // this.LoadFriends();
        }

        public async Task LoadAllFriends()
        {
            var currentUser = await this.userService.GetCurrentUserAsync();
            this.allFriends = currentUser.Friends.ToList();
            this.Friends.Clear();

            foreach (var friend in allFriends)
            {
                Friends.Add(friend);
            }
        }

        private async void AddNewGroupChat()
        {
            List<User> selectedFriends = [];
            var currentUser = await this.userService.GetCurrentUserAsync();
            selectedFriends.Add(currentUser);
            foreach (User friend in this.SelectedFriends)
            {
                selectedFriends.Add(friend);
            }
            var chat = new Chat
            {
                ChatName = this.GroupName,
                Users = selectedFriends,
                Messages = [],
            };
            await this.chatService.CreateChat(chat);
            this.chatListViewModel.LoadCurrentUserChats();
        }

        private void AddFriendToSelectedList(object parameter)
        {
            var friend = parameter as User;
            if (friend != null && !this.SelectedFriends.Contains(friend))
            {
                this.SelectedFriends.Add(friend);
                this.FilterFriends();
            }
        }

        private void LoadFriends()
        {
            this.FilterFriends();
        }

        private void FilterFriends()
        {
            Friends.Clear();

            var filteredFriends = this.allFriends
                ?.Where(f => (string.IsNullOrEmpty(SearchQuery) ||
                             (f.FirstName?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false)) &&
                             !SelectedFriends.Contains(f))
                .ToList() ?? [];

            foreach (var friend in filteredFriends)
            {
                Friends.Add(friend);
            }
        }
    }
}

using Common.Models.Social;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BankAppWeb.ViewModels
{
    public class ChatListViewModel : INotifyPropertyChanged
    {
        private string searchQuery = string.Empty;
        private List<Chat> allChats = [];

        public List<Chat> ChatList { get; set; } = [];

        public string SearchQuery
        {
            get => searchQuery;
            set
            {
                if (searchQuery != value)
                {
                    searchQuery = value;
                    OnPropertyChanged(nameof(SearchQuery));
                    FilterChats();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetChats(List<Chat> chats)
        {
            allChats = chats ?? [];
            FilterChats();
        }

        private void FilterChats()
        {
            if (string.IsNullOrEmpty(SearchQuery))
            {
                ChatList = allChats.ToList();
            }
            else
            {
                ChatList = allChats
                    .Where(c => c.ChatName.Contains(SearchQuery, System.StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            OnPropertyChanged(nameof(ChatList));
        }
    }
}

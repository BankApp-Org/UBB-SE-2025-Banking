using BankApi.Repositories.Social;
using BankAppDesktop.ViewModels;
using Common.Models.Social;
using Common.Services.Social;
using Common.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using BankAppDesktop.Views.Pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace BankAppDesktop.Views.Components
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChatListComponent : Page
    {
        private ChatListViewModel chatListViewModel;
        public IUserService UserService;
        public IChatService ChatService;
        public IMessageService MessageService;
        public IChatReportService ReportService;
        public Frame RightFrame;
        public Window MainFrame;

        // ?
        // change IRepository into IChatRepository?
        private IChatRepository repository;

        public ChatListComponent(Window mainFrame, IChatService chatService, IUserService userService, IChatReportService reportService, IMessageService messageService, Frame rightFrame, IChatRepository repository)
        {
            this.InitializeComponent();

            this.MainFrame = mainFrame;
            this.UserService = userService;
            this.ChatService = chatService;
            this.MessageService = messageService;
            this.ReportService = reportService;
            this.RightFrame = rightFrame;
            this.chatListViewModel = new ChatListViewModel(ChatService, UserService);
            this.MainGrid.DataContext = this.chatListViewModel;

            // ?
            this.repository = repository;
        }

        private void CreateChat_Click(object sender, RoutedEventArgs e)
        {
            this.RightFrame.Content = new CreateChatPage(this.chatListViewModel, this.ChatService, this.UserService);
        }

        private void ChatList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ChatList.SelectedItem is Chat selectedChat)
            {
                this.RightFrame.Content = new ChatMessagesPage(this.chatListViewModel, this.MainFrame, this.RightFrame, selectedChat.Id, this.UserService, this.ChatService, this.MessageService, this.ReportService, this.repository);
            }
        }
    }
}

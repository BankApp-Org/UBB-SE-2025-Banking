using BankAppDesktop.Commands;
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
using System.Reflection.Metadata;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace BankAppDesktop.Views.Pages
{
    using BankAppDesktop.ViewModels;
    using Common.Services;
    using Common.Services.Social;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using NuGet.Protocol.Core.Types;

    public sealed partial class ChatMessagesPage : Page
    {
        public int SelectedChat { get; set; }

        private ChatMessagesViewModel chatMessagesViewModel;
        private Frame rightFrame;
        private IUserService userService;
        private IChatService chatService;
        private IChatReportService reportService;
        private ChatListViewModel chatListViewModel;
        private GenerateTransferViewModel generateTransferViewModel;

        public ChatMessagesPage(ChatListViewModel chatListViewModel, Window mainWindow, Frame rightFrame, int chatID, IUserService userService, IChatService chatService, IMessageService messageService, IChatReportService reportService)
        {
            this.InitializeComponent();
            this.SelectedChat = chatID;
            this.chatListViewModel = chatListViewModel;
            this.userService = userService;
            this.chatService = chatService;
            this.reportService = reportService;
            this.rightFrame = rightFrame;
            this.chatMessagesViewModel = new ChatMessagesViewModel(mainWindow, rightFrame, chatID, messageService, chatService, userService, reportService);

            // ?
            if (this.chatMessagesViewModel.TemplateSelector != null)
            {
                this.ChatListView.ItemTemplateSelector = this.chatMessagesViewModel.TemplateSelector;
            }

            this.generateTransferViewModel = new GenerateTransferViewModel(chatService, chatID);
            this.chatMessagesViewModel.ChatListView = this.ChatListView;
            this.chatMessagesViewModel.SetupMessageTracking();

            this.DataContext = this.chatMessagesViewModel;
        }

        public void AddNewMember_Click(object sender, RoutedEventArgs e)
        {
            this.rightFrame.Content = new AddNewMemberPage(this.chatMessagesViewModel, this, this.rightFrame, this.SelectedChat, this.chatService, this.userService);
        }

        public void LeaveChat_Click(object sender, RoutedEventArgs e)
        {
            this.rightFrame.Content = new LeaveChatPage(this.SelectedChat, this.chatListViewModel, this, this.rightFrame, this.chatService, this.userService);
        }

        public void SendTransfer_Click(object sender, RoutedEventArgs e)
        {
            this.rightFrame.Content = new GenerateTransferPage(this.generateTransferViewModel, this, this.rightFrame, this.SelectedChat, this.chatService);
        }
    }
}

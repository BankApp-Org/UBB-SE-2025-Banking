using Common.Services.Social;
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
using Common.Services;

namespace BankAppDesktop.Views.Pages
{
    /// <summary>
    /// A page that hosts the social features, including chat, friends, feed, and notifications.
    /// </summary>
    public sealed partial class SocialMainPage : Page
    {
        private readonly IUserService userService;
        private readonly IChatService chatService;
        private readonly IMessageService messageService;
        private readonly IChatReportService reportService;
        private readonly INotificationService notificationService;

        public SocialMainPage(IUserService us, IChatService cs, IMessageService ms, IChatReportService rs, INotificationService ns)
        {
            this.InitializeComponent();

            this.notificationService = ns;
            this.userService = us;
            this.chatService = cs;
            this.messageService = ms;
            this.reportService = rs;

            if (this.LeftFrame.Content == null || this.LeftFrame.Content is not ChatListComponent)
            {
                var chatListView = new ChatListComponent(this, this.chatService, this.userService, this.reportService, this.messageService, this.RightFrame, this.repo);
                this.LeftFrame.Content = chatListView;
            }

        }

        private void Chat_Click(object sender, RoutedEventArgs e)
        {
            if (this.LeftFrame.Content == null || this.LeftFrame.Content is not ChatListComponent)
            {
                var chatListView = new ChatListComponent(this, this.chatService, this.userService, this.reportService, this.messageService, this.RightFrame, this.repo);
                this.LeftFrame.Content = chatListView;
            }
        }

        private void Friends_Click(object sender, RoutedEventArgs e)
        {
            if (this.LeftFrame.Content == null || this.LeftFrame.Content is not FriendsListPage)
            {
                var friendsListView = new FriendsListPage(this.chatService, this.userService, this.messageService, this.RightFrame);
                this.LeftFrame.Content = friendsListView;
            }
        }

        private void Notifications_Click(object sender, RoutedEventArgs e)
        {
            if (this.RightFrame.Content == null || this.RightFrame.Content is not NotificationPage)
            {
                var notificationView = new NotificationPage(this.chatService, this.notificationService, this.userService);
                this.RightFrame.Content = notificationView;
            }
        }
    }
}
using BankAppDesktop.ViewModels;
using BankAppDesktop.Views.Components;
using Common.Services;
using Common.Services.Bank;
using Common.Services.Social;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
        private readonly IAuthenticationService authenticationService;
        private readonly IBankAccountService bankAccountService;

        public SocialMainPage(IUserService userService, IChatService chatService, IMessageService messageService, IChatReportService reportService, INotificationService notificationService, IAuthenticationService authenticationService, IBankAccountService bankAccountService)
        {
            this.InitializeComponent();

            this.notificationService = notificationService;
            this.userService = userService;
            this.chatService = chatService;
            this.messageService = messageService;
            this.reportService = reportService;
            this.authenticationService = authenticationService;
            this.bankAccountService = bankAccountService;

            if (this.LeftFrame.Content == null || this.LeftFrame.Content is not ChatListComponent)
            {
                var chatListView = new ChatListComponent(this, this.chatService, this.userService, this.reportService, this.messageService, this.RightFrame, this.authenticationService, this.bankAccountService);
                this.LeftFrame.Content = chatListView;
            }
        }

        private void Chat_Click(object sender, RoutedEventArgs e)
        {
            if (this.LeftFrame.Content == null || this.LeftFrame.Content is not ChatListComponent)
            {
                var chatListView = new ChatListComponent(this, this.chatService, this.userService, this.reportService, this.messageService, this.RightFrame, this.authenticationService, this.bankAccountService);
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
            if (this.RightFrame.Content == null || this.RightFrame.Content is not NotificationsPage)
            {
                var notificationsPage = App.Services.GetRequiredService<NotificationsPage>();
                this.RightFrame.Content = notificationsPage;
            }
        }
    }
}
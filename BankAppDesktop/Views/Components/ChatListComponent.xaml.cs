using BankAppDesktop.ViewModels;
using BankAppDesktop.Views.Pages;
using Common.Models.Social;
using Common.Services;
using Common.Services.Bank;
using Common.Services.Social;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

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
        private readonly IAuthenticationService authenticationService;
        private readonly IBankAccountService bankAccountService;
        public Frame RightFrame;
        public Page MainFrame;

        public ChatListComponent(Page mainFrame, IChatService chatService, IUserService userService, IChatReportService reportService, IMessageService messageService, Frame rightFrame, IAuthenticationService authenticationService, IBankAccountService bankAccountService)
        {
            this.InitializeComponent();

            this.MainFrame = mainFrame;
            this.UserService = userService;
            this.ChatService = chatService;
            this.MessageService = messageService;
            this.ReportService = reportService;
            this.RightFrame = rightFrame;
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.bankAccountService = bankAccountService ?? throw new ArgumentNullException(nameof(bankAccountService));
            this.chatListViewModel = new ChatListViewModel(ChatService, UserService);
            this.MainGrid.DataContext = this.chatListViewModel;
        }

        private void CreateChat_Click(object sender, RoutedEventArgs e)
        {
            this.RightFrame.Content = new CreateChatPage(this.chatListViewModel, this.ChatService, this.UserService);
        }

        private void ChatList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ChatList.SelectedItem is Chat selectedChat)
            {
                this.RightFrame.Content = new ChatMessagesPage(this.chatListViewModel, this.MainFrame, this.RightFrame, selectedChat.Id, this.UserService, this.ChatService, this.MessageService, this.ReportService, this.authenticationService, this.bankAccountService);
            }
        }
    }
}

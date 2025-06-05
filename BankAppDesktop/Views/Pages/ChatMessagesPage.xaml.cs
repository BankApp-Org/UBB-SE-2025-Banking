namespace BankAppDesktop.Views.Pages
{
    using BankAppDesktop.ViewModels;
    using Common.Services;
    using Common.Services.Bank;
    using Common.Services.Social;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    public sealed partial class ChatMessagesPage : Page
    {
        public int SelectedChat { get; set; }

        private ChatMessagesViewModel chatMessagesViewModel;
        private Frame rightFrame;
        private IUserService userService;
        private IChatService chatService;
        private IAuthenticationService authenticationService;
        private IBankAccountService bankAccountService;
        private IChatReportService reportService;
        private ChatListViewModel chatListViewModel;
        private GenerateTransferViewModel generateTransferViewModel;

        public ChatMessagesPage(ChatListViewModel chatListViewModel, Page mainWindow, Frame rightFrame, int chatID, IUserService userService, IChatService chatService, IMessageService messageService, IChatReportService reportService, IAuthenticationService authenticationService, IBankAccountService bankAccountService)
        {
            this.InitializeComponent();
            this.SelectedChat = chatID;
            this.chatListViewModel = chatListViewModel;
            this.userService = userService;
            this.chatService = chatService;
            this.authenticationService = authenticationService;
            this.bankAccountService = bankAccountService;
            this.reportService = reportService;
            this.rightFrame = rightFrame;
            this.chatMessagesViewModel = new ChatMessagesViewModel(mainWindow, rightFrame, chatID, messageService, chatService, userService, reportService);

            // ?
            if (this.chatMessagesViewModel.TemplateSelector != null)
            {
                this.ChatListView.ItemTemplateSelector = this.chatMessagesViewModel.TemplateSelector;
            }

            this.generateTransferViewModel = new GenerateTransferViewModel(chatService, authenticationService, bankAccountService, chatID);
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

namespace BankAppDesktop.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using BankAppDesktop.ViewModels;
    using BankApi.Services.Social;
    using BankApi.Services;
    using Common.Services.Social;
    using Common.Services;

    public sealed partial class AddNewMemberPage : Page
    {
        private IChatService chatService;
        private IUserService userService;
        private AddNewMemberViewModel addNewMemberViewModel;
        private Page lastChat;
        private Frame rightFrame;
        private ChatMessagesViewModel chatMessagesViewModel;

        public AddNewMemberPage(ChatMessagesViewModel chatMessagesViewModel, Page lastChat, Frame rightFrame, int chatID, IChatService chatService, IUserService userService)
        {
            this.InitializeComponent();
            this.chatMessagesViewModel = chatMessagesViewModel;
            this.lastChat = lastChat;
            this.rightFrame = rightFrame;
            this.chatService = chatService;
            this.userService = userService;
            this.addNewMemberViewModel = new AddNewMemberViewModel(chatMessagesViewModel, lastChat, chatID, chatService, userService);

            this.DataContext = this.addNewMemberViewModel;
        }

        public void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.rightFrame.Content = this.lastChat;
        }
    }
}

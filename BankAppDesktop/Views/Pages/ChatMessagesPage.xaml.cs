namespace BankAppDesktop.Views.Pages
{
    using BankAppDesktop.ViewModels;
    using BankAppDesktop.Views.Dialogs;
    using Common.Models.Bank;
    using Common.Models.Social;
    using Common.Services;
    using Common.Services.Bank;
    using Common.Services.Social;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Controls.Primitives;
    using System;
    using System.Linq;

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
        private IMessageService messageService;
        private GenerateTransferViewModel generateTransferViewModel;

        public ChatMessagesPage(ChatListViewModel chatListViewModel, Page mainWindow, Frame rightFrame, int chatID, IUserService userService, IChatService chatService, IMessageService messageService, IChatReportService reportService, IAuthenticationService authenticationService, IBankAccountService bankAccountService)
        {
            this.InitializeComponent();
            this.SelectedChat = chatID;
            this.chatListViewModel = chatListViewModel;
            this.userService = userService;
            this.chatService = chatService;
            this.messageService = messageService;
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
        public async void AddNewMember_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddNewMemberDialog(this.chatMessagesViewModel, this.SelectedChat, this.chatService, this.userService)
            {
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
        public async void LeaveChat_Click(object sender, RoutedEventArgs e)
        {
            var leaveChatViewModel = new LeaveChatViewModel(this.userService, this.chatService, this.chatListViewModel, this.SelectedChat);
            var dialog = new LeaveChatDialog(leaveChatViewModel)
            {
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        public void SendTransfer_Click(object sender, RoutedEventArgs e)
        {
            this.rightFrame.Content = new GenerateTransferPage(this.generateTransferViewModel, this, this.rightFrame, this.SelectedChat, this.chatService);
        }
        public void MessageTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton button && button.Tag is string messageType)
            {
                // Reset all buttons
                TextTypeButton.IsChecked = false;
                ImageTypeButton.IsChecked = false;
                TransferTypeButton.IsChecked = false;
                RequestTypeButton.IsChecked = false;
                BillSplitTypeButton.IsChecked = false;

                // Set active button
                button.IsChecked = true;

                // Update button styles
                UpdateButtonStyles();

                // Hide all input grids
                TextInputGrid.Visibility = Visibility.Collapsed;
                ImageInputGrid.Visibility = Visibility.Collapsed;
                TransferInputGrid.Visibility = Visibility.Collapsed;
                RequestInputGrid.Visibility = Visibility.Collapsed;
                BillSplitInputGrid.Visibility = Visibility.Collapsed;

                // Show appropriate input grid
                switch (messageType)
                {
                    case "Text":
                        TextInputGrid.Visibility = Visibility.Visible;
                        break;
                    case "Image":
                        ImageInputGrid.Visibility = Visibility.Visible;
                        break;
                    case "Transfer":
                        TransferInputGrid.Visibility = Visibility.Visible;
                        break;
                    case "Request":
                        RequestInputGrid.Visibility = Visibility.Visible;
                        break;
                    case "BillSplit":
                        BillSplitInputGrid.Visibility = Visibility.Visible;
                        break;
                }
            }
        }
        public async void SendRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(RequestAmountBox.Text) || string.IsNullOrWhiteSpace(RequestDescriptionBox.Text))
                {
                    // Show error message
                    return;
                }

                if (!decimal.TryParse(RequestAmountBox.Text, out decimal amount))
                {
                    // Show error message
                    return;
                }

                var currencyString = ((ComboBoxItem)RequestCurrencyBox.SelectedItem).Content.ToString();
                if (!Enum.TryParse<Currency>(currencyString, out Currency currency))
                {
                    // Show error message
                    return;
                }

                var user = await userService.GetCurrentUserAsync();
                var requestMessage = new RequestMessage
                {
                    UserId = user.Id,
                    ChatId = SelectedChat,
                    Description = RequestDescriptionBox.Text,
                    Amount = amount,
                    Currency = currency,
                    CreatedAt = DateTime.UtcNow,
                    Type = MessageType.Request.ToString(),
                    MessageType = MessageType.Request,
                    Status = "Pending"
                };

                await messageService.SendMessageAsync(SelectedChat, user, requestMessage);

                // Trigger message refresh
                chatMessagesViewModel.LoadAllMessagesForChat();

                // Clear inputs
                RequestAmountBox.Text = string.Empty;
                RequestDescriptionBox.Text = string.Empty;
            }
            catch (Exception ex)
            {
                // Handle error - could show a dialog or log
            }
        }
        public async void SendBillSplit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(BillSplitAmountBox.Text) || string.IsNullOrWhiteSpace(BillSplitDescriptionBox.Text))
                {
                    // Show error message
                    return;
                }

                if (!decimal.TryParse(BillSplitAmountBox.Text, out decimal amount))
                {
                    // Show error message
                    return;
                }

                var currencyString = ((ComboBoxItem)BillSplitCurrencyBox.SelectedItem).Content.ToString();
                if (!Enum.TryParse<Currency>(currencyString, out Currency currency))
                {
                    // Show error message
                    return;
                }

                var user = await userService.GetCurrentUserAsync();
                var chat = await chatService.GetChatById(SelectedChat);

                // Create bill split with all chat participants except the sender
                var participants = chat.Users.Where(u => u.Id != user.Id).ToList();

                var billSplitMessage = new BillSplitMessage(
                    user.Id,
                    SelectedChat,
                    BillSplitDescriptionBox.Text,
                    amount,
                    currency,
                    DateTime.UtcNow,
                    participants);

                await messageService.SendMessageAsync(SelectedChat, user, billSplitMessage);

                // Trigger message refresh
                chatMessagesViewModel.LoadAllMessagesForChat();

                // Clear inputs
                BillSplitAmountBox.Text = string.Empty;
                BillSplitDescriptionBox.Text = string.Empty;
            }
            catch (Exception ex)
            {
                // Handle error - could show a dialog or log
            }
        }

        private void UpdateButtonStyles()
        {
            // Update button backgrounds based on selection
            var selectedBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.SteelBlue);
            var defaultBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray);

            TextTypeButton.Background = TextTypeButton.IsChecked == true ? selectedBrush : defaultBrush;
            ImageTypeButton.Background = ImageTypeButton.IsChecked == true ? selectedBrush : defaultBrush;
            TransferTypeButton.Background = TransferTypeButton.IsChecked == true ? selectedBrush : defaultBrush;
            RequestTypeButton.Background = RequestTypeButton.IsChecked == true ? selectedBrush : defaultBrush;
            BillSplitTypeButton.Background = BillSplitTypeButton.IsChecked == true ? selectedBrush : defaultBrush;
        }
    }
}

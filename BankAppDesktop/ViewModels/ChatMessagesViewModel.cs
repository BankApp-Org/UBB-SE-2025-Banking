namespace BankAppDesktop.ViewModels
{
    using BankAppDesktop.Commands;
    using BankAppDesktop.Views.Components;
    using Common.Helper;
    using Common.Models;
    using Common.Models.Bank;
    using Common.Models.Social;
    using Common.Services;
    using Common.Services.Impl;
    using Common.Services.Social;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows.Input;
    using Windows.Storage;

    public partial class ChatMessagesViewModel : INotifyPropertyChanged
    {
        private readonly Page page;

        public ObservableCollection<Message> ChatMessages { get; set; }

        public ListView ChatListView { get; set; } = null!;

        public IMessageService MessageService;
        public IChatService ChatService;
        public IUserService UserService;
        public IChatReportService ReportService;
        private ImgurImageUploader imgurImageUploader = App.Host.Services.GetService<ImgurImageUploader>() ?? throw new InvalidOperationException("ImgurImageUploader not found in services.");
        private MessageTemplateSelector templateSelector;

        // move message template selector
        public MessageTemplateSelector TemplateSelector => this.templateSelector;

        public int CurrentChatID { get; set; }

        public int CurrentUserID { get; set; }

        public string CurrentChatName { get; set; } = string.Empty;

        // Currently selected message type
        private MessageType selectedMessageType = MessageType.Text;
        public MessageType SelectedMessageType
        {
            get => selectedMessageType;
            set
            {
                if (selectedMessageType != value)
                {
                    selectedMessageType = value;
                    OnPropertyChanged(nameof(SelectedMessageType));
                }
            }
        }

        // For message polling
        private Timer? messagePollingTimer;
        private DateTime lastMessageTimestamp = DateTime.MinValue;
        private const int POLLING_INTERVAL = 2000; // 2 seconds

        public string CurrentChatParticipantsString => string.Join(", ", this.CurrentChatParticipants ?? []);

        private List<User> currentChatParticipants = [];

        public List<User> CurrentChatParticipants
        {
            get => this.currentChatParticipants;
            set
            {
                if (this.currentChatParticipants != value)
                {
                    this.currentChatParticipants = value;
                    this.OnPropertyChanged(nameof(this.CurrentChatParticipants));
                    this.OnPropertyChanged(nameof(this.CurrentChatParticipantsString));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string messageContent = string.Empty;

        public string MessageContent
        {
            get => this.messageContent;
            set
            {
                if (this.messageContent != value)
                {
                    this.messageContent = value;
                    this.OnPropertyChanged(nameof(this.MessageContent));
                    this.OnPropertyChanged(nameof(this.RemainingCharacterCount));
                }
            }
        }

        // Properties for request, transfer and bill split messages
        private string description = string.Empty;
        public string Description
        {
            get => this.description;
            set
            {
                if (this.description != value)
                {
                    this.description = value;
                    this.OnPropertyChanged(nameof(this.Description));
                }
            }
        }

        private decimal amount;
        public decimal Amount
        {
            get => this.amount;
            set
            {
                if (this.amount != value)
                {
                    this.amount = value;
                    this.OnPropertyChanged(nameof(this.Amount));
                }
            }
        }

        private Currency currency = Currency.USD;
        public Currency Currency
        {
            get => this.currency;
            set
            {
                if (this.currency != value)
                {
                    this.currency = value;
                    this.OnPropertyChanged(nameof(this.Currency));
                }
            }
        }

        public int RemainingCharacterCount => 256 - (this.MessageContent?.Length ?? 0);

        public ICommand SendMessageCommand { get; }

        private async void SendMessage()
        {
            var user = await UserService.GetCurrentUserAsync();

            // Create a message based on the selected message type
            Message message;

            switch (SelectedMessageType)
            {
                case MessageType.Text:
                    string convertedContent = EmoticonConverter.ConvertEmoticonsToEmojis(this.MessageContent);
                    message = new TextMessage
                    {
                        MessageContent = convertedContent,
                        UserId = user.Id,
                        ChatId = this.CurrentChatID,
                        CreatedAt = DateTime.UtcNow,
                        Sender = user,
                        Type = MessageType.Text.ToString(),
                        MessageType = MessageType.Text,
                        UsersReport = []
                    };
                    break;

                case MessageType.Image:
                    // Image messages are handled separately by SendImage method
                    // This is just a fallback
                    message = new ImageMessage
                    {
                        ImageUrl = this.MessageContent, // This shouldn't typically happen
                        UserId = user.Id,
                        ChatId = this.CurrentChatID,
                        CreatedAt = DateTime.UtcNow,
                        Sender = user,
                        Type = MessageType.Image.ToString(),
                        MessageType = MessageType.Image,
                        UsersReport = []
                    };
                    break;

                case MessageType.Transfer:
                    message = new TransferMessage
                    {
                        UserId = user.Id,
                        ChatId = this.CurrentChatID,
                        CreatedAt = DateTime.UtcNow,
                        Status = "Pending",
                        Amount = this.Amount,
                        Description = this.Description,
                        Currency = this.Currency,
                        Sender = user,
                        Type = MessageType.Transfer.ToString(),
                        MessageType = MessageType.Transfer,
                        ListOfReceivers = []
                    };
                    break;

                case MessageType.Request:
                    message = new RequestMessage
                    {
                        UserId = user.Id,
                        ChatId = this.CurrentChatID,
                        CreatedAt = DateTime.UtcNow,
                        Status = "Pending",
                        Amount = this.Amount,
                        Description = this.Description,
                        Currency = this.Currency,
                        Sender = user,
                        Type = MessageType.Request.ToString(),
                        MessageType = MessageType.Request
                    };
                    break;

                case MessageType.BillSplit:
                    // Get all chat participants except the current user
                    var chat = await ChatService.GetChatById(this.CurrentChatID);
                    var participants = chat.Users.Where(u => u.Id != user.Id).ToList();

                    message = new BillSplitMessage
                    {
                        UserId = user.Id,
                        ChatId = this.CurrentChatID,
                        CreatedAt = DateTime.UtcNow,
                        Description = this.Description,
                        TotalAmount = this.Amount,
                        Currency = this.Currency,
                        Participants = participants,
                        Status = "Pending",
                        Sender = user,
                        Type = MessageType.BillSplit.ToString(),
                        MessageType = MessageType.BillSplit
                    };
                    break;

                default:
                    throw new ArgumentException($"Unsupported message type: {SelectedMessageType}");
            }

            await this.MessageService.SendMessageAsync(this.CurrentChatID, user, message);

            // Force loading after sending
            this.CheckForNewMessages();

            // Clear inputs
            this.MessageContent = string.Empty;
            this.Description = string.Empty;
            this.Amount = 0;
        }

        public ICommand SendImageCommand { get; }

        private async void SendImage()
        {
            Windows.Storage.Pickers.FileOpenPicker openFileDialog = new()
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
                FileTypeFilter =
                {
                    ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"
                },
            };

            // Replace the following line:
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(openFileDialog, hwnd);

            StorageFile? file = await openFileDialog.PickSingleFileAsync();

            if (file != null)
            {
                // Convert StorageFile to a stream and then to IFormFile
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    var formFile = new FormFile(stream, 0, stream.Length, file.Name, file.Name)
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = file.ContentType
                    };
                    string imageUrl = await imgurImageUploader.UploadImageAndGetUrl(formFile);
                    var user = await this.UserService.GetCurrentUserAsync();
                    Message message = new ImageMessage
                    {
                        ImageUrl = imageUrl,
                        UserId = user.Id,
                        ChatId = this.CurrentChatID,
                        CreatedAt = DateTime.UtcNow,
                        Sender = user,
                        Type = MessageType.Image.ToString(),
                        MessageType = MessageType.Image,
                        UsersReport = []
                    };
                    await this.MessageService.SendMessageAsync(this.CurrentUserID, user, message);

                    // Force loading after sending
                    this.CheckForNewMessages();
                }
            }
        }

        public void ScrollToBottom()
        {
            if (this.ChatListView != null)
            {
                this.ChatListView.DispatcherQueue.TryEnqueue(() =>
                {
                    var scrollViewer = this.FindVisualChild<ScrollViewer>(this.ChatListView);
                    scrollViewer?.ChangeView(null, scrollViewer.ScrollableHeight, null);
                });
            }
        }

        public ICommand DeleteMessageCommand { get; set; } = null!;

        private async void DeleteMessage(Message message)
        {
            var user = await this.UserService.GetCurrentUserAsync();
            var chatID = this.CurrentChatID;
            await this.MessageService.DeleteMessageAsync(chatID, message.ChatId, user);
            this.LoadAllMessagesForChat();
        }

        public ICommand ReportMessageCommand { get; set; } = null!;

        private async void ReportMessage(Message message)
        {
            ReportViewModel reportViewModel = new(
                App.Host.Services.GetService<IMessageService>() ?? throw new InvalidOperationException("MessageService not found"),
                App.Host.Services.GetService<IAuthenticationService>() ?? throw new InvalidOperationException("AuthenticationService not found"),
                App.Host.Services.GetService<IUserService>() ?? throw new InvalidOperationException("UserService not found"),
                this.CurrentChatID,
                message.Id,
                message.UserId
            );

            var reportDialog = new Views.Dialogs.ReportDialog(reportViewModel)
            {
                XamlRoot = this.page.XamlRoot
            };

            await reportDialog.ShowAsync();
        }

        private T? FindVisualChild<T>(DependencyObject parent)
            where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                T? childOfChild = this.FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }

            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatMessagesViewModel"/> class.
        /// </summary>
        /// <param name="window">The page instance.</param>
        /// <param name="rightFrame">The right frame instance.</param>
        /// <param name="currentChatID">The current chat ID.</param>
        /// <param name="msgService">The message service instance.</param>
        /// <param name="chtService">The chat service instance.</param>
        /// <param name="usrService">The user service instance.</param>
        /// <param name="ReportService">The report service instance.</param>
        public ChatMessagesViewModel(Page page, Frame rightFrame, int currentChatID, IMessageService msgService, IChatService chtService, IUserService usrService, IChatReportService reportService)
        {
            this.page = page;
            this.ChatMessages = [];
            this.MessageService = msgService;
            this.ChatService = chtService;
            this.UserService = usrService;
            this.ReportService = reportService;
            this.CurrentChatID = currentChatID;
            this.templateSelector = new MessageTemplateSelector(App.Host.Services.GetService<IAuthenticationService>() ?? throw new InvalidOperationException("AuthenticationService not found"))
            {
                TextMessageTemplateLeft = App.Current.Resources["TextMessageTemplateLeft"] as DataTemplate ?? throw new InvalidOperationException("TextMessageTemplateLeft not found"),
                TextMessageTemplateRight = App.Current.Resources["TextMessageTemplateRight"] as DataTemplate ?? throw new InvalidOperationException("TextMessageTemplateRight not found"),
                ImageMessageTemplateLeft = App.Current.Resources["ImageMessageTemplateLeft"] as DataTemplate ?? throw new InvalidOperationException("ImageMessageTemplateLeft not found"),
                ImageMessageTemplateRight = App.Current.Resources["ImageMessageTemplateRight"] as DataTemplate ?? throw new InvalidOperationException("ImageMessageTemplateRight not found"),
                TransferMessageTemplateLeft = App.Current.Resources["TransferMessageTemplateLeft"] as DataTemplate ?? throw new InvalidOperationException("TransferMessageTemplateLeft not found"),
                TransferMessageTemplateRight = App.Current.Resources["TransferMessageTemplateRight"] as DataTemplate ?? throw new InvalidOperationException("TransferMessageTemplateRight not found"),
                RequestMessageTemplateLeft = App.Current.Resources["RequestMessageTemplateLeft"] as DataTemplate ?? throw new InvalidOperationException("RequestMessageTemplateLeft not found"),
                RequestMessageTemplateRight = App.Current.Resources["RequestMessageTemplateRight"] as DataTemplate ?? throw new InvalidOperationException("RequestMessageTemplateRight not found"),
                BillSplitMessageTemplateLeft = App.Current.Resources["BillSplitMessageTemplateLeft"] as DataTemplate ?? throw new InvalidOperationException("BillSplitMessageTemplateLeft not found"),
                BillSplitMessageTemplateRight = App.Current.Resources["BillSplitMessageTemplateRight"] as DataTemplate ?? throw new InvalidOperationException("BillSplitMessageTemplateRight not found"),
            };

            this.SendMessageCommand = new RelayCommand(parameter => this.SendMessage());
            this.SendImageCommand = new RelayCommand(parameter => this.SendImage());
            this.DeleteMessageCommand = new RelayCommand(parameter => this.DeleteMessage((Message)parameter));
            this.ReportMessageCommand = new RelayCommand(parameter => this.ReportMessage((Message)parameter));
            LoadUserIdChatStuff();

            // Initial load of messages
            // this.LoadAllMessagesForChat();
            this.ScrollToBottom();

            // Start polling for new messages
            this.StartMessagePolling();
            this.SetupMessageTracking();    // ?????
        }

        private async void LoadUserIdChatStuff()
        {
            var mata = await this.UserService.GetCurrentUserAsync();
            this.CurrentUserID = mata.Id;
            var chat = await this.ChatService.GetChatById(this.CurrentChatID);
            this.CurrentChatName = chat.ChatName;

            var participants = await this.ChatService.GetChatById(this.CurrentChatID);
            this.CurrentChatParticipants = participants.Users;
        }

        // Initial load of all messages
        public async void LoadAllMessagesForChat()
        {
            this.ChatMessages.Clear();
            var chat = await ChatService.GetChatById(this.CurrentChatID);
            var messages = chat.Messages;

            foreach (var message in messages)
            {
                this.AddMessageToChat(message);
            }

            // Update the last message timestamp
            if (this.ChatMessages.Any())
            {
                this.lastMessageTimestamp = this.ChatMessages.Max(m => m.CreatedAt);
            }

            this.ScrollToBottom();
        }

        // Start polling for new messages
        private void StartMessagePolling()
        {
            // Dispose of existing timer if it exists
            this.messagePollingTimer?.Dispose();

            // Create new timer that checks for new messages
            this.messagePollingTimer = new Timer(
                _ => this.ChatListView?.DispatcherQueue.TryEnqueue(() => this.CheckForNewMessages()),
                null,
                0,
                POLLING_INTERVAL);
        }

        // Stop polling for messages
        public void StopMessagePolling()
        {
            this.messagePollingTimer?.Dispose();
            this.messagePollingTimer = null;
        }

        // Check for new messages by comparing timestamps
        private async void CheckForNewMessages()
        {
            var chat = await ChatService.GetChatById(this.CurrentChatID);
            var messages = chat.Messages;
            bool hasNewMessages = false;

            foreach (var message in messages)
            {
                // If the message timestamp is newer than the last message we processed
                if (message.CreatedAt > this.lastMessageTimestamp)
                {
                    this.AddMessageToChat(message);
                    hasNewMessages = true;

                    // Update the last message timestamp if this is newer
                    if (message.CreatedAt > this.lastMessageTimestamp)
                    {
                        this.lastMessageTimestamp = message.CreatedAt;
                    }
                }
            }

            // Only scroll if we added new messages
            if (hasNewMessages)
            {
                this.ScrollToBottom();
            }
        }

        // Helper method to add a message to the chat
        private async void AddMessageToChat(Message message)
        {
            if (message is ImageMessage)
            {
                Console.WriteLine($"Image message found with URL: {((ImageMessage)message).ImageUrl}");
            }
            this.ChatMessages.Add(message);
        }

        // Public method to refresh messages
        public void RefreshMessages()
        {
            this.CheckForNewMessages();
        }

        public void SetupMessageTracking()
        {
            this.ChatMessages.CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add ||
                    e.Action == NotifyCollectionChangedAction.Reset)
                {
                    this.ScrollToBottom();
                }
            };
        }

        // Important: Call this method when the view is being unloaded or when navigating away
        public void Cleanup()
        {
            this.StopMessagePolling();
        }

        // Method to set the selected message type
        public void SetMessageType(MessageType messageType)
        {
            this.SelectedMessageType = messageType;
        }
    }
}

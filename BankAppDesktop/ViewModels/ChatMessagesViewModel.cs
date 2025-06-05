using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAppDesktop.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.Intrinsics.X86;
    using System.Threading;
    using System.Windows.Input;
    using BankAppDesktop.Commands;
    using Common.Models;
    using Common.Models.Social;
    using Common.Services;
    using Common.Services.Social;
    using Microsoft.UI.Windowing;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media;
    using Windows.Media.AppBroadcasting;
    using Windows.Storage;
    using Windows.Storage.Pickers;
    using WinRT.Interop;
    internal class ChatMessagesViewModel : INotifyPropertyChanged
    {
        private readonly Window window;

        public ObservableCollection<Message> ChatMessages { get; set; }

        public ListView ChatListView { get; set; } = null!;

        public IMessageService MessageService;
        public IChatService ChatService;
        public IUserService UserService;
        public IChatReportService ReportService;
        private MessageTemplateSelector templateSelector;

        // move message template selector
        public MessageTemplateSelector TemplateSelector => this.templateSelector;

        public int CurrentChatID { get; set; }

        public int CurrentUserID { get; set; }

        public string CurrentChatName { get; set; }

        // For message polling
        private Timer? messagePollingTimer;
        private DateTime lastMessageTimestamp = DateTime.MinValue;
        private const int POLLING_INTERVAL = 2000; // 2 seconds

        public string CurrentChatParticipantsString => string.Join(", ", this.CurrentChatParticipants ?? new List<string>());

        private List<string> currentChatParticipants = new List<string>();

        public List<string> CurrentChatParticipants
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

        public int RemainingCharacterCount => 256 - (this.MessageContent?.Length ?? 0);

        public ICommand SendMessageCommand { get; }

        private async void SendMessage()
        {
            string convertedContent = EmoticonConverter.ConvertEmoticonsToEmojis(this.MessageContent);
            var user = await UserService.GetCurrentUserAsync();

            // Create a new Message object to pass as the third argument
            Message message = new Message
            {
                MessageContent = convertedContent,
                UserId = user.Id,
                ChatId = this.CurrentChatID,
                CreatedAt = DateTime.UtcNow,
                Sender = user
            };

            await this.MessageService.SendMessageAsync(this.CurrentChatID, user, message);

            // Force loading after sending
            this.CheckForNewMessages();

            this.MessageContent = string.Empty;
        }

        public ICommand SendImageCommand { get; }

        private async void SendImage()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            };

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var hwnd = WindowNative.GetWindowHandle(this.window);
            InitializeWithWindow.Initialize(picker, hwnd);

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                string imageUrl = await ImgurImageUploader.UploadImageAndGetUrl(file);
                var user = await this.UserService.GetCurrentUserAsync();
                Message message = new ImageMessage
                {
                    ImageUrl = imageUrl,
                    UserId = user.Id,
                    ChatId = this.CurrentChatID,
                    CreatedAt = DateTime.UtcNow,
                    Sender = user
                };
                await this.MessageService.SendMessageAsync(this.CurrentUserID, user, message);

                // this.LoadAllMessagesForChat();
                // force loading after
                this.CheckForNewMessages();
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

        public ICommand DeleteMessageCommand { get; set; }

        private async void DeleteMessage(Message message)
        {
            var user = await this.UserService.GetCurrentUserAsync();
            var chatID = this.CurrentChatID;
            this.MessageService.DeleteMessageAsync(chatID, message.ChatId, user);
            this.LoadAllMessagesForChat();
        }

        public ICommand ReportMessageCommand { get; set; }

        private void ReportMessage(Message message)
        {
            // waiting for report view to be done
            ReportView reportView = new ReportView(this.UserService, this.ReportService, message.Sender.Id, message.Id);
            reportView.Activate();
        }

        private T FindVisualChild<T>(DependencyObject parent)
            where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                T childOfChild = this.FindVisualChild<T>(child);
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
        /// <param name="window">The window instance.</param>
        /// <param name="rightFrame">The right frame instance.</param>
        /// <param name="currentChatID">The current chat ID.</param>
        /// <param name="msgService">The message service instance.</param>
        /// <param name="chtService">The chat service instance.</param>
        /// <param name="usrService">The user service instance.</param>
        /// <param name="ReportService">The report service instance.</param>
        public ChatMessagesViewModel(Window window, Frame rightFrame, int currentChatID, IMessageService msgService, IChatService chtService, IUserService usrService, IChatReportService reportService)
        {
            this.window = window;
            this.ChatMessages = new ObservableCollection<Message>();
            this.MessageService = msgService;
            this.ChatService = chtService;
            this.UserService = usrService;
            this.ReportService = reportService;
            this.CurrentChatID = currentChatID;
            this.templateSelector = new MessageTemplateSelector(UserService)
            {
                // //TextMessageTemplateLeft = (DataTemplate)App.Current.Resources["TextMessageTemplateLeft"],
                //    //TextMessageTemplateRight = (DataTemplate)App.Current.Resources["TextMessageTemplateRight"],
                //    //ImageMessageTemplateLeft = (DataTemplate)App.Current.Resources["ImageMessageTemplateLeft"],
                //    //ImageMessageTemplateRight = (DataTemplate)App.Current.Resources["ImageMessageTemplateRight"],
                //    //TransferMessageTemplateLeft = (DataTemplate)App.Current.Resources["TransferMessageTemplateLeft"],
                //    //TransferMessageTemplateRight = (DataTemplate)App.Current.Resources["TransferMessageTemplateRight"],
                //    //RequestMessageTemplateLeft = (DataTemplate)App.Current.Resources["RequestMessageTemplateLeft"],
                //    //RequestMessageTemplateRight = (DataTemplate)App.Current.Resources["RequestMessageTemplateRight"],
                TextMessageTemplateLeft = App.Current.Resources["TextMessageTemplateLeft"] as DataTemplate ?? throw new InvalidOperationException("TextMessageTemplateLeft not found"),
                TextMessageTemplateRight = App.Current.Resources["TextMessageTemplateRight"] as DataTemplate ?? throw new InvalidOperationException("TextMessageTemplateRight not found"),
                ImageMessageTemplateLeft = App.Current.Resources["ImageMessageTemplateLeft"] as DataTemplate ?? throw new InvalidOperationException("ImageMessageTemplateLeft not found"),
                ImageMessageTemplateRight = App.Current.Resources["ImageMessageTemplateRight"] as DataTemplate ?? throw new InvalidOperationException("ImageMessageTemplateRight not found"),
                TransferMessageTemplateLeft = App.Current.Resources["TransferMessageTemplateLeft"] as DataTemplate ?? throw new InvalidOperationException("TransferMessageTemplateLeft not found"),
                TransferMessageTemplateRight = App.Current.Resources["TransferMessageTemplateRight"] as DataTemplate ?? throw new InvalidOperationException("TransferMessageTemplateRight not found"),
                RequestMessageTemplateLeft = App.Current.Resources["RequestMessageTemplateLeft"] as DataTemplate ?? throw new InvalidOperationException("RequestMessageTemplateLeft not found"),
                RequestMessageTemplateRight = App.Current.Resources["RequestMessageTemplateRight"] as DataTemplate ?? throw new InvalidOperationException("RequestMessageTemplateRight not found"),
            };

            this.SendMessageCommand = new RelayCommand(parameter => this.SendMessage());
            this.SendImageCommand = new RelayCommand(parameter => this.SendImage());
            this.DeleteMessageCommand = new RelayCommand(parameter => this.DeleteMessage((Message)parameter));
            this.DeleteMessageCommand = new RelayCommand(parameter => this.ReportMessage((Message)parameter));
            LoadUserIdChatStuff();
            this.templateSelector.CurrentUserID = this.CurrentUserID;

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
            if (this.templateSelector != null)
            {
                this.templateSelector.InitializeCurrentUserId(this.CurrentUserID); // Set CurrentUserID after async fetch
            }
            var chat = await this.ChatService.GetChatById(this.CurrentChatID);
            this.CurrentChatName = chat.ChatName;

            var participants = await this.ChatService.GetChatById(this.CurrentChatID);
            this.CurrentChatParticipants = participants.Users.Select(user => $"{user.FirstName} {user.LastName}").ToList();
        }

        // Initial load of all messages
        private async void LoadAllMessagesForChat()
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
            //// Process message based on its type
            // if (message is TextMessage textMessage)
            // {
            //    TextMessage newTextMessage = new TextMessage(
            //        textMessage.GetMessageID(),
            //        textMessage.GetSenderID(),
            //        textMessage.GetChatID(),
            //        textMessage.GetTimestamp(),
            //        textMessage.GetContent(),
            //        textMessage.GetUsersReport());
            //    var user = await this.UserService.GetUserById(textMessage.GetSenderID());
            //    newTextMessage.SenderUsername = user.GetUsername();
            //    this.ChatMessages.Add(newTextMessage);
            // }
            // else if (message is ImageMessage imageMessage)
            // {
            //    ImageMessage newImageMessage = new ImageMessage(
            //        imageMessage.GetMessageID(),
            //        imageMessage.GetSenderID(),
            //        imageMessage.GetChatID(),
            //        imageMessage.GetTimestamp(),
            //        imageMessage.GetImageURL(),
            //        imageMessage.GetUsersReport());
            //    var user = await this.UserService.GetUserById(newImageMessage.GetSenderID());
            //    newImageMessage.SenderUsername = user.GetUsername();
            //    this.ChatMessages.Add(newImageMessage);
            // }
            // else if (message is TransferMessage transferMessage)
            // {
            //    TransferMessage newTransferMessage = new TransferMessage(
            //        transferMessage.GetMessageID(),
            //        transferMessage.GetSenderID(),
            //        transferMessage.GetChatID(),
            //        transferMessage.GetStatus(),
            //        transferMessage.GetAmount(),
            //        transferMessage.GetDescription(),
            //        transferMessage.GetCurrency());
            //    var user = await this.UserService.GetUserById(newTransferMessage.GetSenderID());
            //    newTransferMessage.SenderUsername = user.GetUsername();
            //    this.ChatMessages.Add(newTransferMessage);
            // }
            // else if (message is RequestMessage requestMessage)
            // {
            //    RequestMessage newRequestMessage = new RequestMessage(
            //        requestMessage.GetMessageID(),
            //        requestMessage.GetSenderID(),
            //        requestMessage.GetChatID(),
            //        requestMessage.GetStatus(),
            //        requestMessage.GetAmount(),
            //        requestMessage.GetDescription(),
            //        requestMessage.GetCurrency());
            //    var user = await this.UserService.GetUserById(newRequestMessage.GetSenderID());
            //    newRequestMessage.SenderUsername = user.GetUsername();
            //    this.ChatMessages.Add(newRequestMessage);
            // }

            // simplified???
            var users = await this.UserService.GetUsers();
            // filtering the user from the userlist
            User user = users.FirstOrDefault(u => u.Id == message.UserId)
                ?? throw new InvalidOperationException($"User with ID {message.UserId} not found");
            message.Sender = user;
            message.Type = message switch
            {
                TextMessage _ => MessageType.Text,
                ImageMessage _ => MessageType.Image,
                TransferMessage _ => MessageType.Transfer,
                RequestMessage _ => MessageType.Request,
                _ => throw new InvalidOperationException($"Unknown message type for message ID {message.Id}")
            };
            this.ChatMessages.Add(message);
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
    }
}

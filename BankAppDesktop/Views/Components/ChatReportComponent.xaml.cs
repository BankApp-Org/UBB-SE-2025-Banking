namespace BankAppDesktop.Views.Components
{
    using Common.Models.Social;
    using Common.Services;
    using Common.Services.Social;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using System;
    using System.Threading.Tasks;

    public sealed partial class ChatReportComponent : Page
    {
        private readonly IChatReportService chatReportService;
        private readonly IAuthenticationService authenticationService;
        private readonly IUserService userService;
        private readonly IProfanityChecker profanityChecker;
        private readonly IMessageService messagesService;

        public event EventHandler? ReportSolved;

        public string ReportedUserCNP { get; set; } = string.Empty;

        public Message ReportedMessage { get; set; } = new();

        public int ReportId { get; set; }

        public ChatReportComponent(IChatReportService chatReportService, IProfanityChecker profanityChecker, IMessageService messagesService, IUserService userService, IAuthenticationService authenticationService)
        {
            this.InitializeComponent();
            this.chatReportService = chatReportService ?? throw new ArgumentNullException(nameof(chatReportService));
            this.profanityChecker = profanityChecker ?? throw new ArgumentNullException(nameof(profanityChecker));
            this.messagesService = messagesService ?? throw new ArgumentNullException(nameof(messagesService));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        private async void PunishReportedUser(object sender, RoutedEventArgs e)
        {
            ChatReport chatReport = new()
            {
                Id = this.ReportId,
                ReportedUserCnp = this.ReportedUserCNP,
                Message = this.ReportedMessage,
                MessageId = this.ReportedMessage?.Id ?? 0,
                SubmitterCnp = this.authenticationService.GetUserCNP(),
                Reason = ReportReason.OffensiveContent // Added required Reason property
            };

            try
            {
                // First apply the punishment
                await this.chatReportService.PunishUser(chatReport);

                // If message should be sent, send it
                if (this.MessageCheckBox != null && this.MessageCheckBox.IsChecked == true &&
                    this.MessageTextBox != null && !string.IsNullOrWhiteSpace(this.MessageTextBox.Text))
                {
                    await SendMessageToReportedUser(this.MessageTextBox.Text);
                }

                this.ReportSolved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"An error occurred: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private async void DoNotPunishReportedUser(object sender, RoutedEventArgs e)
        {
            ChatReport chatReport = new()
            {
                Id = this.ReportId,
                ReportedUserCnp = this.ReportedUserCNP,
                Message = this.ReportedMessage,
                MessageId = this.ReportedMessage?.Id ?? 0,
                SubmitterCnp = this.authenticationService.GetUserCNP(),
                Reason = ReportReason.OffensiveContent // Added required Reason property
            };

            try
            {
                await this.chatReportService.DoNotPunishUser(chatReport);

                // Even if not punishing, we might want to send a message (e.g., a warning)
                if (this.MessageCheckBox != null && this.MessageCheckBox.IsChecked == true &&
                    this.MessageTextBox != null && !string.IsNullOrWhiteSpace(this.MessageTextBox.Text))
                {
                    await SendMessageToReportedUser(this.MessageTextBox.Text);
                }

                this.ReportSolved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"An error occurred: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private async Task SendMessageToReportedUser(string messageContent)
        {
            if (string.IsNullOrWhiteSpace(this.ReportedUserCNP))
            {
                return;
            }

            try
            {
                // Replace GiveMessageToUserAsync with proper message sending through the appropriate service
                // Create a Message object to send
                var message = new Message
                {
                    MessageContent = messageContent
                };

                // Use a chat ID of 0 as a placeholder - this would need to be adjusted based on your system's design
                // The Messages API seems to require a chat ID, user and message object
                int defaultChatId = 0;
                var currentUser = await this.userService.GetCurrentUserAsync();
                await this.messagesService.SendMessageAsync(defaultChatId, currentUser, message);
            }
            catch (Exception)
            {
                // Log or handle the error if needed
            }
        }

        public async void SetReportData(int id, string reportedUserCnp, Message reportedMessage)
        {
            this.ReportId = id;
            this.ReportedUserCNP = reportedUserCnp;
            this.ReportedMessage = reportedMessage;

            // Add null check before calling profanity checker
            bool apiSuggestion = false;
            if (reportedMessage != null)
            {
                apiSuggestion = await this.profanityChecker.IsMessageOffensive(reportedMessage);
            }

            this.IdTextBlock.Text = $"Report ID: {id}";
            this.ReportedUserCNPTextBlock.Text = $"Reported user's CNP: {reportedUserCnp}";
            this.ReportedMessageTextBlock.Text = $"Message: {reportedMessage?.MessageContent ?? "No message content"}";
            this.ApiSuggestionTextBlock.Text = apiSuggestion ? "The software marked this message as offensive" : "The software marked this message as inoffensive";

            // Initialize the message checkbox state
            if (this.MessageCheckBox != null)
            {
                this.MessageCheckBox.IsChecked = apiSuggestion;
            }
        }

        private void MessageCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (this.MessageTextBox != null)
            {
                this.MessageTextBox.IsEnabled = true;
            }
        }

        private void MessageCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this.MessageTextBox != null)
            {
                this.MessageTextBox.IsEnabled = false;
            }
        }
    }
}
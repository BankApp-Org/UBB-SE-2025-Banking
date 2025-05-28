// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace BankAppDesktop.Views.Components
{
    using Common.Models.Social;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TipHistoryComponent : Page
    {
        public TipHistoryComponent()
        {
            this.InitializeComponent();
        }

        public Message Tip
        {
            get => (Message)GetValue(TipProperty);
            set
            {
                SetValue(TipProperty, value);
                this.MessageId = value.Id.ToString();
                this.MessageType = $"Type: {value.Type}";
                this.MessageText = value.MessageContent; // Update UI when Tip changes
            }
        }

        public static readonly DependencyProperty TipProperty =
            DependencyProperty.Register(nameof(Message), typeof(Message), typeof(TipHistoryComponent), new PropertyMetadata(null, OnTipChanged));

        private string _messageText = null!;

        public string MessageText
        {
            get => _messageText;
            set
            {
                _messageText = value;
                MessageContentTextBlock.Text = value; // Update UI when MessageText changes
            }
        }

        private string _messageType = null!;

        public string MessageType
        {
            get => _messageType;
            set
            {
                _messageType = value;
                MessageTypeTextBlock.Text = value; // Update UI when MessageType changes
            }
        }

        private string _messageId = null!;

        public string MessageId
        {
            get => _messageId;
            set
            {
                _messageId = value;
                MessageIdTextBlock.Text = value; // Update UI when MessageId changes
            }
        }

        private static void OnTipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TipHistoryComponent component && e.NewValue is Message newTip)
            {
                component.MessageId = newTip.Id.ToString();
                component.MessageType = $"Type: {newTip.Type}";
                component.MessageText = newTip.MessageContent; // Update UI when Tip changes
            }
        }
    }
}

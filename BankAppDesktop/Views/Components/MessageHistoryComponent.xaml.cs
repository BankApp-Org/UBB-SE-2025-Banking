namespace BankAppDesktop.Views.Components
{
    using Common.Models.Social;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    public sealed partial class MessageHistoryComponent : Page
    {
        public MessageHistoryComponent()
        {
            this.InitializeComponent();
            // this.DataContextChanged += (s, e) => Bindings.Update(); // Ensure bindings update when DataContext changes
        }

        public Message Message
        {
            get => (Message)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(Message), typeof(MessageHistoryComponent), new PropertyMetadata(null, OnMessageChanged));

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MessageHistoryComponent component && e.NewValue is Message newMessage)
            {
                component.MessageTypeTextBlock.Text = $"Type: {newMessage.Type}";
                component.MessageTextBlock.Text = newMessage.MessageContent; // Update UI when Message changes
            }
        }
    }
}

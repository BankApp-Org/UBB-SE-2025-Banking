// <copyright file="MessageTemplateSelector.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BankAppDesktop.Views.Components
{
    using Common.Models.Social;
    using Common.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    public partial class MessageTemplateSelector : DataTemplateSelector
    {
        private readonly IAuthenticationService authenticationService;
        public DataTemplate TextMessageTemplateLeft { get; set; }

        public DataTemplate TextMessageTemplateRight { get; set; }

        public DataTemplate ImageMessageTemplateLeft { get; set; }

        public DataTemplate ImageMessageTemplateRight { get; set; }
        public DataTemplate TransferMessageTemplateLeft { get; set; }

        public DataTemplate TransferMessageTemplateRight { get; set; }

        public DataTemplate RequestMessageTemplateLeft { get; set; }

        public DataTemplate RequestMessageTemplateRight { get; set; }

        public DataTemplate BillSplitMessageTemplateLeft { get; set; }

        public DataTemplate BillSplitMessageTemplateRight { get; set; }

        public MessageTemplateSelector(IAuthenticationService authenticationService)
        {
            this.authenticationService = authenticationService;
            // Initialize templates
            TextMessageTemplateLeft = new DataTemplate();
            TextMessageTemplateRight = new DataTemplate();
            ImageMessageTemplateLeft = new DataTemplate();
            ImageMessageTemplateRight = new DataTemplate();
            TransferMessageTemplateLeft = new DataTemplate();
            TransferMessageTemplateRight = new DataTemplate();
            RequestMessageTemplateLeft = new DataTemplate();
            RequestMessageTemplateRight = new DataTemplate();
            BillSplitMessageTemplateLeft = new DataTemplate();
            BillSplitMessageTemplateRight = new DataTemplate();
        }

        public MessageTemplateSelector()
        {
            authenticationService = App.Host.Services.GetRequiredService<IAuthenticationService>();
            TextMessageTemplateLeft = null!;
            TextMessageTemplateRight = null!;
            ImageMessageTemplateLeft = null!;
            ImageMessageTemplateRight = null!;
            TransferMessageTemplateLeft = null!;
            TransferMessageTemplateRight = null!;
            RequestMessageTemplateLeft = null!;
            RequestMessageTemplateRight = null!;
            BillSplitMessageTemplateLeft = null!;
            BillSplitMessageTemplateRight = null!;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is Message message)
            {
                switch (message)
                {
                    case TextMessage _:
                        return message.Sender.CNP == authenticationService.GetUserCNP() ? TextMessageTemplateRight : TextMessageTemplateLeft;

                    case ImageMessage _:
                        return message.Sender.CNP == authenticationService.GetUserCNP() ? ImageMessageTemplateRight : ImageMessageTemplateLeft;
                    case TransferMessage _:
                        return message.Sender.CNP == authenticationService.GetUserCNP() ? TransferMessageTemplateRight : TransferMessageTemplateLeft;

                    case RequestMessage _:
                        return message.Sender.CNP == authenticationService.GetUserCNP() ? RequestMessageTemplateRight : RequestMessageTemplateLeft;

                    case BillSplitMessage _:
                        return message.Sender.CNP == authenticationService.GetUserCNP() ? BillSplitMessageTemplateRight : BillSplitMessageTemplateLeft;
                }
            }

            return TextMessageTemplateLeft;
        }
    }
}
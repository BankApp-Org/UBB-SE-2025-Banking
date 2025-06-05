using System;
using Common.Models.Social;
using Common.Services.Social;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using System.Windows.Input;
using BankAppDesktop.Commands;
using Common.Models;

namespace BankAppDesktop.ViewModels
{
    public class MessageTemplatesViewModel : ViewModelBase
    {
        private readonly IMessageService _messageService;
        private ObservableCollection<Message> _messages;
        private int _currentChatId;

        public MessageTemplatesViewModel(IMessageService messageService)
        {
            _messageService = messageService;
            _messages = new ObservableCollection<Message>();

            // Initialize commands
            DeleteMessageCommand = new RelayCommandGeneric<Message>(ExecuteDeleteMessage);
            ReportMessageCommand = new RelayCommandGeneric<Message>(ExecuteReportMessage);
            AcceptRequestCommand = new RelayCommandGeneric<Message>(ExecuteAcceptRequest);
        }

        public ObservableCollection<Message> Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                OnPropertyChanged();
            }
        }

        public int CurrentChatId
        {
            get => _currentChatId;
            set
            {
                _currentChatId = value;
                OnPropertyChanged();
                LoadMessages();
            }
        }

        public ICommand DeleteMessageCommand { get; }
        public ICommand ReportMessageCommand { get; }
        public ICommand AcceptRequestCommand { get; }

        private async void LoadMessages()
        {
            if (_currentChatId <= 0) return;

            try
            {
                var messages = await _messageService.GetMessagesAsync(_currentChatId, 1, 20);
                Messages.Clear();
                foreach (var message in messages)
                {
                    Messages.Add(message);
                }
            }
            catch (Exception ex)
            {
                // Handle error (you might want to show a message to the user)
                System.Diagnostics.Debug.WriteLine($"Error loading messages: {ex.Message}");
            }
        }

        private async void ExecuteDeleteMessage(Message message)
        {
            if (message == null) return;

            try
            {
                await _messageService.DeleteMessageAsync(_currentChatId, message.Id, message.Sender);
                Messages.Remove(message);
            }
            catch (Exception ex)
            {
                // Handle error
                System.Diagnostics.Debug.WriteLine($"Error deleting message: {ex.Message}");
            }
        }

        private async void ExecuteReportMessage(Message message)
        {
            if (message == null) return;

            try
            {
                await _messageService.ReportMessage(_currentChatId, message.Id, message.Sender, ReportReason.Other);
                // You might want to show a success message to the user
            }
            catch (Exception ex)
            {
                // Handle error
                System.Diagnostics.Debug.WriteLine($"Error reporting message: {ex.Message}");
            }
        }

        private async void ExecuteAcceptRequest(Message message)
        {
            if (message == null || message.Type != MessageType.Request) return;

            try
            {
                // Implement request acceptance logic here
                // This might involve calling a different service method
                // For now, we'll just remove the message
                Messages.Remove(message);
            }
            catch (Exception ex)
            {
                // Handle error
                System.Diagnostics.Debug.WriteLine($"Error accepting request: {ex.Message}");
            }
        }
    }
}
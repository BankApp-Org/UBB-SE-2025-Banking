using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace BankAppDesktop.Views.Pages
{
    using BankAppDesktop.ViewModels;
    using Common.Services;
    using Common.Services.Social;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    public sealed partial class LeaveChatPage : Page
    {
        private IChatService chatService;
        private IUserService userService;
        private Frame rightFrame;
        private LeaveChatViewModel leaveChatViewModel;
        private Page lastPage;
        // waiting for chatlistviewmodel to be done
        private ChatListViewModel chatMessagesViewModel;

        public LeaveChatPage(int chatID, ChatListViewModel chVm, Page chatM, Frame right, IChatService chat, IUserService user)
        {
            this.InitializeComponent();
            this.chatMessagesViewModel = chVm;
            this.lastPage = chatM;
            this.rightFrame = right;
            this.userService = user;
            this.chatService = chat;

            this.leaveChatViewModel = new LeaveChatViewModel(this.userService, this.chatService, this.chatMessagesViewModel, chatID);
            this.DataContext = this.leaveChatViewModel;
        }

        public void LeaveChat_Click(object sender, RoutedEventArgs e)
        {
            this.rightFrame.Content = null;
        }

        public void CancelLeaving_Click(object sender, RoutedEventArgs e)
        {
            this.rightFrame.Content = this.lastPage;
        }
    }
}

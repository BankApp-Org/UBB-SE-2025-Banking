using BankAppDesktop.ViewModels;
using Common.Services;
using Common.Services.Social;
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
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FriendsListPage : Page
    {
        private FriendsListViewModel friendsListViewModel;
        private IChatService chatService;
        private IUserService userService;
        private IMessageService messageService;
        private Frame rightFrame;
        private Page addFriendsPage;

        public FriendsListPage(IChatService chatService, IUserService userService, IMessageService messageService, Frame rightFrame)
        {
            this.InitializeComponent();
            this.chatService = chatService;
            this.userService = userService;
            this.messageService = messageService;
            this.rightFrame = rightFrame;
            this.friendsListViewModel = new FriendsListViewModel(chatService, userService, messageService);

            this.DataContext = this.friendsListViewModel;
        }

        private void AddFriend_Click(object sender, RoutedEventArgs e)
        {
            this.rightFrame.Content = new AddFriendsPage(this.friendsListViewModel, this.userService);
        }
    }
}

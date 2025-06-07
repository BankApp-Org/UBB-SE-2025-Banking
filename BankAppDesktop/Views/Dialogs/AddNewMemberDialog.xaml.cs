using BankAppDesktop.ViewModels;
using Common.Services;
using Common.Services.Social;
using Microsoft.UI.Xaml.Controls;

namespace BankAppDesktop.Views.Dialogs
{
    /// <summary>
    /// Dialog for adding new members to a chat.
    /// </summary>
    public sealed partial class AddNewMemberDialog : ContentDialog
    {
        private AddNewMemberViewModel addNewMemberViewModel;

        public AddNewMemberDialog(ChatMessagesViewModel chatMessagesViewModel, int chatID, IChatService chatService, IUserService userService)
        {
            this.InitializeComponent();
            this.addNewMemberViewModel = new AddNewMemberViewModel(chatMessagesViewModel, null, chatID, chatService, userService);
            this.DataContext = this.addNewMemberViewModel;

            // Set the XamlRoot for the dialog
            if (App.MainAppWindow?.MainAppFrame?.XamlRoot != null)
            {
                this.XamlRoot = App.MainAppWindow.MainAppFrame.XamlRoot;
            }
        }

        /// <summary>
        /// Gets the view model for this dialog.
        /// </summary>
        public AddNewMemberViewModel ViewModel => this.addNewMemberViewModel;
    }
}

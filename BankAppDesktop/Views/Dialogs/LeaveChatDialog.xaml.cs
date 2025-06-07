using Microsoft.UI.Xaml.Controls;
using BankAppDesktop.ViewModels;

namespace BankAppDesktop.Views.Dialogs
{
    public sealed partial class LeaveChatDialog : ContentDialog
    {
        public LeaveChatViewModel ViewModel { get; }

        public LeaveChatDialog(LeaveChatViewModel viewModel)
        {
            this.InitializeComponent();
            ViewModel = viewModel;
            DataContext = ViewModel;
        }
    }
}

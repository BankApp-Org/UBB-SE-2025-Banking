using BankAppDesktop.ViewModels;
using Microsoft.UI;
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
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace BankAppDesktop.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BankTransactionsPage : Page
    {
        private readonly BankTransactionsViewModel viewModel;
        public BankTransactionsPage(string iban)
        {
            this.InitializeComponent();
            viewModel = new BankTransactionsViewModel(iban);
            MainGrid.DataContext = viewModel;
            viewModel.CloseAction = CloseWindow;
        }

        private void CloseWindow()
        {
            App.MainAppWindow.MainAppFrame.Content = new MainPage();
        }
    }
}

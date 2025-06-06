using BankAppDesktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
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
    public sealed partial class CurrencyExchangeTable : Page
    {
        private readonly CurrencyExchangeViewModel viewModel;
        private string Iban { get; set; }
        public CurrencyExchangeTable(string iban)
        {
            this.InitializeComponent();
            this.viewModel = App.Services.GetRequiredService<CurrencyExchangeViewModel>();
            MainGrid.DataContext = viewModel;
            Iban = iban;
            viewModel.CloseAction = CloseWindow;
        }

        private void CloseWindow()
        {
            App.MainAppWindow.MainAppFrame.Content = new BankTransactionsPage(Iban);
        }
    }
}

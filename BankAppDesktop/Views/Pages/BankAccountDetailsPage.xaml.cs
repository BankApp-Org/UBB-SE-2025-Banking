using BankAppDesktop.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace BankAppDesktop.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BankAccountDetailsPage : Page
    {
        private BankAccountDetailsViewModel? viewModel;
        private string selectedIban = string.Empty;
        public BankAccountDetailsPage(string iban)
        {
            try
            {
                this.InitializeComponent();

                this.viewModel = App.Services.GetRequiredService<BankAccountDetailsViewModel>();
                viewModel.SetIban(iban);
                selectedIban = iban;
                MainGrid.DataContext = viewModel;
                this.Loaded += Page_Loaded;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing BankAccountUpdateView: {ex.Message}");
                // ShowErrorDialog("Initialization Error", $"Failed to initialize the bank account update view: {ex.Message}");
            }

            viewModel.OnClose = () => App.MainAppWindow.MainAppFrame.Content = new MainPage();
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // It's good practice to unsubscribe from the event so it doesn't fire again
            this.Loaded -= Page_Loaded;

            // 5. THIS IS THE CALL: Tell the ViewModel to start loading the data.
            await viewModel.LoadAccountDetailsAsync();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.MainAppWindow.MainAppFrame.Content = new MainPage();
        }
    }
}

using BankAppDesktop.ViewModels;
using Common.Services.Bank;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace BankAppDesktop.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BankTransactionsHistoryPage : Page
    {
        public BankTransactionsHistoryViewModel TransactionsViewModel;
        public ObservableCollection<string> CurrentList;
        private IBankTransactionService transactionHistoryService;

        public BankTransactionsHistoryPage(string iban)
        {
            this.TransactionsViewModel = App.Services.GetRequiredService<BankTransactionsHistoryViewModel>();
            this.TransactionsViewModel.CurrentIban = iban;
            this.transactionHistoryService = App.Services.GetRequiredService<IBankTransactionService>();
            this.InitializeComponent();
            InitializeDataAsync();
        }

        private async void InitializeDataAsync()
        {
            CurrentList = await TransactionsViewModel.RetrieveForMenu();
            TransactionList.ItemsSource = CurrentList;
        }

        private async void ExportToCSV_Click(object sender, RoutedEventArgs e)
        {
            await TransactionsViewModel.CreateCSVAsync();
            ContentDialog dialog = new ContentDialog
            {
                Title = "Export Complete",
                Content = "Transactions exported to CSV.",
                CloseButtonText = "Ok",
                XamlRoot = this.Content.XamlRoot
            };
            _ = dialog.ShowAsync();
        }
        private async void TransactionTypeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string typedText = (sender as TextBox).Text;
            if (!string.IsNullOrEmpty(typedText))
            {
                CurrentList = await TransactionsViewModel.FilterByTypeForMenu(typedText);
                TransactionList.ItemsSource = CurrentList;
            }
            else
            {
                CurrentList = await TransactionsViewModel.RetrieveForMenu();
                TransactionList.ItemsSource = CurrentList;
            }
        }

        private async void TransactionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            return;
            // if (TransactionList.SelectedItem != null)
            // {
            //    string selectedTransactionForMenu = TransactionList.SelectedItem as string;
            //    // Retrieve the detailed information of the selected transaction
            //    var selectedTransaction = await TransactionsViewModel.GetTransactionByMenuString(selectedTransactionForMenu);
            //    string detailedTransaction = selectedTransaction.TostringDetailed();
            //    TransactionDetailsView transactionDetailsWindow = new TransactionDetailsView(detailedTransaction, selectedTransaction, transactionHistoryService);
            //    transactionDetailsWindow.Activate();
            // }
        }

        private async void ViewGraphics_Click(object sender, RoutedEventArgs e)
        {
            var transactionTypeCounts = await TransactionsViewModel.GetTransactionTypeCounts();

            BankTransactionsChartPage bankTransactionsChartPage = new BankTransactionsChartPage(transactionTypeCounts);
            App.MainAppWindow.MainAppFrame.Content = bankTransactionsChartPage;
        }
    }
}

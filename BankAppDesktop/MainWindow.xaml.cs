namespace BankAppDesktop
{
    using BankAppDesktop.Pages;
    using BankAppDesktop.Views;
    using BankAppDesktop.Views.Pages;
    using Common.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using System;
    using System.ComponentModel;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IAuthenticationService authenticationService;

        public Frame MainAppFrame => this.MainFrame;

        private bool _isUnauthenticated;
        public bool IsUnauthenticated
        {
            get => _isUnauthenticated;
            set
            {
                if (_isUnauthenticated != value)
                {
                    _isUnauthenticated = value;
                    OnPropertyChanged(nameof(IsUnauthenticated));
                }
            }
        }

        private bool _isAuthenticated;
        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set
            {
                if (_isAuthenticated != value)
                {
                    _isAuthenticated = value;
                    OnPropertyChanged(nameof(IsAuthenticated));
                }
            }
        }

        private bool _isAdmin;
        public bool IsAdmin
        {
            get => _isAdmin;
            set
            {
                if (_isAdmin != value)
                {
                    _isAdmin = value;
                    OnPropertyChanged(nameof(IsAdmin));
                }
            }
        }

        private string _userName = "Guest";
        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow(IServiceProvider serviceProvider)
        {
            this.InitializeComponent();

            // Set DataContext on the Window's Content (root XAML element), which is a FrameworkElement.
            if (this.Content is FrameworkElement rootElement)
            {
                rootElement.DataContext = this;
            }

            this.serviceProvider = serviceProvider;

            this.MainFrame.Content = this.serviceProvider.GetRequiredService<MainPage>();
            this.authenticationService = this.serviceProvider.GetRequiredService<IAuthenticationService>();
            this.authenticationService.UserLoggedIn += this.AuthenticationService_UserLoggedIn;
            this.authenticationService.UserLoggedOut += this.AuthenticationService_UserLoggedOut;
            UpdateLoginRelatedButtonVisibility();
        }

        private void UpdateLoginRelatedButtonVisibility()
        {
            var isLoggedIn = this.authenticationService.IsUserLoggedIn();
            this.IsUnauthenticated = !isLoggedIn;
            this.IsAuthenticated = isLoggedIn;
            this.IsAdmin = this.authenticationService.IsUserAdmin();

            // Update the username
            if (isLoggedIn)
            {
                var userSession = this.authenticationService.GetCurrentUserSession();
                this.UserName = userSession?.UserName ?? "User";
            }
            else
            {
                this.UserName = "Guest";
            }

            OnPropertyChanged(nameof(IsUnauthenticated));
            OnPropertyChanged(nameof(IsAuthenticated));
            OnPropertyChanged(nameof(IsAdmin));
            OnPropertyChanged(nameof(UserName));
        }

        private void AuthenticationService_UserLoggedIn(object? sender, UserLoggedInEventArgs e)
        {
            UpdateLoginRelatedButtonVisibility();
            this.MainFrame.Content = this.serviceProvider.GetRequiredService<HomePage>();
        }

        private void AuthenticationService_UserLoggedOut(object? sender, UserLoggedOutEventArgs e)
        {
            UpdateLoginRelatedButtonVisibility();
            this.MainFrame.Content = this.serviceProvider.GetRequiredService<LoginPage>();
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                // Check if the selected item is a dropdown parent (has MenuItems collection)
                if (args.SelectedItemContainer is NavigationViewItem navItem && navItem.MenuItems.Count > 0)
                {
                    // This is a parent item with a dropdown - do nothing and let it just expand/collapse
                    return;
                }

                // Check if the Tag property exists
                if (args.SelectedItemContainer.Tag == null)
                {
                    // No Tag property, so don't try to navigate
                    return;
                }

                string invokedItemTag = args.SelectedItemContainer.Tag.ToString() ?? throw new InvalidOperationException("Tag cannot be null");
                NavigateToPage(invokedItemTag);
            }
        }

        private void UserMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.Tag is string tag)
            {
                NavigateToPage(tag);
            }
        }

        private void NavigateToPage(string pageTag)
        {
            this.MainFrame.Content = pageTag switch
            {
                "ChatReports" => this.serviceProvider.GetRequiredService<ChatReportPage>(),
                "LoanRequest" => this.serviceProvider.GetRequiredService<LoanRequestPage>(),
                "Loans" => this.serviceProvider.GetRequiredService<LoansPage>(),
                "UsersList" => this.serviceProvider.GetRequiredService<UsersPage>(),
                "BillSplitReports" => this.serviceProvider.GetRequiredService<BillSplitReportPage>(),
                "Investments" => this.serviceProvider.GetRequiredService<InvestmentsPage>(),
                "AlertsPage" => this.serviceProvider.GetRequiredService<AlertsPage>(),
                "AnalysisPage" => this.serviceProvider.GetRequiredService<AnalysisPage>(),
                "TipsPage" => this.serviceProvider.GetRequiredService<TipsPage>(),
                "HomePage" => this.serviceProvider.GetRequiredService<HomePage>(),
                "NewsListPage" => this.serviceProvider.GetRequiredService<NewsListPage>(),
                "CreateStockPage" => this.serviceProvider.GetRequiredService<CreateStockPage>(),
                "TransactionLogPage" => this.serviceProvider.GetRequiredService<TransactionLogPage>(),
                "ProfilePage" => this.serviceProvider.GetRequiredService<ProfilePage>(),
                "LoginPage" => this.serviceProvider.GetRequiredService<LoginPage>(),
                "GemStorePage" => this.serviceProvider.GetRequiredService<GemStorePage>(),
                "BankAccountCreateView" => this.serviceProvider.GetRequiredService<BankAccountCreateView>(),
                "NotificationsPage" => this.serviceProvider.GetRequiredService<NotificationsPage>(),
                "BankTransactionsHistoryPage" => this.serviceProvider.GetRequiredService<BankTransactionsHistoryPage>(),
                "MainPage" => this.serviceProvider.GetRequiredService<MainPage>(),
                _ => throw new InvalidOperationException($"Unknown navigation item: {pageTag}")
            };
        }
    }
}
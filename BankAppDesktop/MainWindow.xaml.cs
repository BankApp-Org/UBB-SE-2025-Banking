namespace BankAppDesktop
{
    using BankAppDesktop.Pages;
    using BankAppDesktop.Views.Pages;
    using Common.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using System;
    using System.ComponentModel;
    using BankAppDesktop.Views;

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

            this.MainFrame.Content = this.serviceProvider.GetRequiredService<HomePage>();
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
            OnPropertyChanged(nameof(IsUnauthenticated));
            OnPropertyChanged(nameof(IsAuthenticated));
            OnPropertyChanged(nameof(IsAdmin));
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
                string invokedItemTag = args.SelectedItemContainer.Tag.ToString() ?? throw new InvalidOperationException("Tag cannot be null");
                this.MainFrame.Content = invokedItemTag switch
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
                    _ => throw new InvalidOperationException($"Unknown navigation item: {invokedItemTag}")
                };
            }
        }
    }
}
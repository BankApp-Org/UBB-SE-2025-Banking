namespace BankAppDesktop
{
    using Common.Services;
    using Common.Services.Impl;
    using Common.Services.Proxy;
    using Microsoft.AspNetCore.Http.Json;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using Microsoft.UI.Xaml;
    using BankAppDesktop.Pages;
    using BankAppDesktop.Services;
    using BankAppDesktop.ViewModels;
    using BankAppDesktop.Views;
    using BankAppDesktop.Views.Components;
    using BankAppDesktop.Views.Controls;
    using BankAppDesktop.Views.Pages;
    using System;
    using System.Net.Http;
    using Common.Services.Bank;
    using Common.Services.Social;
    using Common.Services.Stock;

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static MainWindow? MainAppWindow { get; private set; } = null!;

        public static IHost Host { get; private set; } = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            // Note: We've replaced DatabaseHelper.InitializeDatabase() with EF Core migrations
            this.InitializeComponent();
            ConfigureHost();

            // explanation before the OnUnhandledException method
            // this.UnhandledException += this.OnUnhandledException;
        }

        private static void ConfigureHost()
        {
            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Build configuration
                    var configBuilder = new ConfigurationBuilder()
                        .AddUserSecrets<App>()
                        .AddEnvironmentVariables();

                    var config = configBuilder.Build();
                    string apiBaseUrl = App.Configuration.GetValue<string>("ApiBase")
                        ?? throw new InvalidOperationException("API base URL is not configured");

                    services.AddHttpClient("ApiClient", client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    });

                    services.ConfigureHttpJsonOptions(options =>
                    {
                        options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                        options.SerializerOptions.WriteIndented = true;
                        options.SerializerOptions.PropertyNameCaseInsensitive = true;
                    });

                    services.AddSingleton<IConfiguration>(config);

                    // Register the authentication service first
                    services.AddSingleton<IAuthenticationService>(sp =>
                        new AuthenticationService(
                            sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient"),
                            sp.GetRequiredService<IConfiguration>(),
                            sp.GetRequiredService<IOptions<JsonOptions>>()));

                    // Register the AuthenticationDelegatingHandler to automatically handle JWT tokens
                    services.AddTransient<AuthenticationDelegatingHandler>();

                    // Other Services
                    services.AddScoped<ITransactionLogService, TransactionLogProxyService>();
                    services.AddScoped<ITransactionService, TransactionProxyService>();
                    services.AddScoped<IChatReportService, ChatReportProxyService>();
                    services.AddScoped<ICreditHistoryService, HistoryProxyService>();
                    services.AddScoped<IBillSplitReportService, BillSplitReportProxyService>();
                    services.AddScoped<ILoanService, LoanProxyService>();
                    services.AddScoped<ILoanRequestService, LoanRequestProxyService>();
                    services.AddScoped<IStockPageService, StockPageProxyService>();
                    services.AddScoped<IInvestmentsService, InvestmentsProxyService>();
                    services.AddScoped<IUserService, UserProxyService>();
                    services.AddScoped<IActivityService, ActivityProxyService>();
                    services.AddScoped<IStoreService, StoreProxyService>();
                    services.AddScoped<INewsService, NewsProxyService>();
                    services.AddScoped<IStockService, StockProxyService>();
                    services.AddScoped<IAlertService, AlertProxyService>();
                    services.AddScoped<IMessagesService, MessagesProxyService>();
                    services.AddScoped<ITipsService, TipsProxyService>();
                    services.AddScoped<IProfanityChecker, ProfanityChecker>();

                    // Configure HttpClients with the AuthenticationDelegatingHandler
                    services.AddHttpClient<IStockService, StockProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<IInvestmentsService, InvestmentsProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<IUserService, UserProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<IActivityService, ActivityProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<IStoreService, StoreProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<INewsService, NewsProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<IStockPageService, StockPageProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<IAlertService, AlertProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<ILoanService, LoanProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<ILoanRequestService, LoanRequestProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<IBillSplitReportService, BillSplitReportProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<IMessagesService, MessagesProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<ICreditHistoryService, HistoryProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<ITipsService, TipsProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<IChatReportService, ChatReportProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<ITransactionLogService, TransactionLogProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    services.AddHttpClient<ITransactionService, TransactionProxyService>(client =>
                    {
                        client.BaseAddress = new Uri(apiBaseUrl);
                    }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                    // Don't add the handler to the authentication service's HttpClient
                    // to avoid circular dependencies
                    services.AddSingleton<MainWindow>();

                    // UI Components
                    services.AddTransient<BillSplitReportComponent>();
                    services.AddTransient<BillSplitReportPage>();
                    services.AddTransient<ChatReportComponent>();
                    services.AddTransient<LoanRequestComponent>();
                    services.AddTransient<LoanComponent>();
                    services.AddTransient<CreateLoanDialog>();
                    services.AddTransient<BillSplitReportComponent>();
                    services.AddTransient<LoanRequestPage>();
                    services.AddTransient<UserInfoComponent>();
                    services.AddTransient<UserProfileComponent>();
                    services.AddTransient<LoginComponent>();
                    services.AddTransient<CreateLoanDialog>();
                    services.AddTransient<AdminNewsControl>();

                    // ViewModels
                    services.AddTransient<BillSplitReportViewModel>();
                    services.AddTransient<BillSplitReportViewModel>();
                    services.AddTransient<StoreViewModel>();
                    services.AddTransient<ProfilePageViewModel>();
                    services.AddTransient<InvestmentsViewModel>();
                    services.AddTransient<HomepageViewModel>();
                    services.AddTransient<CreateStockViewModel>();
                    services.AddTransient<CreateProfilePageViewModel>();
                    services.AddTransient<TipHistoryViewModel>();
                    services.AddTransient<ActivityViewModel>();
                    services.AddTransient<NewsDetailViewModel>();
                    services.AddTransient<NewsListViewModel>();
                    services.AddTransient<NewsListPage>();
                    services.AddTransient<ArticleCreationViewModel>();
                    services.AddTransient<AdminNewsViewModel>();
                    services.AddTransient<StockPageViewModel>();
                    services.AddTransient<TransactionLogViewModel>();
                    services.AddTransient<UserProfileComponentViewModel>();
                    services.AddTransient<AlertViewModel>();
                    services.AddTransient<UpdateProfilePageViewModel>();
                    services.AddTransient<AuthenticationViewModel>();
                    services.AddTransient<LoansViewModel>();
                    services.AddTransient<CreateLoanDialogViewModel>();
                    services.AddTransient<LoanRequestViewModel>();

                    // Pages
                    services.AddTransient<LoansPage>();
                    services.AddTransient<UsersPage>();
                    services.AddTransient<AlertsPage>();
                    services.AddTransient<AnalysisPage>();
                    services.AddTransient<TipsPage>();
                    services.AddTransient<CreateStockPage>();
                    services.AddTransient<TransactionLogPage>();
                    services.AddTransient<ProfilePage>();
                    services.AddTransient<GemStorePage>();
                    services.AddTransient<CreateProfilePage>();
                    services.AddTransient<HomePage>();
                    services.AddTransient<InvestmentsPage>();
                    services.AddTransient<TipHistoryWindow>();
                    services.AddTransient<NewsArticlePage>();
                    services.AddTransient<LoansViewModel>();
                    services.AddTransient<ChatReportPage>();
                    services.AddTransient<ArticleCreationPage>();
                    services.AddTransient<StockPage>();
                    services.AddTransient<UpdateProfilePage>();
                    services.AddTransient<LoginPage>();

                    // FIXME: remove \/\/\/\/
                    services.AddTransient<Func<LoanRequestComponent>>(sp => () => sp.GetRequiredService<LoanRequestComponent>());
                    services.AddTransient<Func<ChatReportComponent>>(sp => () => sp.GetRequiredService<ChatReportComponent>());
                    services.AddTransient<Func<UserInfoComponent>>(sp => () => sp.GetRequiredService<UserInfoComponent>());
                }).Build();
        }

        /// <summary>
        /// Gets or sets the current window of the application.
        /// </summary>
        public static Window CurrentWindow { get; set; } = null!;

        /// <summary>
        /// Gets Configuration object for the application.
        /// </summary>
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="launchActivatedEventArgs">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs launchActivatedEventArgs)
        {
            MainAppWindow = Host.Services.GetRequiredService<MainWindow>();
            MainAppWindow.Activate();
        }
    }
}
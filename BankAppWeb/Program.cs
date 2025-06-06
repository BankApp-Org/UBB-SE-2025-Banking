using BankAppWeb.Services;
using Common.Services;
using Common.Services.Bank;
using Common.Services.Impl;
using Common.Services.Proxy;
using Common.Services.Social;
using Common.Services.Trading;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
// Add for logging

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Configure Razor Pages with routing options
builder.Services.AddRazorPages(options =>
{
    // If needed, configure Razor Pages options here
});

// Configure authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"];
    var jwtIssuer = builder.Configuration["Jwt:Issuer"];
    var jwtAudience = builder.Configuration["Jwt:Audience"];

    // Log the loaded JWT configuration values for verification
    var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConsole());
    var startupLogger = loggerFactory.CreateLogger<Program>();

    if (string.IsNullOrEmpty(jwtKey))
    {
        startupLogger.LogError("JWT Key (Jwt:Key) is not configured in appsettings.json or is empty.");
    }
    if (string.IsNullOrEmpty(jwtIssuer))
    {
        startupLogger.LogWarning("JWT Issuer (Jwt:Issuer) is not configured in appsettings.json or is empty. Depending on validation settings, this might be an issue.");
    }
    if (string.IsNullOrEmpty(jwtAudience))
    {
        startupLogger.LogWarning("JWT Audience (Jwt:Audience) is not configured in appsettings.json or is empty. Depending on validation settings, this might be an issue.");
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = !string.IsNullOrEmpty(jwtIssuer),
        ValidateAudience = !string.IsNullOrEmpty(jwtAudience),
        ValidateLifetime = true,
        ValidateIssuerSigningKey = !string.IsNullOrEmpty(jwtKey),
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = !string.IsNullOrEmpty(jwtKey)
            ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            : null,
        ClockSkew = TimeSpan.FromMinutes(1) // Allow a small clock skew, default is 5 minutes. Adjust if necessary.
    };

    // Configure JWT Bearer to check for token in cookies
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
            if (context.Request.Cookies.TryGetValue("jwt_token", out var tokenFromCookie))
            {
                logger.LogInformation("JWT token found in cookie 'jwt_token'. Token: {Token}", tokenFromCookie);
                context.Token = tokenFromCookie;
            }
            else
            {
                logger.LogWarning("JWT token not found in cookie 'jwt_token'.");
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
            logger.LogError(context.Exception, "JWT Authentication FAILED. Exception: {ExceptionMessage}. Token (if present): {Token}", context.Exception.Message, context.Request.Cookies["jwt_token"]);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
            logger.LogInformation("JWT token successfully VALIDATED. User: {User}, Claims: {Claims}", context.Principal?.Identity?.Name, context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}"));
            return Task.CompletedTask;
        }
    };
});

// Register HttpContextAccessor for session access - still needed for WebAuthenticationService to access User
builder.Services.AddHttpContextAccessor();

// Configure HttpClient for BankApi
string apiBaseUrl = builder.Configuration["ApiBase"]
    ?? throw new InvalidOperationException("API base URL is not configured in appsettings.json");

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddHttpClient("BankApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// Register services
builder.Services.AddScoped<IAuthenticationService, WebAuthenticationService>();
builder.Services.AddScoped<IUserService, UserProxyService>();
builder.Services.AddScoped<ILoanService, LoanProxyService>();
builder.Services.AddScoped<ILoanRequestService, LoanRequestProxyService>();
builder.Services.AddScoped<IStockService, StockProxyService>();
builder.Services.AddScoped<ITransactionService, StockTransactionProxyService>();
builder.Services.AddScoped<ITransactionLogService, StockTransactionLogProxyService>();
builder.Services.AddScoped<IChatReportService, ChatReportProxyService>();
builder.Services.AddScoped<IProfanityChecker, ProfanityChecker>();
builder.Services.AddScoped<IMessageService, MessagesProxyService>();
builder.Services.AddScoped<IActivityService, ActivityProxyService>();
builder.Services.AddScoped<ICreditHistoryService, HistoryProxyService>();
builder.Services.AddScoped<IAlertService, AlertProxyService>();
builder.Services.AddScoped<IStockPageService, StockPageProxyService>();
builder.Services.AddScoped<IStoreService, StoreProxyService>();
builder.Services.AddScoped<INewsService, NewsProxyService>();
builder.Services.AddTransient<IChatService, ChatProxyService>();
builder.Services.AddTransient<IMessageService, MessagesProxyService>();
builder.Services.AddTransient<INotificationService, NotificationProxyService>();
builder.Services.AddTransient<IBankAccountService, BankAccountProxyService>();
builder.Services.AddTransient<AuthenticationDelegatingHandler>();
builder.Services.AddTransient<IBankTransactionService, BankTransactionProxyService>();


builder.Services.AddHttpClient<IUserService, UserProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<ILoanService, LoanProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<ILoanRequestService, LoanRequestProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IMessageService, MessagesProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IActivityService, ActivityProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<ICreditHistoryService, HistoryProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IStockService, StockProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<ITransactionService, StockTransactionProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<ITransactionLogService, StockTransactionLogProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IChatReportService, ChatReportProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IMessageService, MessagesProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IAlertService, AlertProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IStockPageService, StockPageProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IStoreService, StoreProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<INewsService, NewsProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IChatService, ChatProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
builder.Services.AddHttpClient<IMessageService, MessagesProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
builder.Services.AddHttpClient<INotificationService, NotificationProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
builder.Services.AddHttpClient<IBankAccountService, BankAccountProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
builder.Services.AddHttpClient<IBankTransactionService, BankTransactionProxyService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
builder.Services.AddScoped<WebAuthenticationService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=MainPage}/{action=Index}");

app.MapRazorPages();

app.Run();
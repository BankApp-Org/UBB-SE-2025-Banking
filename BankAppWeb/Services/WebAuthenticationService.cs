using Common.Models;
using Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace BankAppWeb.Services
{
    public class WebAuthenticationService : IAuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions;
        private UserSession? _currentUserSession;

        public event EventHandler<UserLoggedInEventArgs>? UserLoggedIn;
        public event EventHandler<UserLoggedOutEventArgs>? UserLoggedOut;

        public WebAuthenticationService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IOptions<JsonOptions> jsonOptions)
        {
            _httpClient = httpClientFactory.CreateClient("BankApi")
                ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration
                ?? throw new ArgumentNullException(nameof(configuration));
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _jsonOptions = jsonOptions.Value.JsonSerializerOptions
                ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

            // Try to restore session from HttpContext User (ClaimsPrincipal)
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var userName = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
                var token = GetTokenFromHttpContext();
                if (userId != null && userName != null && token != null)
                {
                    _currentUserSession = new UserSession
                    {
                        UserId = userId,
                        UserName = userName,
                        Roles = roles,
                        Token = token,
                        CNP = GetUserCNP(),
                    };
                }
            }
        }

        public UserSession? GetCurrentUserSession()
        {
            // Re-evaluate from HttpContext User on each call to ensure freshness
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var userName = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
                var token = GetTokenFromHttpContext();

                if (userId != null && userName != null)
                {
                    _currentUserSession = new UserSession
                    {
                        UserId = userId,
                        UserName = userName,
                        Roles = roles,
                        Token = token,
                        CNP = GetUserCNP(),
                    };
                    return _currentUserSession;
                }
            }
            _currentUserSession = null;
            return null;
        }

        public string? GetToken()
        {
            return GetTokenFromHttpContext();
        }

        private string? GetTokenFromHttpContext()
        {
            // Attempt to retrieve the token from the 'jwt_token' cookie
            if (_httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue("jwt_token", out var tokenFromCookie) == true)
            {
                return tokenFromCookie;
            }

            // Fallback to Authorization header if needed, though cookie is primary for web
            var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
            if (authorizationHeader?.StartsWith("Bearer ") == true)
            {
                return authorizationHeader.Substring("Bearer ".Length);
            }
            return null;
        }

        public string GetUserCNP()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                throw new InvalidOperationException("User is not authenticated");
            }

            var cnpClaim = user.Claims.FirstOrDefault(c => c.Type == "CNP");
            return cnpClaim?.Value; //?? throw new InvalidOperationException("CNP claim not found in token");
        }

        public bool IsUserAdmin()
        {
            return _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;
        }

        public bool IsUserLoggedIn()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }

        public async Task<UserSession> LoginAsync(string username, string password)
        {
            var loginRequest = new
            {
                Username = username,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest, _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Login failed: {response.ReasonPhrase}. Details: {errorContent}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
            {
                throw new InvalidOperationException("Invalid token response");
            }

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenResponse.Token);

            var userId = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                ?? throw new InvalidOperationException("User ID not found in token claims");
            var userName = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                ?? throw new InvalidOperationException("User name not found in token claims");
            var roles = token.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            var userCNP = token.Claims.FirstOrDefault(c => c.Type == "CNP")?.Value
                ?? throw new InvalidOperationException("CNP not found in token claims");

            _currentUserSession = new UserSession
            {
                UserId = userId,
                UserName = userName,
                Token = tokenResponse.Token,
                Roles = roles,
                ExpiryTimestamp = token.ValidTo,
                CNP = userCNP
            };

            OnUserLoggedIn(new UserLoggedInEventArgs(userId));

            return _currentUserSession;
        }

        public async Task LogoutAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            _currentUserSession = null;

            if (!string.IsNullOrEmpty(userId))
            {
                OnUserLoggedOut(new UserLoggedOutEventArgs(userId));
            }

            await Task.CompletedTask;
        }

        protected virtual void OnUserLoggedIn(UserLoggedInEventArgs e)
        {
            UserLoggedIn?.Invoke(this, e);
        }

        protected virtual void OnUserLoggedOut(UserLoggedOutEventArgs e)
        {
            UserLoggedOut?.Invoke(this, e);
        }
    }

    internal class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
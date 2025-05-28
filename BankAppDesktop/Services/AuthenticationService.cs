using Common.Models;
using Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace BankAppDesktop.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly TokenStorageService _tokenStorage;
        private readonly JsonSerializerOptions _jsonOptions;
        private UserSession? _currentUserSession;
        public event EventHandler<UserLoggedInEventArgs>? UserLoggedIn;
        public event EventHandler<UserLoggedOutEventArgs>? UserLoggedOut;

        public AuthenticationService(HttpClient httpClient, IConfiguration configuration, IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions> jsonOptions)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _tokenStorage = new TokenStorageService(configuration);
            _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");
            // Try to restore session from storage
            _currentUserSession = _tokenStorage.GetUserSession();
        }

        protected virtual void OnUserLoggedIn(UserLoggedInEventArgs e)
        {
            UserLoggedIn?.Invoke(this, e);
        }

        protected virtual void OnUserLoggedOut(UserLoggedOutEventArgs e)
        {
            UserLoggedOut?.Invoke(this, e);
        }

        public async Task<UserSession> LoginAsync(string username, string password)
        {
            // Create login request model
            var loginRequest = new
            {
                Username = username,
                Password = password
            };

            // Call the auth endpoint
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest, _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Login failed: {response.ReasonPhrase}");
            }

            // Deserialize the token response
            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(_jsonOptions);
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
            {
                throw new Exception("Invalid token response");
            }

            // Parse the JWT to get claims
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenResponse.Token);

            // Extract user information from token claims
            var userId = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User ID not found in token claims");
            var userName = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? throw new Exception("User name not found in token claims");
            var roles = token.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            // Create and store the session
            _currentUserSession = new UserSession
            {
                UserId = userId,
                UserName = userName,
                Token = tokenResponse.Token,
                Roles = roles,
                ExpiryTimestamp = token.ValidTo,
            };

            // Save token in secure storage
            _tokenStorage.SaveToken(
                tokenResponse.Token,
                token.ValidTo,
                userId,
                userName,
                roles);
            OnUserLoggedIn(new UserLoggedInEventArgs(userId));

            return _currentUserSession;
        }

        public Task LogoutAsync()
        {
            _currentUserSession = null;
            string userId = _tokenStorage.GetUserSession()?.UserId ?? throw new Exception("User ID not found in session");
            _tokenStorage.ClearToken();
            OnUserLoggedOut(new UserLoggedOutEventArgs(userId));
            return Task.CompletedTask;
        }

        public UserSession? GetCurrentUserSession()
        {
            // If we have a cached session but it's expired, clear it
            if (_currentUserSession != null && !_currentUserSession.IsLoggedIn)
            {
                _currentUserSession = null;
                _tokenStorage.ClearToken();
            }

            // If no cached session, try to get from storage
            if (_currentUserSession == null)
            {
                _currentUserSession = _tokenStorage.GetUserSession();

                // If retrieved session is expired, clear it
                if (_currentUserSession != null && !_currentUserSession.IsLoggedIn)
                {
                    _currentUserSession = null;
                    _tokenStorage.ClearToken();
                }
            }

            return _currentUserSession;
        }

        public bool IsUserAdmin()
        {
            var session = GetCurrentUserSession();
            return session?.IsAdmin ?? false;
        }

        public bool IsUserLoggedIn()
        {
            var session = GetCurrentUserSession();
            return session?.IsLoggedIn ?? false;
        }

        public string? GetToken()
        {
            return _currentUserSession?.IsLoggedIn == true ? _currentUserSession.Token : null;
        }

        public string GetUserCNP()
        {
            var session = GetCurrentUserSession();
            return session != null && session.IsLoggedIn ? session.UserId : throw new Exception("User is not logged in or session is invalid");
        }
    }

    // Helper class for deserializing token response
    internal class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
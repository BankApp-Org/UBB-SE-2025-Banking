using Common.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Windows.Storage;

namespace StockApp.Services
{
    // This class provides secure token storage using Windows.Storage.ApplicationDataContainer
    public class TokenStorageService(IConfiguration configuration)
    {
        private readonly string _tokenKey = configuration["Authentication:TokenStorageKey"] ?? "AuthToken";
        private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;

        public void SaveToken(string token, DateTime expiry, string userId, string username, List<string> roles)
        {
            try
            {
                // Store the token and related information
                _localSettings.Values[_tokenKey] = token;
                _localSettings.Values[$"{_tokenKey}_Expiry"] = expiry.ToString("o");
                _localSettings.Values[$"{_tokenKey}_UserId"] = userId;
                _localSettings.Values[$"{_tokenKey}_Username"] = username;
                _localSettings.Values[$"{_tokenKey}_Roles"] = string.Join(",", roles);
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error saving token: {ex.Message}");
                throw;
            }
        }

        public UserSession? GetUserSession()
        {
            try
            {
                if (!_localSettings.Values.ContainsKey(_tokenKey))
                {
                    return null;
                }

                var token = _localSettings.Values[_tokenKey] as string;
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                string expiryStr = _localSettings.Values[$"{_tokenKey}_Expiry"] as string ?? DateTime.UtcNow.ToString("o");
                string userId = _localSettings.Values[$"{_tokenKey}_UserId"] as string ?? string.Empty;
                string username = _localSettings.Values[$"{_tokenKey}_Username"] as string ?? string.Empty;
                string rolesStr = _localSettings.Values[$"{_tokenKey}_Roles"] as string ?? string.Empty;

                DateTime expiry = DateTime.Parse(expiryStr);
                List<string> roles = !string.IsNullOrEmpty(rolesStr)
                    ? [.. rolesStr.Split(',')]
                    : [];

                return new UserSession
                {
                    Token = token,
                    ExpiryTimestamp = expiry,
                    UserId = userId,
                    UserName = username,
                    Roles = roles
                };
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error retrieving token: {ex.Message}");
                return null;
            }
        }

        public void ClearToken()
        {
            try
            {
                _localSettings.Values.Remove(_tokenKey);
                _localSettings.Values.Remove($"{_tokenKey}_Expiry");
                _localSettings.Values.Remove($"{_tokenKey}_UserId");
                _localSettings.Values.Remove($"{_tokenKey}_Username");
                _localSettings.Values.Remove($"{_tokenKey}_Roles");
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error clearing token: {ex.Message}");
                throw;
            }
        }
    }
}
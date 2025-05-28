using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Common.Services;

namespace BankAppDesktop.Services
{
    // This DelegatingHandler adds the JWT token to outgoing HTTP requests
    public class AuthenticationDelegatingHandler(IAuthenticationService authenticationService) : DelegatingHandler
    {
        private readonly IAuthenticationService _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Get the JWT token
            var token = _authenticationService.GetToken();

            // If we have a token, add it to the request's Authorization header
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Call the base implementation to continue the handler chain
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
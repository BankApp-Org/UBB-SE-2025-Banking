namespace Common.Services.Impl
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Common.Services;

    public class ProfanityChecker(HttpClient httpClient) : IProfanityChecker
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        public async Task<bool> IsMessageOffensive(string messageToBeChecked)
        {
            try
            {
                string apiUrl = $"https://www.purgomalum.com/homepageService/containsprofanity?text={Uri.EscapeDataString(messageToBeChecked)}";
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                string result = await response.Content.ReadAsStringAsync();
                return result.Trim().Equals("true", StringComparison.CurrentCultureIgnoreCase);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

}

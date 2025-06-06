namespace Common.Services.Impl
{
    using Common.Models.Social;
    using Common.Services.Social;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class ProfanityChecker(HttpClient httpClient) : IProfanityChecker
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        public async Task<bool> IsMessageOffensive(Message messageToBeChecked)
        {
            try
            {
                if (messageToBeChecked == null || string.IsNullOrWhiteSpace(messageToBeChecked.MessageContent))
                {
                    return false;
                }

                string apiUrl = $"https://www.purgomalum.com/homepageService/containsprofanity?text={Uri.EscapeDataString(messageToBeChecked.MessageContent)}";
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

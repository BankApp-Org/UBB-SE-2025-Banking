using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Common.Services.Impl
{
    public class ImgurImageUploader
    {
        private readonly string _clientId;
        private readonly HttpClient _httpClient;

        public ImgurImageUploader(IConfiguration configuration, HttpClient httpClient)
        {
            _clientId = configuration["ImgurClientId"]
                ?? throw new ArgumentNullException(nameof(configuration), "Imgur:ClientId configuration is missing.");
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string> UploadImageAndGetUrl(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file provided or file is empty.", nameof(file));
            }

            try
            {
                // Validate file type (optional)
                if (!file.ContentType.StartsWith("image/"))
                {
                    throw new InvalidOperationException("File must be an image (e.g., jpg, png).");
                }

                // Create multipart form data content for binary upload
                using var content = new MultipartFormDataContent();
                using var stream = file.OpenReadStream();
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(streamContent, "image", file.FileName);

                // Set authorization header
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.imgur.com/3/image")
                {
                    Content = content
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Client-ID", _clientId);

                // Send request
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // Parse response
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                // Validate response structure
                string? imageUrl = result?.data?.link;
                if (string.IsNullOrEmpty(imageUrl))
                {
                    throw new InvalidOperationException("Imgur API response did not contain a valid image URL.");
                }

                return imageUrl;
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Failed to upload image to Imgur due to a network error.", ex);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to parse Imgur API response.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occurred while uploading the image to Imgur.", ex);
            }
        }
    }
}

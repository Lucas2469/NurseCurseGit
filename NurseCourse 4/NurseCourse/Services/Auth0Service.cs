using NurseCourse.ViewModels;
using System.Text.Json;

namespace NurseCourse.Services
{
    public class Auth0Service
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _auth0TokenEndpoint = $"https://dev-vkqv0tzyahvotuzs.us.auth0.com/oauth/token";
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _audience;
        public Auth0Service(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _clientId = configuration["Auth0:ClientId"];
            _clientSecret = configuration["Auth0:ClientSecret"];
            _audience = configuration["Auth0:Audience"];
        }

        public async Task<Auth0Token> GetAuth0TokenAsync()
        {
            var token = await FetchAuth0TokenAsync();
            Console.WriteLine("Token: " + token);
            return token;
        }

        private async Task<Auth0Token> FetchAuth0TokenAsync()
        {
            var client = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Post, _auth0TokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", _clientId },
                    { "client_secret", _clientSecret },
                    { "audience", _audience },
                    { "grant_type", "client_credentials" },
                })
            };

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<Auth0Token>(jsonResponse);

            return token;
        }
    }
}

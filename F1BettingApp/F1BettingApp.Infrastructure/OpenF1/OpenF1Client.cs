using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace F1BettingApp.Infrastructure.OpenF1
{
    public class OpenF1Client
    {
        private readonly HttpClient _httpClient;
        private readonly OpenF1Settings _settings;

        public OpenF1Client(IHttpClientFactory httpClientFactory, IOptions<OpenF1Settings> options)
        {
            _settings = options.Value;
            _httpClient = httpClientFactory.CreateClient("OpenF1");
        }

        public async Task<string> GetRacesAsync()
        {
            var response = await _httpClient.GetAsync("races");
            return await response.Content.ReadAsStringAsync();
        }
    }

    public class OpenF1Settings
    {
        public string BaseUrl { get; set; } = "https://api.openf1.org";
        public int TimeoutSeconds { get; set; } = 30;
        public int RetryCount { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 5;
    }
}
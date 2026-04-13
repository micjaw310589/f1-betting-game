using System.Net.Http;
using System.Threading.Tasks;

namespace F1BettingApp.Infrastructure.OpenF1
{
    public class OpenF1Client
    {
        private readonly HttpClient _httpClient;

        public OpenF1Client(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.openf1.org/v1/");
        }

        public async Task<string> GetRacesAsync()
        {
            var response = await _httpClient.GetAsync("races");
            return await response.Content.ReadAsStringAsync();
        }
    }
}
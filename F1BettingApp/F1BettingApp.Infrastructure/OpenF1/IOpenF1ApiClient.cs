using System.Collections.Generic;
using System.Threading.Tasks;

namespace F1BettingApp.Infrastructure.OpenF1
{
    public interface IOpenF1ApiClient
    {
        Task<IEnumerable<OpenF1Race>> GetRacesAsync();
    }

    public class OpenF1Race
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Circuit { get; set; }
        public string Country { get; set; }
        public int Season { get; set; }
    }
}
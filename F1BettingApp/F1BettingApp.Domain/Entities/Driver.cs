namespace F1BettingApp.Domain.Entities;

    public class Driver
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string OpenF1DriverId { get; set; }
        public int TeamId { get; set; }
        public Team Team { get; set; }
        public ICollection<Bet> Bets { get; set; }

        public Driver(string name, string country, string openF1DriverId, int teamId)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Driver name is required.", nameof(name));
            if (string.IsNullOrWhiteSpace(country)) throw new ArgumentException("Driver country is required.", nameof(country));
            if (string.IsNullOrWhiteSpace(openF1DriverId)) throw new ArgumentException("OpenF1DriverId is required.", nameof(openF1DriverId));
            if (teamId <= 0) throw new ArgumentException("TeamId must be positive.", nameof(teamId));

            Name = name;
            Country = country;
            OpenF1DriverId = openF1DriverId;
            TeamId = teamId;
            Bets = new List<Bet>();
        }

        public string GetFullName()
        {
            // In a real scenario, we might fetch the team name to return full name, 
            // but based on available properties, we return the name for now.
            return Name; 
        }
    }

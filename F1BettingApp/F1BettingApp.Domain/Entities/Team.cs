using System.Collections.Generic;

namespace F1BettingApp.Domain.Entities;

    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string OpenF1TeamId { get; set; }
        public ICollection<Driver> Drivers { get; set; }

        public Team(string name, string country, string openF1TeamId)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Team name is required.", nameof(name));
            if (string.IsNullOrWhiteSpace(country)) throw new ArgumentException("Team country is required.", nameof(country));
            if (string.IsNullOrWhiteSpace(openF1TeamId)) throw new ArgumentException("OpenF1TeamId is required.", nameof(openF1TeamId));

            Name = name;
            Country = country;
            OpenF1TeamId = openF1TeamId;
            Drivers = new List<Driver>();
        }

        public string GetDrivers()
        {
            // Assuming this method should return a structured string or list of driver names.
            // Returning a comma-separated list of driver names for simplicity.
            return string.Join(", ", Drivers.Select(d => d.Name));
        }
    }

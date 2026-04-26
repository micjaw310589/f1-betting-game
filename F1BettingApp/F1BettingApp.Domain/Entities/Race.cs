using F1BettingApp.Domain.Enums;
using System.Collections.Generic;

namespace F1BettingApp.Domain.Entities;

    public class Race
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public RaceStatus Status { get; set; }
        public string Circuit { get; set; }
        public string Country { get; set; }
        public string OpenF1RaceId { get; set; }
        public int Season { get; set; }
        public ICollection<Bet> Bets { get; set; }

        public Race(string name, DateTime date, string circuit, string country, string openF1RaceId, int season)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Race name is required.");
            if (string.IsNullOrWhiteSpace(circuit)) throw new ArgumentException("Circuit name is required.");
            if (string.IsNullOrWhiteSpace(country)) throw new ArgumentException("Country is required.");
            if (string.IsNullOrWhiteSpace(openF1RaceId)) throw new ArgumentException("OpenF1RaceId is required.");
            if (season <= 0) throw new ArgumentException("Season must be positive.");

            Name = name;
            Date = date;
            Circuit = circuit;
            Country = country;
            OpenF1RaceId = openF1RaceId;
            Season = season;
            Status = RaceStatus.Scheduled;
            Bets = new List<Bet>();
        }

        public bool CanPlaceBets()
        {
            return Status == RaceStatus.Scheduled;
        }

        public bool IsRaceFinished()
        {
            return Status == RaceStatus.Finished;
        }
    }

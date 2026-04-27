using F1BettingApp.Domain.Enums;
using System;
using System.Collections.Generic;

namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// Detailed race information DTO
    /// </summary>
    public class RaceDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Circuit { get; set; }
        public string Country { get; set; }
        public DateTime RaceDate { get; set; }
        public RaceStatus Status { get; set; }
        public string OpenF1RaceId { get; set; }
        public int Season { get; set; }
        public string Weather { get; set; }
        public string TrackCondition { get; set; }
        public string Flag { get; set; }
        public string Paddock { get; set; }
        public string CircuitLayout { get; set; }
        public string SprintRace { get; set; }
        public string SprintDate { get; set; }
    }
}
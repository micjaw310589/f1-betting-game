export interface DriverChampionshipRaceDto {
  raceId: number;
  raceName: string;
  position: number;
  pointsEarned: number;
  raceDate: Date;
}

export interface DriverChampionshipDto {
  driverId: number;
  driverName: string;
  driverCountry?: string;
  teamName: string;
  season: number;
  totalPoints: number; // Zmapowane z pola TotalPoints z backendu
  position: number;
  lastUpdated: Date;
}
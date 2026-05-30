export interface DriverChampionshipRaceDto {
  raceId: number;
  raceName: string;
  position: number;
  pointsEarned: number;
  raceDate: Date;
}

export interface DriverChampionshipDto {
  id: number;
  driverId: number;
  driverName: string;
  teamName: string;
  season: number;
  points: number;
  position: number;
  wins: number;
  podiums: number;
  lastUpdated: Date;
  raceResults?: DriverChampionshipRaceDto[];
}
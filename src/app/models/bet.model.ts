export interface Bet {
  id: number;
  userId: number;
  raceId: number;
  driverIdPrediction: number;
  fastLapPrediction: number;
  pointsAwarded: number;
  status: string;
}
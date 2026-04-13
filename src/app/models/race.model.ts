export interface Race {
  id: number;
  name: string;
  circuit: string;
  raceDate: Date;
  country: string;
  drivers?: Driver[];
}

export interface Driver {
  id: number;
  name: string;
  team: string;
  carNumber: number;
}
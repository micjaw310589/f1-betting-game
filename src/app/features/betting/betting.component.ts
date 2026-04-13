import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-betting',
  templateUrl: './betting.component.html',
  styleUrls: ['./betting.component.scss']
})
export class BettingComponent implements OnInit {
  races: any[] = [];
  selectedRace: any = null;
  selectedDriver: any = null;
  betAmount: number = 0;

  constructor(private apiService: ApiService) { }

  ngOnInit(): void {
    this.loadRaces();
  }

  loadRaces(): void {
    this.apiService.getRaces().subscribe(
      (data) => {
        this.races = data;
      },
      (error) => {
        console.error('Error loading races:', error);
      }
    );
  }

  placeBet(): void {
    if (this.selectedRace && this.selectedDriver && this.betAmount > 0) {
      const betData = {
        raceId: this.selectedRace.id,
        driverId: this.selectedDriver.id,
        amount: this.betAmount
      };

      this.apiService.placeBet(betData).subscribe(
        () => {
          alert('Bet placed successfully!');
        },
        (error) => {
          console.error('Error placing bet:', error);
        }
      );
    }
  }
}
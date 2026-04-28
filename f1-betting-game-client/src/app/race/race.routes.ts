import { Routes } from '@angular/router';
import { RaceListComponent } from './race-list/race-list.component';
import { RaceDetailComponent } from './race-detail/race-detail.component';

export const RaceRoutes: Routes = [
  {
    path: '',
    component: RaceListComponent,
    data: { description: 'View and manage all Formula 1 races' },
    children: [
      {
        path: 'upcoming',
        component: RaceListComponent,
        data: { filterType: 'upcoming' }
      },
      {
        path: 'past',
        component: RaceListComponent,
        data: { filterType: 'past' }
      }
    ]
  },
  {
    path: ':id',
    component: RaceDetailComponent,
    data: { description: 'Detailed information for a specific race' }
  }
];
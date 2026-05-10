import { Routes } from '@angular/router';
import { RaceListComponent } from './race-list/race-list.component';
import { RaceDetailComponent } from './race-detail/race-detail.component';
import { authGuard } from '../auth/guards/auth.guard';

export const RaceRoutes: Routes = [
  {
    path: '',
    component: RaceListComponent,
    canActivate: [authGuard],
    data: { description: 'View and manage all Formula 1 races' },
    children: [
      {
        path: 'upcoming',
        component: RaceListComponent,
        canActivate: [authGuard],
        data: { filterType: 'upcoming' }
      },
      {
        path: 'past',
        component: RaceListComponent,
        canActivate: [authGuard],
        data: { filterType: 'past' }
      }
    ]
  },
  {
    path: ':id',
    component: RaceDetailComponent,
    canActivate: [authGuard],
    data: { description: 'Detailed information for a specific race' }
  }
];
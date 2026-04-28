import { Routes } from '@angular/router';

import { RaceRoutes } from './race/race.routes';

export const routes: Routes = [
  {
    path: '',
    children: [
      {
        path: '',
        redirectTo: 'race', // Redirect to race module if needed, or handle main dashboard
        pathMatch: 'full'
      }
    ]
  },
  {
    path: 'race',
    loadChildren: () => import('./race/race.routes').then(m => m.RaceRoutes)
  }
];

import { Routes } from '@angular/router';

import { RaceRoutes } from './race/race.routes';
import { AdminRoutes } from './admin/admin.routes';

export const routes: Routes = [
  {
    path: '',
    children: [
      {
        path: '',
        redirectTo: 'races', // Redirect to race module if needed, or handle main dashboard
        pathMatch: 'full'
      }
    ]
  },
  {
    path: 'races',
    loadChildren: () => import('./race/race.routes').then(m => m.RaceRoutes)
  },
  {
    path: 'admin',
    loadChildren: () => import('./admin/admin.routes').then(m => m.AdminRoutes)
  }
];

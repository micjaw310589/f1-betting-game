import { Routes } from '@angular/router';

import { AuthRoutes } from './auth/auth.routes';
import { RaceRoutes } from './race/race.routes';
import { AdminRoutes } from './admin/admin.routes';

export const routes: Routes = [
  {
    path: '',
    children: [
      {
        path: '',
        redirectTo: 'races',
        pathMatch: 'full'
      },
      {
        path: 'auth',
        loadChildren: () => import('./auth/auth.routes').then(m => m.AuthRoutes)
      },
      {
        path: 'login',
        redirectTo: 'auth/login',
        pathMatch: 'full'
      },
      {
        path: 'register',
        redirectTo: 'auth/register',
        pathMatch: 'full'
      },
    ]
  },
  {
    path: 'races',
    loadChildren: () => import('./race/race.routes').then(m => m.RaceRoutes)
  },
  {
    path: 'admin',
    loadChildren: () => import('./admin/admin.routes').then(m => m.AdminRoutes)
  },
  {
    path: '**',
    redirectTo: 'races'
  }
];

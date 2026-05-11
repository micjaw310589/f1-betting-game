import { Routes } from '@angular/router';

import { RaceRoutes } from './race/race.routes';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'races',
    pathMatch: 'full'
  },
  {
    path: 'races',
    loadChildren: () => import('./race/race.routes').then(m => m.RaceRoutes)
  },
  {
    path: 'auth',
    loadChildren: () => import('./auth/auth.module').then(m => m.AuthModule)
  },
  {
    path: 'profile',
    loadComponent: () => import('./profile/user-profile/user-profile.component').then(m => m.UserProfileComponent)
  }
];

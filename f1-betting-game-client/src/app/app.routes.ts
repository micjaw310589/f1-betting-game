import { Routes } from '@angular/router';

import { AuthRoutes } from './auth/auth.routes';
import { RaceRoutes } from './race/race.routes';
import { AdminRoutes } from './admin/admin.routes';

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
  },
  {
    path: 'admin',
    loadChildren: () => import('./admin/admin.routes').then(m => m.AdminRoutes)
  },
  {
    path: 'championship',
    loadComponent: () => import('./race/championship/championship-component/championship.component')
      .then(m => m.ChampionshipComponent),
    title: 'F1 Betting - Drivers\' Championship'
  },
{ path: 'championship/driver/:id', loadComponent: () => import('./race/championship/driver-detail-component/driver-detail-component').then(m => m.DriverDetailComponent) }
];

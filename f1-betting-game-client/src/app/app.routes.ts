import { Routes } from '@angular/router';

import { AuthRoutes } from './auth/auth.routes';
import { RaceRoutes } from './race/race.routes';
import { AdminRoutes } from './admin/admin.routes';
import { ProfileRoutes } from './profile/profile.routes';

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
    loadChildren: () => import('./profile/profile.routes').then(m => m.ProfileRoutes)
  },
  {
    path: 'quests',
    loadComponent: () => import('./quest-board/quest-board.component').then(m => m.QuestBoardComponent)
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

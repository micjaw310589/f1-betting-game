import { Routes } from '@angular/router';

export const ProfileRoutes: Routes = [
  {
    path: '',
    redirectTo: 'profile',
    pathMatch: 'full'
  },
  {
    path: 'profile',
    loadComponent: () => import('./user-profile/user-profile.component').then(m => m.UserProfileComponent)
  },
  {
    path: 'stats',
    loadComponent: () => import('./user-stats/user-stats.component').then(m => m.UserStatsComponent)
  },
  {
    path: 'bets',
    loadComponent: () => import('./user-bets/user-bets.component').then(m => m.UserBetsComponent)
  },
  {
    path: 'analytics',
    loadComponent: () => import('./user-analytics/user-analytics.component').then(m => m.UserAnalyticsComponent)
  }
];
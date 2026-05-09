import { Routes } from '@angular/router';
import { AdminUserManagementComponent } from './admin-user-management/admin-user-management.component';
import { adminGuard } from './guards/admin.guard';

export const AdminRoutes: Routes = [
    {
        path: '',
        canActivate: [adminGuard],
        children: [
            {
                path: 'users',
                component: AdminUserManagementComponent,
                data: { description: 'Manage platform users' },
            },
            {
                path: '',
                redirectTo: 'users',
                pathMatch: 'full',
            },
        ],
    },
];

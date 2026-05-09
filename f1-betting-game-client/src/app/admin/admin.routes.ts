import { Routes } from '@angular/router';
import { AdminUserManagementComponent } from './admin-user-management/admin-user-management.component';
import { AdminSystemManagementComponent } from './admin-system-management/admin-system-management.component';
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
                path: 'system',
                component: AdminSystemManagementComponent,
                data: { description: 'Manage system sync and race results' },
            },
            {
                path: '',
                redirectTo: 'users',
                pathMatch: 'full',
            },
        ],
    },
];

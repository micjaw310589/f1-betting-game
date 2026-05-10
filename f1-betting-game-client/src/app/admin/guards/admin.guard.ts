import { inject } from '@angular/core';
import {
    CanActivateFn,
    Router,
    ActivatedRouteSnapshot,
    RouterStateSnapshot,
} from '@angular/router';
import { AuthService } from '../../auth/services/auth.service';

/**
 * Route guard that protects admin routes.
 * - Redirects unauthenticated users to /login
 * - Redirects non-admin users to /races
 */
export const adminGuard: CanActivateFn = (
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    // Check if user is authenticated
    if (!authService.isLoggedIn()) {
        router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
        return false;
    }

    // Check if user is admin
    if (!authService.isAdmin()) {
        router.navigate(['/races']);
        return false;
    }

    return true;
};

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
 * Redirects non-admin users to the home page.
 */
export const adminGuard: CanActivateFn = (
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (authService.isAdmin()) {
        return true;
    }

    // Redirect non-admin users to home
    router.navigate(['/']);
    return false;
};

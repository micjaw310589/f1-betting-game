import { inject } from '@angular/core';
import {
    CanActivateFn,
    Router,
    ActivatedRouteSnapshot,
    RouterStateSnapshot,
} from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Route guard that protects routes requiring authentication.
 * Redirects unauthenticated users to /login.
 */
export const authGuard: CanActivateFn = (
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (authService.isLoggedIn()) {
        return true;
    }

    router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
    return false;
};

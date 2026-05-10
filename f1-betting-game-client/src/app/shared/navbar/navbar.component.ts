import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../auth/services/auth.service';

@Component({
    selector: 'app-navbar',
    standalone: true,
    imports: [CommonModule, RouterModule],
    template: `
        <nav class="navbar">
            <div class="navbar-container">
                <div class="navbar-brand">
                    <a routerLink="/races" class="brand-link">🏎️ F1 Betting</a>
                </div>

                <div class="navbar-links">
                    <a routerLink="/races" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}">
                        Races
                    </a>

                    @if (authService.isAdmin()) {
                        <a routerLink="/admin/users" routerLinkActive="active">
                            Admin
                        </a>
                    }
                </div>

                <div class="navbar-auth">
                    @if (authService.isLoggedIn()) {
                        <div class="user-menu">
                            <span class="user-name">{{ authService.user()?.username }}</span>
                            @if (authService.isAdmin()) {
                                <span class="admin-badge">ADMIN</span>
                            }
                            <button class="btn-logout" (click)="logout()">Logout</button>
                        </div>
                    } @else {
                        <div class="auth-buttons">
                            <a routerLink="/login" class="btn-login">Login</a>
                            <a routerLink="/register" class="btn-register">Sign Up</a>
                        </div>
                    }
                </div>
            </div>
        </nav>
    `,
    styles: [`
        .navbar {
            background: #1a1a2e;
            padding: 0 24px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
            position: sticky;
            top: 0;
            z-index: 100;
        }

        .navbar-container {
            max-width: 1200px;
            margin: 0 auto;
            display: flex;
            align-items: center;
            justify-content: space-between;
            height: 60px;
        }

        .navbar-brand .brand-link {
            font-size: 20px;
            font-weight: 700;
            color: white;
            text-decoration: none;
        }

        .navbar-links {
            display: flex;
            gap: 24px;
        }

        .navbar-links a {
            color: #ccc;
            text-decoration: none;
            font-size: 14px;
            font-weight: 500;
            padding: 8px 0;
            border-bottom: 2px solid transparent;
            transition: all 0.2s;
        }

        .navbar-links a:hover,
        .navbar-links a.active {
            color: white;
            border-bottom-color: #e63946;
        }

        .navbar-auth {
            display: flex;
            align-items: center;
        }

        .user-menu {
            display: flex;
            align-items: center;
            gap: 12px;
        }

        .user-name {
            color: #ccc;
            font-size: 14px;
        }

        .admin-badge {
            background: #e63946;
            color: white;
            padding: 2px 8px;
            border-radius: 4px;
            font-size: 11px;
            font-weight: 700;
        }

        .btn-logout {
            padding: 6px 16px;
            background: transparent;
            color: #ccc;
            border: 1px solid #555;
            border-radius: 6px;
            cursor: pointer;
            font-size: 13px;
            transition: all 0.2s;
        }

        .btn-logout:hover {
            background: #e63946;
            border-color: #e63946;
            color: white;
        }

        .auth-buttons {
            display: flex;
            gap: 12px;
        }

        .btn-login {
            padding: 8px 20px;
            background: transparent;
            color: white;
            border: 1px solid #555;
            border-radius: 6px;
            text-decoration: none;
            font-size: 14px;
            font-weight: 500;
            transition: all 0.2s;
        }

        .btn-login:hover {
            background: rgba(255, 255, 255, 0.1);
        }

        .btn-register {
            padding: 8px 20px;
            background: #e63946;
            color: white;
            border: 1px solid #e63946;
            border-radius: 6px;
            text-decoration: none;
            font-size: 14px;
            font-weight: 500;
            transition: background 0.2s;
        }

        .btn-register:hover {
            background: #c1121f;
        }
    `]
})
export class NavbarComponent {
    constructor(public authService: AuthService) {}

    logout(): void {
        this.authService.logout();
    }
}

import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { NavigationEnd, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { filter, map, Observable } from 'rxjs';

@Component({
  selector: 'app-nav-bar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './nav-bar.html',
  styleUrl: './nav-bar.css',
})
export class NavBar {
showNavbar$: Observable<boolean>;

  constructor(public authService: AuthService, private router: Router) {
    // Sprawdzamy czy obecna trasa NIE zawiera '/auth'
    this.showNavbar$ = this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      map((event: any) => !event.urlAfterRedirects.includes('/auth'))
    );
  }

  onLogout(): void {
    this.authService.logout();
  }

}

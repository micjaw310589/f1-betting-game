import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core'; // Dodaj OnInit
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
export class NavBar implements OnInit { // Dodaj interfejs
  showNavbar$: Observable<boolean>;
  currentUser$: Observable<any>;
  isDropdownOpen = false;

  constructor(public authService: AuthService, private router: Router, private cdr: ChangeDetectorRef) {
    this.currentUser$ = this.authService.currentUser; // Podpinamy się pod strumień
    
    this.showNavbar$ = this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      map((event: any) => !event.urlAfterRedirects.includes('/auth'))
    );
  }
  ngOnInit(): void {
    // To kluczowe: NavBar zaczyna obserwować zmiany użytkownika
    this.authService.currentUser.subscribe(() => {
      this.cdr.markForCheck(); // Powiedz Angularowi: "Hej, dane się zmieniły, sprawdź widok!"
      //this.cdr.detectChanges(); // Wymuś natychmiastową aktualizację
    });
 }

  toggleDropdown(): void {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  closeDropdown(): void {
    this.isDropdownOpen = false;
  }

  onLogout(): void {
    this.authService.logout();
  }
}
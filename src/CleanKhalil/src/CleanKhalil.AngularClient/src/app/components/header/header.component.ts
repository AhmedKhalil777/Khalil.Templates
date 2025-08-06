import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { UnifiedAuthService } from '../../services/unified-auth.service';
import { UserInfo } from '../../models/auth.model';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit {
  currentUser$: Observable<UserInfo | null>;
  authMode: 'jwt' | 'adfs' | 'none';

  constructor(
    private authService: UnifiedAuthService,
    private router: Router
  ) {
    this.currentUser$ = this.authService.currentUser$;
    this.authMode = this.authService.getAuthMode();
  }

  ngOnInit(): void {
  }

  logout(): void {
    this.authService.logout().subscribe(() => {
      this.router.navigate(['/login']);
    });
  }

  navigateToTodos(): void {
    this.router.navigate(['/todos']);
  }

  navigateToLogin(): void {
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }

  getAuthModeDisplay(): string {
    switch (this.authMode) {
      case 'jwt':
        return 'JWT';
      case 'adfs':
        return 'ADFS';
      default:
        return 'None';
    }
  }
} 
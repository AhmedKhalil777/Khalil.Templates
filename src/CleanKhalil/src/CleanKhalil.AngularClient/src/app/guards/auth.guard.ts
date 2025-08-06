import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { UnifiedAuthService } from '../services/unified-auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(
    private authService: UnifiedAuthService,
    private router: Router
  ) { }

  canActivate(): boolean {
    if (this.authService.isAuthenticated()) {
      return true;
    } else {
      this.router.navigate(['/login']);
      return false;
    }
  }
} 
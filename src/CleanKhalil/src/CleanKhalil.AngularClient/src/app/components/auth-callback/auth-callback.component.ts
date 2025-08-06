import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { UnifiedAuthService } from '../../services/unified-auth.service';

@Component({
  selector: 'app-auth-callback',
  templateUrl: './auth-callback.component.html',
  styleUrls: ['./auth-callback.component.scss']
})
export class AuthCallbackComponent implements OnInit {
  loading = true;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: UnifiedAuthService,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.handleCallback();
  }

  private handleCallback(): void {
    this.route.queryParams.subscribe(params => {
      const code = params['code'];
      const state = params['state'];
      const error = params['error'];
      const errorDescription = params['error_description'];

      if (error) {
        this.error = errorDescription || error;
        this.loading = false;
        this.snackBar.open(`Authentication failed: ${this.error}`, 'Close', { duration: 5000 });
        this.router.navigate(['/login']);
        return;
      }

      if (code && state) {
        const callbackResult = this.authService.handleAdfsCallback(code, state);
        
        if (callbackResult) {
          callbackResult.subscribe({
            next: (success) => {
              this.loading = false;
              if (success) {
                this.snackBar.open('Login successful!', 'Close', { duration: 3000 });
                this.router.navigate(['/todos']);
              } else {
                this.error = 'Authentication failed';
                this.snackBar.open('Authentication failed', 'Close', { duration: 5000 });
                this.router.navigate(['/login']);
              }
            },
            error: (error) => {
              this.loading = false;
              this.error = 'Authentication error occurred';
              console.error('Authentication error:', error);
              this.snackBar.open('Authentication error occurred', 'Close', { duration: 5000 });
              this.router.navigate(['/login']);
            }
          });
        } else {
          this.loading = false;
          this.error = 'ADFS authentication not configured';
          this.router.navigate(['/login']);
        }
      } else {
        this.loading = false;
        this.error = 'Invalid callback parameters';
        this.router.navigate(['/login']);
      }
    });
  }

  retryLogin(): void {
    this.router.navigate(['/login']);
  }
} 
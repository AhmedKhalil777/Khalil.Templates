import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { UnifiedAuthService } from '../../services/unified-auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  registerForm!: FormGroup;
  isRegistering = false;
  loading = false;
  authMode: 'jwt' | 'adfs' | 'none';
  supportsRegistration = false;

  constructor(
    private fb: FormBuilder,
    private authService: UnifiedAuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.authMode = this.authService.getAuthMode();
    this.supportsRegistration = this.authService.supportsRegistration();
  }

  ngOnInit(): void {
    // Redirect if already authenticated
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/todos']);
      return;
    }

    this.initForms();
  }

  private initForms(): void {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });

    this.registerForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    });
  }

  onLogin(): void {
    if (this.authMode === 'adfs') {
      // ADFS login - redirect to ADFS
      this.authService.login();
      return;
    }

    // JWT login
    if (this.loginForm.valid) {
      this.loading = true;
      const credentials = this.loginForm.value;

      const loginResult = this.authService.login(credentials);
      if (loginResult) {
        loginResult.subscribe({
          next: (success) => {
            this.loading = false;
            if (success) {
              this.snackBar.open('Login successful!', 'Close', { duration: 3000 });
              this.router.navigate(['/todos']);
            } else {
              this.snackBar.open('Invalid email or password', 'Close', { duration: 5000 });
            }
          },
          error: (error) => {
            this.loading = false;
            this.snackBar.open('Login failed. Please try again.', 'Close', { duration: 5000 });
            console.error('Login error:', error);
          }
        });
      }
    }
  }

  onRegister(): void {
    if (this.registerForm.valid) {
      const formValue = this.registerForm.value;
      
      if (formValue.password !== formValue.confirmPassword) {
        this.snackBar.open('Passwords do not match', 'Close', { duration: 3000 });
        return;
      }

      this.loading = true;
      
      this.authService.register(formValue).subscribe({
        next: (success) => {
          this.loading = false;
          if (success) {
            this.snackBar.open('Registration successful!', 'Close', { duration: 3000 });
            this.router.navigate(['/todos']);
          } else {
            this.snackBar.open('Registration failed', 'Close', { duration: 5000 });
          }
        },
        error: (error) => {
          this.loading = false;
          this.snackBar.open('Registration failed. Please try again.', 'Close', { duration: 5000 });
          console.error('Registration error:', error);
        }
      });
    }
  }

  toggleMode(): void {
    this.isRegistering = !this.isRegistering;
    this.loginForm.reset();
    this.registerForm.reset();
  }

  getAuthModeTitle(): string {
    switch (this.authMode) {
      case 'jwt':
        return this.isRegistering ? 'Create Account' : 'Login with Email';
      case 'adfs':
        return 'Enterprise Login';
      default:
        return 'Login';
    }
  }

  getLoginButtonText(): string {
    if (this.authMode === 'adfs') {
      return 'Login with ADFS';
    }
    return this.isRegistering ? 'Create Account' : 'Login';
  }

  showDemoCredentials(): boolean {
    return this.authMode === 'jwt' && !this.isRegistering;
  }
} 
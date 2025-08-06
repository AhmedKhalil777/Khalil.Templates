import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { LoginRequest, LoginResponse, RegisterRequest, UserInfo } from '../models/auth.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<UserInfo | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadCurrentUser();
  }

  // JWT Authentication Methods
  login(credentials: LoginRequest): Observable<boolean> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, credentials)
      .pipe(
        map(response => {
          if (response && response.token) {
            localStorage.setItem('authToken', response.token);
            localStorage.setItem('refreshToken', response.refreshToken);
            this.currentUserSubject.next(response.user);
            return true;
          }
          return false;
        }),
        catchError(() => of(false))
      );
  }

  register(credentials: RegisterRequest): Observable<boolean> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/register`, credentials)
      .pipe(
        map(response => {
          if (response && response.token) {
            localStorage.setItem('authToken', response.token);
            localStorage.setItem('refreshToken', response.refreshToken);
            this.currentUserSubject.next(response.user);
            return true;
          }
          return false;
        }),
        catchError(() => of(false))
      );
  }

  logout(): Observable<boolean> {
    return this.http.post(`${this.apiUrl}/auth/logout`, {})
      .pipe(
        map(() => {
          this.clearTokens();
          return true;
        }),
        catchError(() => {
          this.clearTokens();
          return of(true);
        })
      );
  }

  refreshToken(): Observable<boolean> {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) {
      return of(false);
    }

    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/refresh`, { refreshToken })
      .pipe(
        map(response => {
          if (response && response.token) {
            localStorage.setItem('authToken', response.token);
            localStorage.setItem('refreshToken', response.refreshToken);
            this.currentUserSubject.next(response.user);
            return true;
          }
          return false;
        }),
        catchError(() => of(false))
      );
  }

  getCurrentUser(): Observable<UserInfo | null> {
    return this.http.get<UserInfo>(`${this.apiUrl}/auth/me`)
      .pipe(
        map(user => {
          this.currentUserSubject.next(user);
          return user;
        }),
        catchError(() => {
          this.clearTokens();
          return of(null);
        })
      );
  }

  isAuthenticated(): boolean {
    const token = localStorage.getItem('authToken');
    if (!token) {
      return false;
    }

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expiry = payload.exp * 1000;
      return Date.now() < expiry;
    } catch {
      return false;
    }
  }

  getToken(): string | null {
    return localStorage.getItem('authToken');
  }

  hasRole(role: string): boolean {
    const user = this.currentUserSubject.value;
    return user?.roles?.includes(role) || false;
  }

  private loadCurrentUser(): void {
    if (this.isAuthenticated()) {
      this.getCurrentUser().subscribe();
    }
  }

  private clearTokens(): void {
    localStorage.removeItem('authToken');
    localStorage.removeItem('refreshToken');
    this.currentUserSubject.next(null);
  }
} 
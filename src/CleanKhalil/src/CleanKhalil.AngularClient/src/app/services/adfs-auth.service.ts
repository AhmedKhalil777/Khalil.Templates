import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { UserInfo } from '../models/auth.model';
import { environment } from '../../environments/environment';

export interface AdfsConfig {
  authority: string;
  clientId: string;
  redirectUri: string;
  postLogoutRedirectUri: string;
  scope: string;
  responseType: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdfsAuthService {
  private currentUserSubject = new BehaviorSubject<UserInfo | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  
  private adfsConfig: AdfsConfig = {
    authority: environment.adfs.authority,
    clientId: environment.adfs.clientId,
    redirectUri: `${window.location.origin}/auth/callback`,
    postLogoutRedirectUri: `${window.location.origin}/`,
    scope: 'openid profile email',
    responseType: 'code'
  };

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.loadCurrentUser();
  }

  // ADFS OAuth/OpenID Connect flow
  login(): void {
    const state = this.generateRandomString(32);
    const nonce = this.generateRandomString(32);
    
    // Store state and nonce for validation
    sessionStorage.setItem('oauth_state', state);
    sessionStorage.setItem('oauth_nonce', nonce);

    const authUrl = this.buildAuthUrl(state, nonce);
    window.location.href = authUrl;
  }

  logout(): Observable<boolean> {
    const idToken = localStorage.getItem('id_token');
    this.clearTokens();
    
    if (idToken) {
      const logoutUrl = `${this.adfsConfig.authority}/ls/?wa=wsignout1.0&wreply=${encodeURIComponent(this.adfsConfig.postLogoutRedirectUri)}`;
      window.location.href = logoutUrl;
    } else {
      this.router.navigate(['/']);
    }
    
    return of(true);
  }

  // Handle OAuth callback
  handleCallback(code: string, state: string): Observable<boolean> {
    const storedState = sessionStorage.getItem('oauth_state');
    
    if (state !== storedState) {
      console.error('Invalid state parameter');
      return of(false);
    }

    return this.exchangeCodeForToken(code).pipe(
      map(response => {
        if (response && response.access_token) {
          localStorage.setItem('access_token', response.access_token);
          if (response.id_token) {
            localStorage.setItem('id_token', response.id_token);
          }
          if (response.refresh_token) {
            localStorage.setItem('refresh_token', response.refresh_token);
          }

          // Extract user info from ID token
          const userInfo = this.parseIdToken(response.id_token);
          if (userInfo) {
            this.currentUserSubject.next(userInfo);
          }

          // Clean up
          sessionStorage.removeItem('oauth_state');
          sessionStorage.removeItem('oauth_nonce');

          return true;
        }
        return false;
      }),
      catchError(error => {
        console.error('Token exchange failed:', error);
        return of(false);
      })
    );
  }

  isAuthenticated(): boolean {
    const token = localStorage.getItem('access_token');
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
    return localStorage.getItem('access_token');
  }

  getCurrentUser(): Observable<UserInfo | null> {
    const idToken = localStorage.getItem('id_token');
    if (idToken) {
      const userInfo = this.parseIdToken(idToken);
      if (userInfo) {
        this.currentUserSubject.next(userInfo);
        return of(userInfo);
      }
    }
    return of(null);
  }

  hasRole(role: string): boolean {
    const user = this.currentUserSubject.value;
    return user?.roles?.includes(role) || false;
  }

  // Private helper methods
  private buildAuthUrl(state: string, nonce: string): string {
    const params = new URLSearchParams({
      response_type: this.adfsConfig.responseType,
      client_id: this.adfsConfig.clientId,
      redirect_uri: this.adfsConfig.redirectUri,
      scope: this.adfsConfig.scope,
      state: state,
      nonce: nonce,
      response_mode: 'query'
    });

    return `${this.adfsConfig.authority}/oauth2/authorize?${params.toString()}`;
  }

  private exchangeCodeForToken(code: string): Observable<any> {
    const tokenEndpoint = `${this.adfsConfig.authority}/oauth2/token`;
    
    const body = new URLSearchParams({
      grant_type: 'authorization_code',
      client_id: this.adfsConfig.clientId,
      code: code,
      redirect_uri: this.adfsConfig.redirectUri
    });

    return this.http.post(tokenEndpoint, body.toString(), {
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded'
      }
    });
  }

  private parseIdToken(idToken: string): UserInfo | null {
    try {
      const payload = JSON.parse(atob(idToken.split('.')[1]));
      
      return {
        id: payload.sub || payload.oid || '',
        email: payload.email || payload.upn || '',
        name: payload.name || payload.given_name || '',
        roles: payload.roles || payload.groups || []
      };
    } catch (error) {
      console.error('Failed to parse ID token:', error);
      return null;
    }
  }

  private generateRandomString(length: number): string {
    const charset = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~';
    let result = '';
    for (let i = 0; i < length; i++) {
      result += charset.charAt(Math.floor(Math.random() * charset.length));
    }
    return result;
  }

  private loadCurrentUser(): void {
    if (this.isAuthenticated()) {
      this.getCurrentUser().subscribe();
    }
  }

  private clearTokens(): void {
    localStorage.removeItem('access_token');
    localStorage.removeItem('id_token');
    localStorage.removeItem('refresh_token');
    sessionStorage.removeItem('oauth_state');
    sessionStorage.removeItem('oauth_nonce');
    this.currentUserSubject.next(null);
  }
} 
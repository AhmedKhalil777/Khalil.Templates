import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
//#if (UseADFS)
import { AdfsAuthService } from './adfs-auth.service';
//#endif
import { UserInfo, LoginRequest, RegisterRequest } from '../models/auth.model';
import { environment } from '../../environments/environment';

//#if (UseADFS)
const USE_ADFS = true;
//#else
// const USE_ADFS = false;
//#endif

//#if (UseJWT)
const USE_JWT = true;
//#else
// const USE_JWT = false;
//#endif

@Injectable({
  providedIn: 'root'
})
export class UnifiedAuthService {
  private authMode: 'jwt' | 'adfs' | 'none';

  constructor(
    private jwtAuthService: AuthService,
//#if (UseADFS)
    private adfsAuthService: AdfsAuthService
//#endif
  ) {
    this.authMode = this.determineAuthMode();
  }

  get currentUser$(): Observable<UserInfo | null> {
    switch (this.authMode) {
      case 'jwt':
        return this.jwtAuthService.currentUser$;
//#if (UseADFS)
      case 'adfs':
        return this.adfsAuthService.currentUser$;
//#endif
      default:
        return this.jwtAuthService.currentUser$;
    }
  }

  login(credentials?: LoginRequest): Observable<boolean> | void {
    switch (this.authMode) {
      case 'jwt':
        if (credentials) {
          return this.jwtAuthService.login(credentials);
        }
        throw new Error('JWT login requires credentials');
//#if (UseADFS)
      case 'adfs':
        this.adfsAuthService.login();
        return;
//#endif
      default:
        throw new Error('No authentication method configured');
    }
  }

  register(credentials: RegisterRequest): Observable<boolean> {
    if (this.authMode === 'jwt') {
      return this.jwtAuthService.register(credentials);
    }
    throw new Error('Registration is only available with JWT authentication');
  }

  logout(): Observable<boolean> {
    switch (this.authMode) {
      case 'jwt':
        return this.jwtAuthService.logout();
//#if (UseADFS)
      case 'adfs':
        return this.adfsAuthService.logout();
//#endif
      default:
        return this.jwtAuthService.logout();
    }
  }

  isAuthenticated(): boolean {
    switch (this.authMode) {
      case 'jwt':
        return this.jwtAuthService.isAuthenticated();
//#if (UseADFS)
      case 'adfs':
        return this.adfsAuthService.isAuthenticated();
//#endif
      default:
        return false;
    }
  }

  getToken(): string | null {
    switch (this.authMode) {
      case 'jwt':
        return this.jwtAuthService.getToken();
//#if (UseADFS)
      case 'adfs':
        return this.adfsAuthService.getToken();
//#endif
      default:
        return null;
    }
  }

  hasRole(role: string): boolean {
    switch (this.authMode) {
      case 'jwt':
        return this.jwtAuthService.hasRole(role);
//#if (UseADFS)
      case 'adfs':
        return this.adfsAuthService.hasRole(role);
//#endif
      default:
        return false;
    }
  }

  getCurrentUser(): Observable<UserInfo | null> {
    switch (this.authMode) {
      case 'jwt':
        return this.jwtAuthService.getCurrentUser();
//#if (UseADFS)
      case 'adfs':
        return this.adfsAuthService.getCurrentUser();
//#endif
      default:
        return this.jwtAuthService.getCurrentUser();
    }
  }

//#if (UseADFS)
  // ADFS specific methods
  handleAdfsCallback(code: string, state: string): Observable<boolean> | null {
    if (this.authMode === 'adfs') {
      return this.adfsAuthService.handleCallback(code, state);
    }
    return null;
  }
//#endif

  getAuthMode(): 'jwt' | 'adfs' | 'none' {
    return this.authMode;
  }

  supportsRegistration(): boolean {
    return this.authMode === 'jwt';
  }

  private determineAuthMode(): 'jwt' | 'adfs' | 'none' {
//#if (UseADFS)
    if (USE_ADFS) {
      return 'adfs';
    } else
//#endif
//#if (UseJWT)
    if (USE_JWT) {
      return 'jwt';
    } else
//#endif
    {
      return 'none';
    }
  }
} 
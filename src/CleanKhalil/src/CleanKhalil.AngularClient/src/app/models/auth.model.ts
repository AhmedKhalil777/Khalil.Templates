export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  name: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expires: Date;
  user: UserInfo;
}

export interface UserInfo {
  id: string;
  email: string;
  name: string;
  roles: string[];
}

export interface RefreshTokenRequest {
  refreshToken: string;
} 
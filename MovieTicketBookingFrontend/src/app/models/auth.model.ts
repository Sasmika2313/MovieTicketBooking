export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
}

export interface LoginResponse {
  userId: number;
  fullName: string;
  email: string;
  role: string;
  token: string;
  expiresAt: string;
}

export interface AuthResponse {
  userId: number;
  fullName: string;
  email: string;
  role: string;
}
import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  AuthResponse
} from '../../models/auth.model';

import { API_BASE_URL } from './api.config';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private http = inject(HttpClient);
  private platformId = inject(PLATFORM_ID);

  private readonly tokenKey = 'movie_booking_token';
  private readonly userKey = 'movie_booking_user';

  private isBrowser(): boolean {
    return isPlatformBrowser(this.platformId);
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(
      `${API_BASE_URL}/auth/login`,
      request
    );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(
      `${API_BASE_URL}/auth/register`,
      request
    );
  }

  saveLogin(response: LoginResponse): void {
    if (!this.isBrowser()) return;

    localStorage.setItem(this.tokenKey, response.token);

    const user = {
      userId: response.userId,
      fullName: response.fullName,
      email: response.email,
      role: response.role
    };

    localStorage.setItem(this.userKey, JSON.stringify(user));
  }

  getToken(): string | null {
    if (!this.isBrowser()) return null;

    return localStorage.getItem(this.tokenKey);
  }

  getCurrentUser(): any {
    if (!this.isBrowser()) return null;

    const user = localStorage.getItem(this.userKey);

    return user ? JSON.parse(user) : null;
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  logout(): void {
    if (!this.isBrowser()) return;

    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
  }
}
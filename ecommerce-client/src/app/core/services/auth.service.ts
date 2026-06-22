import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { ApiService } from './api.service';
import {
  AuthResponse, LoginRequest, RegisterRequest, UserProfile,
  ForgotPasswordRequest, ResetPasswordRequest, ChangePasswordRequest,
  UpdateProfileRequest, RefreshTokenRequest
} from '../models/auth.models';
import { environment } from '../../../environments/environment';

const TOKEN_KEY   = 'sv_access_token';
const REFRESH_KEY = 'sv_refresh_token';
const USER_KEY    = 'sv_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api    = inject(ApiService);
  private readonly router = inject(Router);

  private _user   = signal<UserProfile | null>(this.loadUser());
  private _token  = signal<string | null>(localStorage.getItem(TOKEN_KEY));

  readonly user    = this._user.asReadonly();
  readonly token   = this._token.asReadonly();
  readonly isLoggedIn = computed(() => !!this._user());
  readonly isAdmin    = computed(() => this._user()?.roles.includes('Admin') ?? false);

  register(dto: RegisterRequest): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('/auth/register', dto).pipe(
      tap(r => this.persist(r))
    );
  }

  login(dto: LoginRequest): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('/auth/login', dto).pipe(
      tap(r => this.persist(r))
    );
  }

  logout(): void {
    const refresh = localStorage.getItem(REFRESH_KEY);
    if (refresh) {
      this.api.post('/auth/logout', { refreshToken: refresh }).subscribe({ error: () => {} });
    }
    this.clear();
    this.router.navigate(['/auth/login']);
  }

  refreshToken(): Observable<AuthResponse> {
    const body: RefreshTokenRequest = {
      accessToken:  localStorage.getItem(TOKEN_KEY) ?? '',
      refreshToken: localStorage.getItem(REFRESH_KEY) ?? ''
    };
    return this.api.post<AuthResponse>('/auth/refresh', body).pipe(
      tap(r => this.persist(r))
    );
  }

  getProfile(): Observable<UserProfile> {
    return this.api.get<UserProfile>('/auth/profile').pipe(
      tap(u => { this._user.set(u); localStorage.setItem(USER_KEY, JSON.stringify(u)); })
    );
  }

  updateProfile(dto: UpdateProfileRequest): Observable<UserProfile> {
    return this.api.put<UserProfile>('/auth/profile', dto).pipe(
      tap(u => { this._user.set(u); localStorage.setItem(USER_KEY, JSON.stringify(u)); })
    );
  }

  forgotPassword(dto: ForgotPasswordRequest): Observable<void> {
    return this.api.post<void>('/auth/forgot-password', dto);
  }

  resetPassword(dto: ResetPasswordRequest): Observable<void> {
    return this.api.post<void>('/auth/reset-password', dto);
  }

  changePassword(dto: ChangePasswordRequest): Observable<void> {
    return this.api.post<void>('/auth/change-password', dto);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  private persist(r: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, r.accessToken);
    localStorage.setItem(REFRESH_KEY, r.refreshToken);
    localStorage.setItem(USER_KEY, JSON.stringify(r.user));
    this._user.set(r.user);
    this._token.set(r.accessToken);
  }

  private clear(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_KEY);
    localStorage.removeItem(USER_KEY);
    this._user.set(null);
    this._token.set(null);
  }

  private loadUser(): UserProfile | null {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? JSON.parse(raw) : null;
  }
}

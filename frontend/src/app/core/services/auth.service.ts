import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse } from '../models/auth.models';

const STORAGE_KEY = 'purchase-bill.session';

interface StoredSession {
  token: string;
  expiresAtUtc: string;
  username: string;
}

/**
 * Owns the client-side session: calls the login endpoint, and keeps the JWT in
 * sessionStorage (cleared when the tab closes) rather than localStorage, so a stolen/shared
 * machine doesn't leave a session behind indefinitely. Session state is exposed as a signal so
 * the route guard and the shell can react to login/logout without polling.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly session = signal<StoredSession | null>(this.readStoredSession());

  readonly username = computed(() => this.session()?.username ?? null);
  readonly isAuthenticated = computed(() => {
    const current = this.session();
    return !!current && new Date(current.expiresAtUtc).getTime() > Date.now();
  });

  constructor(private readonly http: HttpClient) {}

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, request).pipe(
      tap((response) => {
        const stored: StoredSession = {
          token: response.token,
          expiresAtUtc: response.expiresAtUtc,
          username: response.username,
        };
        sessionStorage.setItem(STORAGE_KEY, JSON.stringify(stored));
        this.session.set(stored);
      }),
    );
  }

  logout(): void {
    sessionStorage.removeItem(STORAGE_KEY);
    this.session.set(null);
  }

  getToken(): string | null {
    return this.session()?.token ?? null;
  }

  private readStoredSession(): StoredSession | null {
    try {
      const raw = sessionStorage.getItem(STORAGE_KEY);
      return raw ? (JSON.parse(raw) as StoredSession) : null;
    } catch {
      return null;
    }
  }
}

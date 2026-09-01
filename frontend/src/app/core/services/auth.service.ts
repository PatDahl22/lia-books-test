import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { AuthRequest, AuthResponse, AuthSession } from '../models/auth.models';

const SESSION_KEY = 'lia-books-session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly session = signal<AuthSession | null>(this.readSession());

  readonly username = computed(() => this.session()?.username ?? null);
  readonly isAuthenticated = computed(() => this.hasValidSession());

  login(request: AuthRequest) {
    return this.http.post<AuthResponse>('/api/auth/login', request).pipe(
      tap((response) => this.storeSession(response)),
    );
  }

  register(request: AuthRequest) {
    return this.http.post<AuthResponse>('/api/auth/register', request).pipe(
      tap((response) => this.storeSession(response)),
    );
  }

  getToken(): string | null {
    const currentSession = this.session();
    if (!currentSession || new Date(currentSession.expiresAt).getTime() <= Date.now()) {
      this.clearSession();
      return null;
    }

    return currentSession.token;
  }

  logout(): void {
    this.clearSession();
  }

  private hasValidSession(): boolean {
    const currentSession = this.session();
    return !!currentSession && new Date(currentSession.expiresAt).getTime() > Date.now();
  }

  private storeSession(session: AuthSession): void {
    localStorage.setItem(SESSION_KEY, JSON.stringify(session));
    this.session.set(session);
  }

  private clearSession(): void {
    localStorage.removeItem(SESSION_KEY);
    if (this.session()) {
      this.session.set(null);
    }
  }

  private readSession(): AuthSession | null {
    const storedValue = localStorage.getItem(SESSION_KEY);
    if (!storedValue) {
      return null;
    }

    try {
      const parsedSession = JSON.parse(storedValue) as AuthSession;
      if (!parsedSession.token || !parsedSession.username || !parsedSession.expiresAt) {
        localStorage.removeItem(SESSION_KEY);
        return null;
      }

      return parsedSession;
    } catch {
      localStorage.removeItem(SESSION_KEY);
      return null;
    }
  }
}

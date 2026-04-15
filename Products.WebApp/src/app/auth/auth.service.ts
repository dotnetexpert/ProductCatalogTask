import { Inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, map, Observable, of, tap } from 'rxjs';
import { API_BASE_URL } from '../api/client';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http: HttpClient;
  private readonly baseUrl: string;

  readonly isLoggedIn = signal(false);

  constructor(http: HttpClient, @Inject(API_BASE_URL) baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl.replace(/\/+$/, '');
  }

  setLoggedIn(v: boolean): void {
    this.isLoggedIn.set(v);
  }

  checkAuth(): Observable<boolean> {
    return this.http
      .get<{ isAuthenticated: true }>(`${this.baseUrl}/api/auth/me`, { withCredentials: true })
      .pipe(
        map(() => true),
        catchError(() => of(false)),
        tap(v => this.setLoggedIn(v)),
      );
  }

  logout(): void {
    this.setLoggedIn(false);
  }
}


import { Injectable, signal } from '@angular/core';

const KEY = 'auth.isLoggedIn';

@Injectable({ providedIn: 'root' })
export class AuthService {
  readonly isLoggedIn = signal(this.read());

  setLoggedIn(v: boolean): void {
    this.isLoggedIn.set(v);
    if (v) localStorage.setItem(KEY, '1');
    else localStorage.removeItem(KEY);
  }

  logout(): void {
    this.setLoggedIn(false);
  }

  private read(): boolean {
    return localStorage.getItem(KEY) === '1';
  }
}


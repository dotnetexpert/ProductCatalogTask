import { CommonModule } from '@angular/common';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter, finalize } from 'rxjs';
import { AuthService } from './auth/auth.service';
import { Client } from './api/client';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('products-ui');

  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly url = signal(this.router.url);
  private readonly auth = inject(AuthService);
  private readonly api = inject(Client);
  protected loggingOut = false;

  protected readonly showAppShell = computed(() => {
    const u = (this.url() || '').split('?')[0].split('#')[0];
    return this.auth.isLoggedIn() && u !== '' && u !== '/' && !u.startsWith('/login');
  });

  constructor() {
    const sub = this.router.events
      .pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd))
      .subscribe((e) => this.url.set(e.urlAfterRedirects));
    this.destroyRef.onDestroy(() => sub.unsubscribe());
  }

  logout(): void {
    if (this.loggingOut) return;
    this.loggingOut = true;
    this.api
      .logout()
      .pipe(finalize(() => (this.loggingOut = false)))
      .subscribe({
        next: () => {
          this.auth.logout();
          this.router.navigateByUrl('');
        },
        error: (e: unknown) => {
          console.error('Logout Error', e);
          this.auth.logout();
          this.router.navigateByUrl('');
        },
      });
  }
}

import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import {
  HTTP_INTERCEPTORS,
  HttpErrorResponse,
  HttpInterceptorFn,
  provideHttpClient,
  withInterceptors,
  withInterceptorsFromDi,
} from '@angular/common/http';
import { provideRouter, Router } from '@angular/router';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

import { routes } from './app.routes';
import { API_BASE_URL, Client } from './api/client';
import { environment } from '../environments/environment';
import { AuthInterceptor } from './shared/Helper/auth.interceptor';
import { AuthService } from './auth/auth.service';

const auth401RedirectInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const auth = inject(AuthService);
  return next(req).pipe(
    catchError((e: unknown) => {
      const err = e as HttpErrorResponse;
      if (err?.status === 401) {
        auth.setLoggedIn(false);
        router.navigateByUrl('');
      }
      return throwError(() => e);
    }),
  );
};

const httpErrorLoggingInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(
    catchError((e: unknown) => {
      const err = e as HttpErrorResponse;
      const payload = {
        method: req.method,
        url: req.urlWithParams,
        status: err?.status,
        statusText: err?.statusText,
        message: err?.message,
        error: err?.error,
      };
      console.error('HTTP Error', payload);
      return throwError(() => e);
    }),
  );

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),

    provideHttpClient(
      withInterceptorsFromDi(),
      withInterceptors([
        auth401RedirectInterceptor,
        httpErrorLoggingInterceptor,
      ]),
    ),
    provideRouter(routes),
    Client,
    { provide: API_BASE_URL, useValue: environment.apiBaseUrl },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true,
    },
  ],
};

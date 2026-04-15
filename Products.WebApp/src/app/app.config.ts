import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import {
  HTTP_INTERCEPTORS,
  HttpErrorResponse,
  HttpInterceptorFn,
  provideHttpClient,
  withInterceptors,
} from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { routes } from './app.routes';
import { API_BASE_URL, Client } from './api/client';
import { environment } from '../environments/environment';
import { AuthInterceptor } from './shared/Helper/auth.interceptor';

const noCredentialsInterceptor: HttpInterceptorFn = (req, next) =>
  next(req.clone({ withCredentials: false }));

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

    provideHttpClient(withInterceptors([noCredentialsInterceptor, httpErrorLoggingInterceptor])),
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

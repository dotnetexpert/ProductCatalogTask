import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private readonly apiKey = 'MySuperSecretApiKey123';
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // const token = localStorage.getItem('token');

    // let headers: Record<string, string> = {
    //   'x-api-key': this.apiKey, // add API key to all requests
    // };

    // if (token) {
    //   headers['Authorization'] = `Bearer ${token}`;
    // }

    const cloned = req.clone({
      withCredentials: true, // This makes cookies go automatically
    });

    return next.handle(cloned);
  }
}

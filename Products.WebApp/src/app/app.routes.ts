import { Routes } from '@angular/router';
import { authGuard } from './auth/auth.guard';
import { reverseAuthGuard } from './auth/reverse-auth.guard';

export const routes: Routes = [
  {
    path: '',
    canActivate: [reverseAuthGuard],
    loadComponent: () =>
      import('./features/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'login',
    redirectTo: '',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    redirectTo: 'products',
    pathMatch: 'full'
  },
  {
    path: 'products',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./features/product/Routes/products.routes').then(m => m.PRODUCTS_ROUTES)
  },
  {
    path: '**',
    redirectTo: 'products'
  }
];

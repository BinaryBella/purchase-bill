import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    path: 'purchase-bill',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/purchase-bill/purchase-bill-page/purchase-bill-page').then(
        (m) => m.PurchaseBillPage,
      ),
  },
  { path: '**', redirectTo: 'login' },
];

import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    // Every authenticated page renders inside the shell (top bar + sidebar).
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./core/layout/app-shell/app-shell').then((m) => m.AppShell),
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard-page/dashboard-page').then(
            (m) => m.DashboardPage,
          ),
      },
      {
        path: 'purchase-bill',
        loadComponent: () =>
          import('./features/purchase-bill/purchase-bill-page/purchase-bill-page').then(
            (m) => m.PurchaseBillPage,
          ),
      },
    ],
  },
  { path: '**', redirectTo: 'dashboard' },
];

import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * A 401 on any call other than the login attempt itself means the session token has expired
 * or was rejected - clear it and send the user back to the login page. The login call's own
 * 401 (wrong credentials) is left alone so the login form can show its own message.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: unknown) => {
      const isLoginRequest = req.url.includes('/auth/login');
      if (error instanceof HttpErrorResponse && error.status === 401 && !isLoginRequest) {
        authService.logout();
        router.navigate(['/login']);
      }
      return throwError(() => error);
    }),
  );
};

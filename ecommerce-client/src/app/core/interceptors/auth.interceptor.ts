import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

// ─── Race-Condition-Safe Token Refresh ─────────────────────────────────────────
// Problem: Multiple 401s arrive simultaneously → each calls refreshToken() → race.
// Solution: Queue waiters on a BehaviorSubject; only one refresh in-flight at a time.

let isRefreshing      = false;
const refreshToken$   = new BehaviorSubject<string | null>(null);

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const authService = inject(AuthService);

  const token   = authService.getAccessToken();
  const authReq = token ? addToken(req, token) : req;

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      // Only intercept 401s that are NOT the refresh call itself
      if (err.status !== 401 || req.url.includes('/auth/refresh')) {
        return throwError(() => err);
      }

      if (isRefreshing) {
        // ── Another refresh is in-flight — wait for its result ──
        return refreshToken$.pipe(
          filter(t => t !== null),          // Wait until token is emitted
          take(1),
          switchMap(newToken => next(addToken(req, newToken!)))
        );
      }

      // ── Start a new refresh ──
      isRefreshing = true;
      refreshToken$.next(null);            // Block waiters

      return authService.refreshToken().pipe(
        switchMap(response => {
          isRefreshing = false;
          refreshToken$.next(response.accessToken);   // Unblock waiters
          return next(addToken(req, response.accessToken));
        }),
        catchError(refreshErr => {
          isRefreshing = false;
          refreshToken$.next(null);
          authService.logout();
          return throwError(() => refreshErr);
        })
      );
    })
  );
};

function addToken(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}

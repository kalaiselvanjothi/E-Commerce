import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../services/toast.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 0) {
        toast.error('Network error. Please check your connection.');
      } else if (err.status === 403) {
        toast.error('You do not have permission to perform this action.');
      } else if (err.status === 404) {
        // Let components handle 404s
      } else if (err.status >= 500) {
        toast.error('Server error. Please try again later.');
      } else if (err.status !== 401) {
        const message = err.error?.message ?? err.error?.errors?.[0] ?? 'Something went wrong.';
        if (message) toast.error(message);
      }
      return throwError(() => err);
    })
  );
};

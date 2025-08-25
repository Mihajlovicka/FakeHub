import { inject } from '@angular/core';
import {
  HttpEvent,
  HttpRequest,
  HttpErrorResponse,
  HttpResponse,
  HttpInterceptorFn,
  HttpHandlerFn,
} from '@angular/common/http';
import { throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { PopupHandlerService } from '../services/popup-handler.service';
import { Router } from '@angular/router';

export const HttpResponseInterceptor: HttpInterceptorFn = (
  request: HttpRequest<any>,
  next: HttpHandlerFn
) => {
  const popupHandler: PopupHandlerService = inject(PopupHandlerService);
  const router: Router = inject(Router);
  return next(request).pipe(
    map((event: HttpEvent<any>) => {
      if (event instanceof HttpResponse) {
        return event.clone({ body: event.body?.result });
      }
      return event;
    }),
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        popupHandler.openSnackbar('Unauthorized');
        router.navigate(['/login']);
      } else if (error.status === 403) {
        popupHandler.openSnackbar('Forbidden');
        router.navigate(['/login']);
      } else if (error.status === 400) {
        popupHandler.openSnackbar(error.error.errorMessage);
      } else if (error.status === 500) {
        popupHandler.openSnackbar(error.error.message);
      } else {
        popupHandler.openSnackbar('An error occurred. Please try again.');
      }
      return throwError(() => error);
    })
  );
};

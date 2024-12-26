import { inject } from "@angular/core";
import {
  HttpRequest,
  HttpInterceptorFn,
  HttpHandlerFn,
} from "@angular/common/http";
import { UserService } from "../services/user.service";

export const AuthInterceptor: HttpInterceptorFn = (
  request: HttpRequest<any>,
  next: HttpHandlerFn
) => {
  const userService: UserService = inject(UserService);
  if (request.headers.get("No-Auth") === "True") {
    return next(request);
  }

  const token: string | null = userService.getToken();

  if (token) {
    request = !request.headers.has("Authorization")
      ? request.clone({
          setHeaders: {
            Authorization: `Bearer ${token}`,
          },
        })
      : request;
  }

  return next(request);
};

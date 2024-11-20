import {CanActivateFn, Router,} from "@angular/router";
import {UserService} from "../services/user.service";
import {inject} from "@angular/core";

export const NoAuthGuard: CanActivateFn = () => {
  const userService: UserService = inject(UserService);
  const router: Router = inject(Router);
  const token: string | null = userService.getToken();
  if (!token) {
    return true;
  }
  router.navigate([""]);
  return false;
};

import {
  CanActivateFn,
  Router,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
} from "@angular/router";
import { inject } from "@angular/core";
import { UserService } from "../services/user.service";
import { UserRole } from "../model/user-role";

export const AuthGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
) => {
  const userService: UserService = inject(UserService);
  const router: Router = inject(Router);

  const role: string | null = userService.getRole();
  const requiredRole: UserRole[] = route.data["requiredRole"];

  if (role && requiredRole.includes(role as UserRole)) {
    return true;
  }

  router.navigate(["login"]);
  return false;
};

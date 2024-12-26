import {ActivatedRouteSnapshot, CanActivateFn, Router, RouterStateSnapshot} from "@angular/router";
import {UserService} from "../services/user.service";
import {inject} from "@angular/core";
import {UserRole} from "../model/user-role";

export const AuthGuard: CanActivateFn = (
    route: ActivatedRouteSnapshot,
) => {
  const userService: UserService = inject(UserService);
  const router: Router = inject(Router);

  const role: string | null = userService.getRole();
  const requiredRole: UserRole[] = route.data["requiredRole"];

  if (role && requiredRole.includes(role as UserRole)) {
    return true;
  }

  router.navigate([""]);
  return false;
};
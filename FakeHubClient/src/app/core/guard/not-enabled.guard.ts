import {CanActivateFn, Router} from "@angular/router";
import {UserService} from "../services/user.service";
import {inject} from "@angular/core";
import {UserRole} from "../model/user-role";

export const NotEnabledGuard: CanActivateFn = (
) => {
    const userService: UserService = inject(UserService);
    const router: Router = inject(Router);

    const token = userService.getToken();
    const role = userService.getRole();

    if(token && role == UserRole.SUPERADMIN && !userService.isEnabled()){
        router.navigate(['/change-password']);
        return false;
    }
    return true;
};
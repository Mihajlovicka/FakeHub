import { Routes } from "@angular/router";
import { RegisterComponent } from "./features/user/register/register.component";
import { HomeComponent } from "./features/home/home.component";
import { LoginComponent } from "./features/user/login/login.component";
import { UserRole } from "./core/model/user-role";
import { NoAuthGuard } from "./core/guard/no-auth.guard";
import { ProfileComponent } from "./features/user/profile/profile.component";
import { AuthGuard } from "./core/guard/auth.guard";

export const routes: Routes = [
  {
    path: "",
    component: HomeComponent,
  },
  {
    path: "register",
    canActivate: [NoAuthGuard],
    component: RegisterComponent,
  },
  {
    path: "login",
    canActivate: [NoAuthGuard],
    component: LoginComponent,
  },
  {
    path: "register/admin",
    component: RegisterComponent,
    canActivate: [AuthGuard],
    data: { requiredRole: [UserRole.SUPERADMIN] },
  },
  {
    path: 'profile/:username',
    component: ProfileComponent,
    canActivate: [AuthGuard],
    data: { requiredRole: [UserRole.USER, UserRole.ADMIN] },
  },
  {
    path: "**",
    redirectTo: "",
  },
  {
    path: 'profile/:username',
    component: ProfileComponent,
    canActivate: [AuthGuard],
    data: { requiredRole: [UserRole.USER, UserRole.ADMIN] },
  },
];

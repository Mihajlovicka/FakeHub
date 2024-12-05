import { Routes } from '@angular/router';
import { RegisterComponent } from './features/user/register/register.component';
import { HomeComponent } from './features/home/home.component';
import { LoginComponent } from './features/user/login/login.component';
import { UserRole } from './core/model/user-role';
import { NoAuthGuard } from './core/guard/no-auth.guard';
import { AuthGuard } from './core/guard/auth.guard';
import { ChangePasswordComponent } from "./features/user/change-password/change-password.component";
import { NotEnabledGuard } from "./core/guard/not-enabled.guard";
import { ProfileComponent } from './features/user/profile/profile.component';
import { SettingsComponent } from './features/settings/settings.component';
import { EmailEditComponent } from './features/settings/email-edit/email-edit.component';
import { AddOrganizationComponent } from './features/organization/add-organization/add-organization.component';
import { EditOrganizationComponent } from './features/organization/edit-organization/edit-organization.component';

export const routes: Routes = [
  {
    path: '',
    component: HomeComponent,
    canActivate: [NotEnabledGuard]
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
    path: 'change-password',
    component: ChangePasswordComponent,
    canActivate: [AuthGuard],
    data: { requiredRole: [UserRole.SUPERADMIN, UserRole.ADMIN, UserRole.USER] }
  },
  {
    path: 'settings',
    component: SettingsComponent,
    canActivate: [AuthGuard],
    data: { requiredRole: [UserRole.USER, UserRole.ADMIN] },
  },
  {
    path: 'settings/email-edit',
    component: EmailEditComponent,
    canActivate: [AuthGuard],
    data: { requiredRole: [UserRole.USER, UserRole.ADMIN] },
  },
  {
    path: "organization",
    canActivate: [AuthGuard],
    data: { requiredRole: [UserRole.USER] },
    children: [
      {
        path: "add",
        component: AddOrganizationComponent,
      },
      {
        path: "edit/:id",
        component: EditOrganizationComponent,
      },
    ],
  },
  {
    path: '**',
    redirectTo: ''
  }
];

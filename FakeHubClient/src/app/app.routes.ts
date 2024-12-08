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
import { OrganizationsComponent } from './features/organization/organizations/organizations.component';
import { ViewOrganizationComponent } from './features/organization/view-organization/view-organization.component';
import { AddTeamComponent } from './features/team/add-team/add-team.component';
import { ViewTeamComponent } from './features/team/view-team/view-team.component';
import { EditTeamComponent } from './features/team/edit-team/edit-team.component';
import {UsersListViewComponent} from "./features/user/users-list-view/users-list-view.component";

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
    data: { requiredRole: [UserRole.USER, UserRole.ADMIN,UserRole.SUPERADMIN] },
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
    data: { requiredRole: [UserRole.USER, UserRole.ADMIN, UserRole.SUPERADMIN] },
  },
  {
    path: 'settings/email-edit',
    component: EmailEditComponent,
    canActivate: [AuthGuard],
    data: { requiredRole: [UserRole.USER, UserRole.ADMIN, UserRole.SUPERADMIN] },
  },
  {
    path: 'organizations',
    canActivate: [AuthGuard],
    component: OrganizationsComponent,
    data: { requiredRole: [UserRole.USER] }
  },
  {
    path: "organization",
    canActivate: [AuthGuard],
    data: { requiredRole: [UserRole.USER] },
    children: [
      {
        path: "view/:name",
        component: ViewOrganizationComponent,
      },
      {
        path: "add",
        component: AddOrganizationComponent,
      },
      {
        path: "edit/:name",
        component: EditOrganizationComponent,
      },
      {
        path: "team",
        children: [
          {
            path: "add/:organizationName",
            component: AddTeamComponent,
          },
          {
            path: "view/:organizationName/:teamName",
            component: ViewTeamComponent,
          },
          {
            path: "edit/:organizationName/:teamName",
            component: EditTeamComponent,
          },
        ],
      }
    ]
  },
  {
    path: 'users',
    component: UsersListViewComponent,
    canActivate: [AuthGuard],
    data: { requiredRole: [UserRole.ADMIN, UserRole.SUPERADMIN]}
  },
  {
    path: 'users/admins',
    component: UsersListViewComponent,
    canActivate: [AuthGuard],
    data: { requiredRole: [UserRole.SUPERADMIN]}
  },
  {
    path: '**',
    redirectTo: ''
  }
];

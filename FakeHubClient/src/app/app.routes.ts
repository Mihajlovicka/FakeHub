import { Routes } from '@angular/router';
import { RegisterComponent } from './features/user/register/register.component';
import { AppComponent } from './app.component';
import { AuthGuard } from './core/guard/auth.guard';
import { LoginComponent } from './features/user/login/login.component';
import { UserRole } from './core/model/user-role';

export const routes: Routes = [
  {
    path: '',
    component: AppComponent,
    canActivate: [AuthGuard],
    data: { requiredRole: [UserRole.USER, UserRole.ADMIN] },
  },
  {
    path: 'register',
    component: RegisterComponent,
  },
  {
    path: 'login',
    component: LoginComponent,
  },
];
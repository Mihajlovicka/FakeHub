import { Routes } from '@angular/router';
import { RegisterComponent } from './features/user/register/register.component';
import { HomeComponent } from './features/home/home.component';
import { LoginComponent } from './features/user/login/login.component';

export const routes: Routes = [
  {
    path: '',
    component: HomeComponent,
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
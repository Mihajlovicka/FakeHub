import { Routes } from '@angular/router';
import { RegisterComponent } from './features/user/register/register.component';
import { AppComponent } from './app.component';

export const routes: Routes = [
  {
    path: '',
    component: AppComponent,
  },
  {
    path: 'register',
    component: RegisterComponent,
  },
];

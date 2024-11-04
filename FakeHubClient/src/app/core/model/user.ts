import { UserRole } from './user-role';

export class RegistrationRequestDto {
  email: string;
  username: string;
  password?: string;
  role: UserRole;

  constructor() {
    this.email = '';
    this.username = '';
    this.password = '';
    this.role = UserRole.USER;
  }
}

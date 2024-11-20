export class RegistrationRequestDto {
  email: string;
  username: string;
  password?: string;

  constructor() {
    this.email = '';
    this.username = '';
    this.password = '';
  }
}

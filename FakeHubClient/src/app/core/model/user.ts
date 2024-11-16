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

export class UserProfileResponseDto {
  username: string;
  email: string;
  role: string;
  createdAt: Date;

  constructor() {
    this.username = '';
    this.email = '';
    this.role = '';
    this.createdAt = new Date();
  }
}
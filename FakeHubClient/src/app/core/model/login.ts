export class LoginRequestDto {
  email: string;
  password: string;

  constructor() {
    this.email = '';
    this.password = '';
  }
}

export interface LoginResponseDto {
  token: string;
}

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
  badge: UserBadge;

  constructor() {
    this.username = '';
    this.email = '';
    this.role = '';
    this.createdAt = new Date();
    this.badge = UserBadge.None;
  }
}

export enum UserBadge {
  None = 0,
  VerifiedPublisher = 1,
  SponsoredOSS = 2,
  DockerOfficialImage = 3
}
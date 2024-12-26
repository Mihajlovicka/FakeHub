import { UserProfileResponseDto } from "./user";

export interface Team {
  name: string;
  description: string;
  createdAt: Date;
  teamRole: TeamRole;
  owner?: string;
  users?: UserProfileResponseDto[];
}

export enum TeamRole {
  Admin = "Admin",
  ReadWrite = "ReadWrite",
  ReadOnly = "ReadOnly",
}

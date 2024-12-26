import { Team } from "./team";
import { UserProfileResponseDto } from "./user";

export interface Organization {
  name: string;
  description: string;
  imageBase64: string;
  owner?: string;
  teams?: Team[];
  users?: UserProfileResponseDto[];
}

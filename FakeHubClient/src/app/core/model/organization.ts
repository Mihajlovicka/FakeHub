import { Team } from "./team";

export interface Organization {
  name: string;
  description: string;
  imageBase64: string;
  owner?: string;
  teams: Team[];
}

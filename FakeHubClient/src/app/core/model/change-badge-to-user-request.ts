import { UserBadge } from "./user";

export class ChangeUserBadgeRequest {
    badge: UserBadge;
    username: string;

    constructor(badge: UserBadge, username: string) {
        this.badge = badge;
        this.username = username;
      }
}
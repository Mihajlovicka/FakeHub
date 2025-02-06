import { UserBadge } from "./user";

export interface Repository {
    id?: number;
    name: string;
    description: string;
    isPrivate: boolean;
    ownerId: number;
    ownedBy: RepositoryOwnedBy;
    badge: UserBadge;
}

export enum RepositoryOwnedBy {
    ORGANIZATION=0, USER=1, ADMIN=2, SUPERADMIN=3
}
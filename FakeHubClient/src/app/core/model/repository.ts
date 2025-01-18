export interface Repository {
    id?: number;
    name: string;
    description: string;
    isPrivate: boolean;
    ownerId: number;
    ownedBy: RepositoryOwnedBy;
}

export enum RepositoryOwnedBy {
    ORGANIZATION=0, USER=1
}
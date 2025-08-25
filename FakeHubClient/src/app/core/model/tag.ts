export interface Tag {
    id: number;
    name: string;
    pushTime: Date;
    pullTime: Date;
}

export interface Artifact {
    id: number;
    tags: Tag[];
    repositoryName: string;
    extraAttrs: ExtraAttrs;
}

export interface ExtraAttrs {
    os: string;
}
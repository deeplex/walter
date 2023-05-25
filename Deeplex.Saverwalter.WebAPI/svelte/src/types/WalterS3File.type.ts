export type WalterS3File = {
    FileName: string;
    Key: string;
    LastModified: number;
    Size?: number;
    Blob?: Blob;
    Type?: string;
};

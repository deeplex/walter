export class WalterFile {
    constructor(
        public fileName: string,
        public key: string,
        public lastModified: number,
        public size?: number,
        public blob?: Blob,
        public type?: string
    ) {}

    static fromFile(file: File, key: string) {
        return new WalterFile(
            file.name,
            key,
            file.lastModified,
            file.size,
            file,
            file.type
        );
    }
}

import { S3URL, walter_s3_get_files } from '$walter/services/s3';
import type { WalterS3File } from '$walter/types';

export class WalterS3FileWrapper {
    handles: WalterS3FileHandle[] = [];

    constructor(public fetchImpl: typeof fetch) {}

    // Always register for the next to last position; the last is always the stack.
    register(name: string, S3URL: string) {
        const handle = new WalterS3FileHandle(name, S3URL, this.fetchImpl);
        this.handles.splice(this.handles.length - 1, 0, handle);
    }

    registerStack() {
        this.register('Ablagestapel', S3URL.stack);
    }
}

export class WalterS3FileHandle {
    files: Promise<WalterS3File[]>;

    constructor(
        public name: string,
        public S3URL: string,
        public fetchImpl: typeof fetch
    ) {
        this.files = walter_s3_get_files(this.S3URL, this.fetchImpl);
    }

    async removeFile(file: WalterS3File): Promise<WalterS3File[]> {
        if (file.Key.includes(this.S3URL)) {
            const these_files = await this.files;
            this.files = Promise.resolve(
                these_files.filter((e) => e.FileName !== file.FileName)
            );
        }

        return this.files;
    }

    async addFile(file: WalterS3File): Promise<WalterS3File[]> {
        const files = await this.files;

        if (!files) {
            return [];
        }
        // Do not overwrite if the file already exists
        if (files.some((e) => e.FileName === file.FileName)) {
            return this.files;
        }

        this.files = Promise.resolve([...files, file]);

        return this.files;
    }
}

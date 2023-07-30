import { walter_s3_get_files } from '$walter/services/s3';
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
        this.register('Ablagestapel', 'stack');
    }

    // target might be either an index of a S3URL that has to match the handles S3URL
    async addFile(file: WalterS3File, target: string | number) {
        const index =
            typeof target === 'number'
                ? target
                : this.handles.findIndex((e) => e.S3URL === target);

        const files = await this.handles[index].files;

        if (!files) {
            return;
        }
        // Do not overwrite if the file already exists
        if (files.some((e) => e.FileName === file.FileName)) {
            return;
        }

        this.handles[index].files = Promise.resolve([...files, file]);
    }

    async removeFile(file: WalterS3File) {
        for (const handle of this.handles) {
            if (file.Key.includes(handle.S3URL)) {
                const files = await handle.files;
                handle.files = Promise.resolve(
                    files.filter((e) => e.FileName !== file.FileName)
                );
            }
        }
    }
}

class WalterS3FileHandle {
    files: Promise<WalterS3File[]>;

    constructor(
        public name: string,
        public S3URL: string,
        public fetchImpl: typeof fetch
    ) {
        this.files = walter_s3_get_files(this.S3URL, this.fetchImpl);
    }
}

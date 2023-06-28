import { walter_s3_get_files } from '$walter/services/s3';
import type { WalterS3File } from '$walter/types';

export class WalterS3FileWrapper {
    handles: WalterS3FileHandle[] = [];

    constructor(public fetchImpl: typeof fetch) {
        this.register('Ablagestapel', 'stack');
    }

    // Always register for the next to last position; the last is always the stack.
    register(name: string, S3URL: string) {
        const handle = new WalterS3FileHandle(name, S3URL, this.fetchImpl);
        this.handles.splice(this.handles.length - 1, 0, handle);
    }

    addFile(file: WalterS3File, target: string) {
        for (const handle of this.handles) {
            console.log('Check for add:', handle.S3URL, target);
            if (handle.S3URL == target) {
                console.log('Match for add.');
                handle.files.then((e) => e.push(file));
            }
        }
    }

    removeFile(file: WalterS3File) {
        for (const handle of this.handles) {
            console.log('Check for remove: ', file.Key, handle.S3URL);
            if (file.Key.includes(handle.S3URL)) {
                console.log('Match for remove.');
                handle.files.then((e) => e.splice(e.indexOf(file), 1));
            }
        }
    }
}

class WalterS3FileHandle {
    files: Promise<WalterS3File[]>;

    constructor(
        public name: string,
        public S3URL: string,
        fetchImpl: typeof fetch
    ) {
        this.files = walter_s3_get_files(this.S3URL, fetchImpl);
    }
}

import { walter_s3_get_files } from '$walter/services/s3';
import type { WalterS3File } from '$walter/types';

export class WalterS3FileWrapper {
    handles: WalterS3FileHandle[] = [];

    constructor(public fetchImpl: typeof fetch) {}

    register(name: string, S3URL: string) {
        const wrapper = new WalterS3FileHandle(name, S3URL, this.fetchImpl);
        this.handles.push(wrapper);
    }

    addFile(file: WalterS3File, target: string) {
        for (const wrapper of this.handles) {
            if (wrapper.S3URL == target) {
                wrapper.files.then((e) => e.push(file));
            }
        }
    }

    removeFile(file: WalterS3File, target: string) {
        for (const wrapper of this.handles) {
            if (wrapper.S3URL == target) {
                wrapper.files.then((e) => e.splice(e.indexOf(file), 1));
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

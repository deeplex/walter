import { walter_get_files } from '$walter/services/files';
import type { WalterFile } from '$walter/types';

export class WalterFileHandle {
    files: Promise<WalterFile[]>;

    constructor(
        public name: string,
        public fileURL: string,
        public fetchImpl: typeof fetch
    ) {
        this.files = walter_get_files(this.fileURL, this.fetchImpl);
    }

    // target might be either an index of a fileURL that has to match the handles fileURL
    async addFile(file: WalterFile): Promise<WalterFile[]> {
        const files = await this.files;

        if (!files) {
            return [];
        }
        // Do not overwrite if the file already exists
        if (files.some((e) => e.fileName === file.fileName)) {
            return files;
        }

        this.files = Promise.resolve([...files, file]);

        return this.files;
    }

    async removeFile(file: WalterFile) {
        const files = await this.files;
        this.files = Promise.resolve(
            files.filter((e) => e.fileName !== file.fileName)
        );

        return this.files;
    }
}

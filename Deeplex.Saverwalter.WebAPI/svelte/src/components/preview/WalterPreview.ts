import { download_file_blob } from '$walter/services/files';
import type { WalterFile } from '$walter/types';

export function download(file: WalterFile) {
    if (file.blob) {
        download_file_blob(file.blob, file.fileName);
    }
}

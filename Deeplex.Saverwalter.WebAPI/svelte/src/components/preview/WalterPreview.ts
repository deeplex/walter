import { WalterToastContent } from '$walter/lib';
import type { WalterS3FileHandle } from '$walter/lib/WalterS3FileWrapper';
import { download_file_blob, walter_s3_delete } from '$walter/services/s3';
import { openModal } from '$walter/store';
import type { WalterS3File } from '$walter/types';

export function download(file: WalterS3File) {
    if (file.Blob) {
        download_file_blob(file.Blob, file.FileName);
    }
}

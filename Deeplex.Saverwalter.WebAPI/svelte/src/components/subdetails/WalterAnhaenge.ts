import type { WalterS3FileWrapper } from '$walter/lib';
import type { WalterS3FileHandle } from '$walter/lib/WalterS3FileWrapper';
import {
    create_walter_s3_file_from_file,
    walter_s3_post
} from '$walter/services/s3';
import { openModal } from '$walter/store';
import type { WalterS3File } from '$walter/types';

function upload_finished(
    file: File,
    handle: WalterS3FileHandle
): Promise<WalterS3File[]> {
    return handle.addFile(
        create_walter_s3_file_from_file(file, `${handle.S3URL}/${file.name}`)
    );
}

function post_s3_file(
    file: File,
    handle: WalterS3FileHandle,
    fetchImpl: typeof fetch
): Promise<WalterS3File[]> {
    return walter_s3_post(file, handle.S3URL, fetchImpl).then(() =>
        upload_finished(file, handle)
    );
}

export async function upload_new_files(
    handle: WalterS3FileHandle,
    newFiles: File[],
    fetchImpl: typeof fetch
) {
    let files = await handle.files;

    for (const file of newFiles) {
        {
            if (files.map((e) => e.FileName).includes(file.name)) {
                const content = `Eine Datei mit dem Namen ${file.name} existiert bereits in dieser Ablage. Bist du sicher, dass diese Datei hochgeladen werden soll?`;
                openModal({
                    modalHeading: `Datei existiert bereits`,
                    content,
                    primaryButtonText: 'Überschreiben',
                    submit: () => post_s3_file(file, handle, fetchImpl)
                });
            } else {
                files = await post_s3_file(file, handle, fetchImpl);
            }
        }
    }

    return files;
}

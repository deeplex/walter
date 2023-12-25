import type { WalterFileHandle, WalterFileWrapper } from '$walter/lib';
import { walter_file_post } from '$walter/services/files';
import { openModal } from '$walter/store';
import { WalterFile } from '$walter/lib/WalterFile';

function upload_finished(
    handle: WalterFileHandle,
    file: File
): Promise<WalterFile[]> {
    return handle.addFile(
        new WalterFile(
            file.name,
            `${handle.fileURL}/${file.name}`,
            file.lastModified,
            file.size,
            file,
            file.type
        )
    );
}

async function post_file(
    handle: WalterFileHandle,
    file: File,
    fetchImpl: typeof fetch
): Promise<WalterFile[]> {
    return walter_file_post(file, handle.fileURL, fetchImpl).then(() =>
        upload_finished(handle, file)
    );
}

export async function upload_new_files(
    handle: WalterFileHandle,
    newFiles: File[],
    fetchImpl: typeof fetch
) {
    let files = await handle.files;

    for (const file of newFiles) {
        {
            if (files.map((e) => e.fileName).includes(file.name)) {
                const content = `Eine Datei mit dem Namen ${file.name} existiert bereits in dieser Ablage. Bist du sicher, dass diese Datei hochgeladen werden soll?`;
                openModal({
                    modalHeading: `Datei existiert bereits`,
                    content,
                    primaryButtonText: 'Überschreiben',
                    submit: () => post_file(handle, file, fetchImpl)
                });
            } else {
                files = await post_file(handle, file, fetchImpl);
            }
        }
    }

    return files;
}

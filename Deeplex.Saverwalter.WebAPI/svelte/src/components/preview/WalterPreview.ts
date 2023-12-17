import { WalterS3FileWrapper, WalterToastContent } from '$walter/lib';
import { download_file_blob, walter_s3_delete } from '$walter/services/s3';
import { openModal } from '$walter/store';
import type { WalterS3File } from '$walter/types';

export function download(file: WalterS3File) {
    if (file.Blob) {
        download_file_blob(file.Blob, file.FileName);
    }
}

export function remove(file: WalterS3File, fileWrapper: WalterS3FileWrapper) {
    const content = `Bist du sicher, dass du ${file.FileName} löschen möchtest?`;

    const deleteToast = new WalterToastContent(
        'Löschen erfolgreich',
        'Löschen fehlgeschlagen',
        () => `${file.FileName} erfolgreich gelöscht.`,
        () => ''
    );

    openModal({
        modalHeading: 'Löschen',
        content,
        danger: true,
        primaryButtonText: 'Löschen',
        submit: () =>
            walter_s3_delete(file, deleteToast).then((e) => {
                if (e.status === 200) {
                    fileWrapper.removeFile(file);
                }
            })
    });
}

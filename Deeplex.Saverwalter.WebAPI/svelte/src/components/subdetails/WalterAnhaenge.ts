import type { WalterS3FileWrapper } from '$walter/lib';
import {
    create_walter_s3_file_from_file,
    walter_s3_post
} from '$walter/services/s3';
import { openModal } from '$walter/store';

function upload_finished(fileWrapper: WalterS3FileWrapper, file: File) {
    fileWrapper.addFile(
        create_walter_s3_file_from_file(file, fileWrapper.handles[0].S3URL),
        0
    );
}

function post_s3_file(fileWrapper: WalterS3FileWrapper, file: File) {
    walter_s3_post(
        file,
        fileWrapper.handles[0].S3URL,
        fileWrapper.fetchImpl
    ).then(() => upload_finished(fileWrapper, file));
}

export async function upload_new_files(
    fileWrapper: WalterS3FileWrapper,
    newFiles: File[]
) {
    const files = await fileWrapper.handles[0].files;
    for (const file of newFiles) {
        {
            if (files.map((e) => e.FileName).includes(file.name)) {
                const content = `Eine Datei mit dem Namen ${file.name} existiert bereits in dieser Ablage. Bist du sicher, dass diese Datei hochgeladen werden soll?`;
                openModal({
                    modalHeading: `Datei existiert bereits`,
                    content,
                    primaryButtonText: 'Überschreiben',
                    submit: () => post_s3_file(fileWrapper, file)
                });
            } else {
                post_s3_file(fileWrapper, file);
            }
        }
    }
}

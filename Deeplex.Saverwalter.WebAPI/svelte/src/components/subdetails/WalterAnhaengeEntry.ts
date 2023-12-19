import { walter_file_get } from '$walter/services/files';
import { walter_goto } from '$walter/services/utils';
import { WalterFile } from '$walter/lib/WalterFile';

export async function get_file_and_update_url(file: WalterFile) {
    const searchParams = new URLSearchParams({ file: `${file.fileName}` });
    walter_goto(`?${searchParams.toString()}`, { noScroll: true });

    return walter_file_get(file.key).then((e: unknown) => {
        const blob = e as Blob;
        return new WalterFile(
            file.fileName,
            file.key,
            file.lastModified,
            blob.size,
            new File([blob], file.fileName, { type: blob.type }),
            blob.type
        );
    });
}

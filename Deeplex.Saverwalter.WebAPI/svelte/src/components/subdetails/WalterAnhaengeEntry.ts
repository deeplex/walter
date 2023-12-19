import {
    create_walter_s3_file_from_file,
    walter_s3_get
} from '$walter/services/s3';
import { walter_goto } from '$walter/services/utils';
import type { WalterS3File } from '$walter/types';

export async function get_file_and_update_url(file: WalterS3File) {
    const searchParams = new URLSearchParams({ file: `${file.FileName}` });
    walter_goto(`?${searchParams.toString()}`, { noScroll: true });

    return walter_s3_get(file.Key).then((e: unknown) => {
        const new_file = new File([e as Blob], file.FileName, {
            type: (e as Blob).type
        });
        return create_walter_s3_file_from_file(new_file, file.Key);
    });
}

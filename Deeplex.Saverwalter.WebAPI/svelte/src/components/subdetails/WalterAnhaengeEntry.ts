import {
    create_walter_s3_file_from_file,
    walter_s3_get
} from '$walter/services/s3';
import type { WalterS3File } from '$walter/types';

export async function get_file(file: WalterS3File) {
    return walter_s3_get(file.Key).then((e: Blob) => {
        const new_file = new File([e], file.FileName, { type: e.type });
        return create_walter_s3_file_from_file(new_file, file.Key);
    });
}

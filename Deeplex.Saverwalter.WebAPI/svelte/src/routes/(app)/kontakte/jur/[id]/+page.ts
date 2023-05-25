import { WalterJuristischePersonEntry } from '$walter/lib';
import { walter_selection } from '$walter/services/requests';
import { walter_s3_get_files } from '$walter/services/s3';
import type { WalterS3File } from '$walter/types';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/kontakte/jur/${params.id}`;
    const S3URL = `kontakte/jur/${params.id}`;

    return {
        fetch,
        id: params.id,
        S3URL: S3URL,
        apiURL: apiURL,
        a: WalterJuristischePersonEntry.GetOne<WalterJuristischePersonEntry>(
            `jur/${params.id}`,
            fetch
        ),

        kontakte: walter_selection.kontakte(fetch),
        wohnungen: walter_selection.wohnungen(fetch),
        juristischePersonen: walter_selection.juristischePersonen(fetch),

        anhaenge: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
    };
};

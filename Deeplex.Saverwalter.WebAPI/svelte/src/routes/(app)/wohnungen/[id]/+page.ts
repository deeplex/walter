import { WalterWohnungEntry } from '$walter/lib';
import { S3URL } from '$walter/services/s3';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterWohnungEntry.ApiURL}/${params.id}`;
    const s3URL = S3URL.wohnung(params.id);

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        S3URL: s3URL,
        entry: WalterWohnungEntry.GetOne<WalterWohnungEntry>(params.id, fetch)
    };
};

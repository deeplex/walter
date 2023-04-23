import { WalterZaehlerEntry } from '$WalterLib';
import { walter_selection } from '$WalterServices/requests';
import { walter_s3_get_files } from '$WalterServices/s3';
import type { WalterS3File } from '$WalterTypes';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
  const apiURL = `/api/zaehler/${params.id}`;
  const S3URL = `zaehler/${params.id}`;

  return {
    fetch,
    id: params.id,
    apiURL: apiURL,
    S3URL: S3URL,
    a: WalterZaehlerEntry.GetOne<WalterZaehlerEntry>(params.id, fetch),

    wohnungen: walter_selection.wohnungen(fetch),
    zaehler: walter_selection.zaehler(fetch),
    zaehlertypen: walter_selection.zaehlertypen(fetch),

    anhaenge: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
  };
};

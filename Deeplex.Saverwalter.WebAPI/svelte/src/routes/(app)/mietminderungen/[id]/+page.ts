import { WalterMietminderungEntry } from '$WalterLib';
import { walter_s3_get_files } from '$WalterServices/s3';
import type { WalterS3File } from '$WalterTypes';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
  const apiURL = `/api/mietminderungen/${params.id}`;
  const S3URL = `mieten/${params.id}`;

  return {
    fetch,
    id: params.id,
    apiURL: apiURL,
    S3URL: S3URL,
    a: WalterMietminderungEntry.GetOne<WalterMietminderungEntry>(
      params.id,
      fetch
    ),
    anhaenge: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
  };
};

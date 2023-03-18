import { walter_get, walter_selection } from "$WalterServices/requests";
import { walter_s3_get_files } from "$WalterServices/s3";
import type { WalterBetriebskostenabrechnungEntry, WalterS3File, WalterVertragEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const vertragURL = `/api/vertraege/${params.id}`;
    const abrechnungURL = `/api/betriebskostenabrechnung/${params.id}/${params.year}`;
    const S3URL = `vertraege/${params.id}`;

    return {
        id: params.id,
        year: params.year,
        vertragURL: vertragURL,
        abrechnungURL: abrechnungURL,
        S3URL: S3URL,
        kontakte: walter_selection.kontakte(fetch),
        wohnungen: walter_selection.wohnungen(fetch),
        vertrag: walter_get(vertragURL, fetch) as Promise<WalterVertragEntry>,
        abrechnung: walter_get(abrechnungURL, fetch) as Promise<WalterBetriebskostenabrechnungEntry>,

        anhaenge: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
    }
}
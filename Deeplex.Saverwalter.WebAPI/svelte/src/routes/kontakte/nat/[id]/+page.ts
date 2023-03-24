import { WalterNatuerlichePersonEntry } from "$WalterLib";
import { walter_selection } from "$WalterServices/requests";
import { walter_s3_get_files } from "$WalterServices/s3";
import type { WalterS3File } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
        const apiURL = `/api/kontakte/nat/${params.id}`;
        const S3URL = `kontakte/nat/${params.id}`

        return {
                id: params.id,
                apiURL: apiURL,
                S3URL: S3URL,
                a: WalterNatuerlichePersonEntry.GetOne<WalterNatuerlichePersonEntry>(`nat/${params.id}`, fetch),

                kontakte: walter_selection.kontakte(fetch),
                wohnungen: walter_selection.wohnungen(fetch),
                juristischePersonen: walter_selection.juristischePersonen(fetch),

                anhaenge: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
        }
}
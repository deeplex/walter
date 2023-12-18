import { addToast, openModal } from '$walter/store';
import type { WalterS3File } from '$walter/types';
import * as parser from 'fast-xml-parser';
import { walter_delete, walter_fetch } from './requests';
import type { WalterToastContent } from '../lib/WalterToastContent';
import {
    WalterAdresseEntry,
    WalterBetriebskostenrechnungEntry,
    WalterErhaltungsaufwendungEntry,
    WalterKontaktEntry,
    WalterMieteEntry,
    WalterMietminderungEntry,
    WalterUmlageEntry,
    WalterUmlagetypEntry,
    WalterVertragVersionEntry,
    WalterWohnungEntry,
    WalterZaehlerEntry,
    WalterZaehlerstandEntry
} from '$walter/lib';

export const S3URL = {
    adresse: (id: string) => `${WalterAdresseEntry.ApiURL}/${id}/files`,
    betriebskostenrechnung: (id: string) =>
        `${WalterBetriebskostenrechnungEntry.ApiURL}/${id}/files`,
    erhaltungsaufwendung: (id: string) =>
        `${WalterErhaltungsaufwendungEntry.ApiURL}/${id}/files`,
    miete: (id: string) => `${WalterMieteEntry.ApiURL}/${id}/files`,
    mietminderung: (id: string) =>
        `${WalterMietminderungEntry.ApiURL}/${id}/files`,
    kontakt: (id: string) => `${WalterKontaktEntry.ApiURL}/${id}/files`,
    umlage: (id: string) => `${WalterUmlageEntry.ApiURL}/${id}/files`,
    umlagetyp: (id: string) => `${WalterUmlagetypEntry.ApiURL}/${id}/files`,
    vertrag: (id: string) => `${WalterVertragVersionEntry.ApiURL}/${id}/files`,
    vertragversion: (id: string) =>
        `${WalterVertragVersionEntry.ApiURL}/${id}/files`,
    wohnung: (id: string) => `${WalterWohnungEntry.ApiURL}/${id}/files`,
    zaehler: (id: string) => `${WalterZaehlerEntry.ApiURL}/${id}/files`,
    zaehlerstand: (id: string) =>
        `${WalterZaehlerstandEntry.ApiURL}/${id}/files`,
    stack: `/api/user/files`
};

type XMLResult = {
    ListBucketResult?: { Contents?: WalterS3File | WalterS3File[] };
};

export const walter_s3_post = (
    file: File,
    S3URL: string,
    fetchImpl: typeof fetch,
    toast?: WalterToastContent
) =>
    walter_fetch(fetchImpl, `${S3URL}/${file.name}`, {
        method: 'PUT',
        headers: {
            // Ignored, due to header being replaced in walter_fetch
            'Content-Type': `${file.type}`
        },
        body: file
    }).then((e) => finish_s3_post(e, toast));

export async function finish_s3_post(e: Response, toast?: WalterToastContent) {
    toast && addToast(toast, e.status === 200);
    return e;
}

export const walter_s3_get = (S3URL: string): Promise<unknown> =>
    walter_fetch(fetch, S3URL, { method: 'GET' }).then((e) => e.blob());

export async function walter_s3_delete(
    file: WalterS3File,
    toast?: WalterToastContent
) {
    return walter_delete(file.Key, toast);
}

export function download_file_blob(blob: Blob, fileName: string) {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

export async function walter_s3_get_files(
    S3URL: string,
    fetchImpl: typeof fetch
): Promise<WalterS3File[]> {
    const requestInit = { method: 'GET' };

    const fetched = await walter_fetch(fetchImpl, S3URL, requestInit);
    const reader = fetched.body?.getReader();

    if (!reader) {
        return []; // throw?
    }

    let done = false;
    let result: Uint8Array | undefined = undefined;

    while (!done) {
        const { done: isDone, value } = await reader.read();
        done = isDone;

        if (value && value.length > 0) {
            if (!result) {
                result = value;
            } else {
                const concatenatedResult: Uint8Array = new Uint8Array(
                    result.length + value.length
                );
                concatenatedResult.set(result, 0);
                concatenatedResult.set(value, result.length);
                result = concatenatedResult;
            }
        }
    }

    const parsed = parse_stream_into_walter_s3_files(result, S3URL);

    return parsed;
}

function hideTrash(contents: WalterS3File[]): WalterS3File[] {
    return contents.filter((c) => {
        const splits = c.Key.split('/');
        const trash = splits[splits.length - 2];
        return trash !== 'trash';
    });
}

function parse_stream_into_walter_s3_files(
    uint8array: Uint8Array | undefined,
    S3URL: string
) {
    if (!uint8array) {
        return [];
    }
    const result: XMLResult = new parser.XMLParser().parse(
        new TextDecoder().decode(uint8array)
    );

    const Contents = result?.ListBucketResult?.Contents;

    if (!Contents) {
        return [];
    } else if (Array.isArray(Contents)) {
        const filteredContents = hideTrash(Contents);
        return (
            filteredContents.map((c) =>
                create_walter_s3_file_from_xml_parse(c, S3URL)
            ) || []
        );
    } else {
        return [create_walter_s3_file_from_xml_parse(Contents, S3URL)];
    }
}

export function create_walter_s3_file_from_file(
    file: File,
    S3URL: string
): WalterS3File {
    return {
        FileName: file.name,
        Key: `${S3URL}`,
        LastModified: file.lastModified,
        Type: file.type,
        Size: file.size,
        Blob: file
    };
}

function create_walter_s3_file_from_xml_parse(
    e: WalterS3File,
    S3URL: string
): WalterS3File {
    const FileName = e.Key.split('/').pop();
    const Key = e.Key;

    if (FileName && Key) {
        const FileName = e.Key.split('/').pop()!;
        return {
            FileName,
            Key: `${S3URL}/${FileName}`,
            LastModified: e.LastModified,
            Size: e.Size
        };
    } else {
        return {
            FileName: `Fehler (${FileName})`,
            Key:
                e.Key ||
                `Fehler (${Key}) - ${Math.floor(
                    100000 + Math.random() * 900000
                )}`,
            LastModified: e.LastModified || new Date().getTime(),
            Size: e.Size || 0
        };
    }
}

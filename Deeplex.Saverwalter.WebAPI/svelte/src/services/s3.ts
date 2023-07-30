import { addToast, openModal } from '$walter/store';
import type { WalterS3File } from '$walter/types';
import * as parser from 'fast-xml-parser';
import { walter_delete, walter_fetch } from './requests';
import type { WalterToastContent } from '../lib/WalterToastContent';

const baseURL = '/api/files';
type XMLResult = {
    ListBucketResult?: { Contents?: WalterS3File | WalterS3File[] };
};

export const walter_s3_post = (
    file: File,
    path: string,
    fetchImpl: typeof fetch,
    toast?: WalterToastContent
) =>
    walter_fetch(fetchImpl, `${baseURL}/${path}/${file.name}`, {
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

export const walter_s3_get = (S3URL: string): Promise<any> =>
    walter_fetch(fetch, `${baseURL}/${S3URL}`, { method: 'GET' }).then((e) =>
        e.blob()
    );

async function walter_s3_remove_folder_content_impl(
    files: WalterS3File[], toast?: WalterToastContent)
{
    let everything_okay = true;

    for (const file of files)
    {
        const file_path = `${baseURL}/${file.Key}`
        const result = await walter_delete(file_path);
        if (!result.ok)
        {
            everything_okay = false;
        }
    }

    toast && addToast(toast, everything_okay);

    if (everything_okay)
    {
        files = [];
    }

    return everything_okay;

}

export async function walter_s3_remove_folder_content(
    files: WalterS3File[],
    toast?:WalterToastContent)
{
    const file_s = files.length === 1 ? "eine Datei" : `${files.length} Dateien`;
    const content = `Bist du sicher, dass du ${file_s} endgültig löschen willst? Diese Änderung kann nicht rückgängig gemacht werden.`

    openModal({
        modalHeading: 'Löschen',
        content,
        danger: true,
        primaryButtonText: 'Löschen',
        submit: () =>
            walter_s3_remove_folder_content_impl(files, toast)
    });
}

export async function walter_s3_delete(
    file: WalterS3File,
    fetchImpl: typeof fetch,
    toast?: WalterToastContent,
) {
    const file_path = `${baseURL}/${file.Key}`;

    if (file.Blob)
    {
        const response = walter_s3_post(
            new File([file.Blob], file.FileName),
            "trash",
            fetchImpl,
        );

        if ((await response).ok)
        {
            return walter_delete(file_path, toast)
        }
        else
        {
            return response;
        }
    }
    else
    {
        return walter_delete(file_path, toast);
    }

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
    S3prefixURL: string,
    fetchImpl: typeof fetch
): Promise<WalterS3File[]> {
    const url = `${baseURL}?prefix=${S3prefixURL}`;
    const requestInit = { method: 'GET' };

    const fetched = await walter_fetch(fetchImpl, url, requestInit);
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

    const parsed = parse_stream_into_walter_s3_files(result);

    return parsed;
}

function parse_stream_into_walter_s3_files(uint8array: Uint8Array | undefined) {
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
        return Contents.map(create_walter_s3_file_from_xml_parse) || [];
    } else {
        return [create_walter_s3_file_from_xml_parse(Contents)];
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

function create_walter_s3_file_from_xml_parse(e: WalterS3File): WalterS3File {
    const FileName = e.Key.split('/').pop();
    const Key = e.Key;

    if (FileName && Key) {
        return {
            FileName: e.Key.split('/').pop()!,
            Key: e.Key,
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

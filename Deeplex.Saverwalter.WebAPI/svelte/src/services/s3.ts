import type { WalterS3File } from '$WalterTypes';
import * as parser from 'fast-xml-parser';
import { walter_delete } from './requests';

const baseURL = "http://192.168.178.61:9002/saverwalter";
type fetchType = (input: RequestInfo | URL, init?: RequestInit | undefined) => Promise<Response>

export const walter_s3_post = (file: File, path: string) => fetch(
    `${baseURL}/${path}/${file.name}`,
    {
        method: 'PUT',
        headers: {
            'Content-Type': `${file.type}`,
        },
        body: file
    }
);

export const walter_s3_get = (S3URL: string) => fetch(
    `${baseURL}/${S3URL}`, {
    method: 'GET',
    headers: {}
}).then(e => e.blob());

export const walter_s3_delete = (file: WalterS3File, nav: string) =>
    walter_delete(`${baseURL}/${file.Key}`, file.FileName, nav);

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

type Content = {
    Key: string;
    Size: string;
    LastModified: string;
}

export function walter_s3_get_files(S3prefixURL: string, f: fetchType) {
    return f(`${baseURL}?prefix=${S3prefixURL}`, {
        method: 'GET',
        headers: {}
    })
        .then((e) => e.body?.getReader().read())
        .then((e) => {
            if (!e) {
                return [];
            }
            const result = new parser.XMLParser().parse(new TextDecoder().decode(e.value));

            const Contents = result?.ListBucketResult?.Contents;
            if (!Contents) {
                return [];
            }
            else if (Array.isArray(Contents)) {
                return Contents.map((e: Content) => ({
                    FileName: e.Key.split("/").pop(),
                    Key: e.Key,
                    LastModified: e.LastModified,
                    Size: e.Size
                })) || [];
            }
            else {
                return [{
                    FileName: Contents.Key.split("/").pop(),
                    Key: Contents.Key,
                    LastModified: Contents.LastModified,
                    Size: Contents.Size
                }];
            }
        });
}

type XMLResult = { ListBucketResult: { Contents: { Key: string } | { Key: string }[] } }

function getFileNamesFromCommonPrefix(result: XMLResult) {
    const Contents = result?.ListBucketResult?.Contents;
    if (!Contents) {
        return [];
    }
    else if (Array.isArray(Contents)) {
        // Key is the full filepath, therefore split("/").pop() to get the filename
        return Contents.map((e) => e.Key.split("/").pop() as string) || [];
    }
    else {
        return [Contents.Key];
    }
}

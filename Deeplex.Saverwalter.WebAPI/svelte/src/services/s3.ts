import type { WalterS3File } from '$WalterTypes';
import * as parser from 'fast-xml-parser';

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

export const walter_s3_get = (url: string) => fetch(
    `${baseURL}/${url}`, {
    method: 'GET',
    headers: {}
}).then(e => e.blob());

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

export function walter_s3_get_files(url: string, f: fetchType) {
    return f(`${baseURL}?prefix=${url}`, {
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

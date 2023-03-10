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

export const walter_s3_get = (url: string) => fetch(
    `${baseURL}/${url}`, {
    method: 'GET',
    headers: {}
}).then(e => e.blob());

export const walter_s3_delete = (url: string, fileName: string) =>
    walter_delete(`${baseURL}/${url}`, fileName);

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
    LastModified: string
}

export function get_files_with_common_prefix(url: string, f: fetchType) {
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
                // Key is the full filepath, therefore split("/").pop() to get the filename
                return Contents.map((e: Content) => e.Key.split("/").pop()) || [];
            }
            else {
                return [Contents.Key];
            }
        });
}

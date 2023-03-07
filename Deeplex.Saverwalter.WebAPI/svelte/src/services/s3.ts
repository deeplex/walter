import type { IWalterAnhang, WalterAnhangEntry } from "$WalterTypes";
import { walter_post } from "./requests";

const headers = {
    'Content-Type': 'application/octet-stream'
};

export const walter_s3_post = (file: File, reference: IWalterAnhang) => {
    const new_file: Partial<WalterAnhangEntry> = {
        fileName: file.name,
        creationTime: new Date(file.lastModified)
    };

    return walter_post(`/api/anhaenge`, new_file)
        .then((e: WalterAnhangEntry) => {
            return upload_file(e.id, file).then(() => e);
        });
}

function upload_file(id: string, file: File) {
    return fetch(
        `http://192.168.178.61:9002/saverwalter/${id}`,
        {
            method: 'PUT',
            headers: {
                'Content-Type': `${file.type}`,
            },
            body: file
        }
    )
}

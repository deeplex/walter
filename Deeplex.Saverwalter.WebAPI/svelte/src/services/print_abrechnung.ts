import { walter_s3_post } from "./s3";

const headers = {
    'Content-Type': 'application/octet-stream'
};

export function print_abrechnung(id: string, jahr: number, fileNameBase: string) {
    const apiURL = `/api/betriebskostenabrechnung/${id}/${jahr}`;
    const fileName = `Abrechnung ${jahr} - ${fileNameBase}.docx`;
    return fetch(apiURL, {
        method: 'GET',
        headers
    })
        .then((e) => e.blob())
        .then((e) =>
            walter_s3_post(
                new File([e], fileName),
                `vertraege/${id}`
            )
        ).then(() => fileName);
}
import { walter_s3_post } from '$walter/services/s3';

const headers = {
    'Content-Type': 'application/octet-stream'
};

export function post_files(id: number, jahr: number, fetchImpl: typeof fetch) {
    const apiURL = `/api/betriebskostenabrechnung/${id}/${jahr}`;
    fetch(apiURL, {
        method: 'GET',
        headers
    })
        .then((e) => e.blob())
        .then((e) =>
            walter_s3_post(
                new File([e], `Abrechnung ${jahr}.docx`),
                `vertraege/${id}`,
                fetchImpl
            )
        );
}

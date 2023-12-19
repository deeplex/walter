import { WalterVertragEntry } from '$walter/lib';
import { walter_file_post } from '$walter/services/files';

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
            walter_file_post(
                new File([e], `Abrechnung ${jahr}.docx`),
                `${WalterVertragEntry.ApiURL}/${id}/files`,
                fetchImpl
            )
        );
}

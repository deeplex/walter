const headers = {
    'Content-Type': 'application/octet-stream'
};

export function create_abrechnung(id: string, jahr: number, fileNameBase: string) {
    const apiURL = `/api/betriebskostenabrechnung/${id}/${jahr}`;
    const fileName = `Abrechnung ${jahr} - ${fileNameBase}.docx`;
    return fetch(apiURL, {
        method: 'GET',
        headers
    })
        .then((e) => e.blob())
        .then(e => new File([e], fileName));
}
export function convertDate(text: string | undefined): string | undefined {
    if (text) {
        return new Date(text).toLocaleDateString("de-DE");
    }
    else {
        return undefined;
    }
}

export const request_options = {
    method: 'GET',
    headers: {
        'Content-Type': 'text/json'
    }
};
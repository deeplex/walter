import { WalterToastContent } from '$walter/lib';
import { walter_post } from '$walter/services/requests';
import { addToast } from '$walter/store';

export async function handle_save(
    url: string,
    body: unknown,
    toastTitle: string
) {
    const PostToast = new WalterToastContent(
        'Speichern erfolgreich',
        'Speichern fehlgeschlagen',
        () => `${toastTitle} erfolgreich gespeichert.`,
        (a: unknown) => `Konnte ${a} nicht speichern.`
    );

    const response = await walter_post(url, body);
    const parsed = await response.json();
    addToast(PostToast, response.status === 200, parsed);

    return parsed;
}

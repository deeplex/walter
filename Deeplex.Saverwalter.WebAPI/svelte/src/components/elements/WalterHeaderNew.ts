import { WalterToastContent } from '$walter/lib';
import { walter_post } from '$walter/services/requests';
import { walter_goto } from '$walter/services/utils';
import { addToast, changeTracker } from '$walter/store';

export async function handle_save(apiURL: string, entry: unknown) {
    const SaveToast = new WalterToastContent(
        'Speichern erfolgreich',
        'Speichern fehlgeschlagen',
        (a: unknown) => a as string,
        (a: unknown) =>
            `Speichern fehlgeschlagen.
            Folgende Einträge sind erforderlich:
            ${Object.keys((a as { errors: string }).errors)
                .map((e) => e.split('.').pop())
                .join(', \n')}`
    );

    const response = await walter_post(apiURL, entry);
    const parsed = await response.json();
    addToast(SaveToast, response.status === 200, parsed);

    if (parsed.id) {
        changeTracker.set(0);
        walter_goto(`${apiURL}/${parsed.id}`.replace('api/', ''));
    }
}

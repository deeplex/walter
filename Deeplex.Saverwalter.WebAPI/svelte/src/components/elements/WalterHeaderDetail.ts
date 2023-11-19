import { WalterToastContent } from '$walter/lib';
import { walter_delete, walter_put } from '$walter/services/requests';
import { changeTracker, openModal } from '$walter/store';

export async function handle_save(
    apiURL: string,
    entry: { lastModified: Date } & unknown,
    toastTitle: string
) {
    const PutToast = new WalterToastContent(
        'Speichern erfolgreich',
        'Speichern fehlgeschlagen',
        () => `${toastTitle} erfolgreich gespeichert.`,
        (a: unknown) =>
            `Folgende Einträge sind erforderlich:
            ${Object.keys((a as { errors: string }).errors)
                .map((e) => e.split('.').pop())
                .join(', \n')}`
    );

    const result = await walter_put(apiURL, entry, PutToast);
    if (result.ok) {
        entry.lastModified = new Date();
        changeTracker.set(-1);
        changeTracker.set(0);
    }
}

export function handle_delete(title: string, apiURL: string) {
    const content = `Bist du sicher, dass du ${title} löschen möchtest?
    Dieser Vorgang kann nicht rückgängig gemacht werden.`;

    const DeleteToast = new WalterToastContent(
        'Löschen erfolgreich',
        'Löschen fehlgeschlagen',
        () => `${title} erfolgreich gelöscht.`,
        () => ''
    );

    openModal({
        modalHeading: 'Löschen',
        content,
        danger: true,
        primaryButtonText: 'Löschen',
        submit: async () => {
            const result = await walter_delete(apiURL, DeleteToast);
            if (result.ok) {
                changeTracker.set(-1);
                changeTracker.set(0);
                history.back();
            }
        }
    });
}

import { WalterToastContent } from '$walter/lib';
import { walter_delete, walter_put } from '$walter/services/requests';
import { openModal } from '$walter/store';

export function handle_save(
    apiURL: string,
    entry: unknown,
    toastTitle: string
) {
    const PutToast = new WalterToastContent(
        'Speichern erfolgreich',
        'Speichern fehlgeschlagen',
        (_a: unknown) => `${toastTitle} erfolgreich gespeichert.`,
        (a: any) =>
            `Folgende Einträge sind erforderlich:
            ${Object.keys(a.errors)
                .map((e) => e.split('.').pop())
                .join(', \n')}`
    );

    walter_put(apiURL, entry, PutToast);
}

export function handle_delete(title: string, apiURL: string) {
    const content = `Bist du sicher, dass du ${title} löschen möchtest?
    Dieser Vorgang kann nicht rückgängig gemacht werden.`;

    const DeleteToast = new WalterToastContent(
        'Löschen erfolgreich',
        'Löschen fehlgeschlagen',
        (_a: unknown) => `${title} erfolgreich gelöscht.`,
        (_a: unknown) => ''
    );

    openModal({
        modalHeading: 'Löschen',
        content,
        danger: true,
        primaryButtonText: 'Löschen',
        submit: () =>
            walter_delete(apiURL, DeleteToast).then(() => history.back())
    });
}

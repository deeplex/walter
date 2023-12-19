import { WalterToastContent } from '$walter/lib';
import { walter_sign_out } from '$walter/services/auth';
import { fileURL, walter_get_files } from '$walter/services/files';
import { walter_goto } from '$walter/services/utils';
import { Checkmark, DocumentAttachment } from 'carbon-icons-svelte';

export async function checkStackTodo(fetchImpl: typeof fetch) {
    const files = await walter_get_files(fileURL.stack, fetchImpl);
    const todo = files.length > 0;

    if (todo) {
        return DocumentAttachment;
    } else {
        return Checkmark;
    }
}

export function logout() {
    const LogoutToast = new WalterToastContent(
        'Abmeldung erfolgreich',
        'Abmeldung fehlgeschlagen'
    );
    walter_sign_out(LogoutToast);
    walter_goto('/login');
}

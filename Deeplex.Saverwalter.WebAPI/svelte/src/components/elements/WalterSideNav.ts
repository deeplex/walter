import { walter_s3_get_files } from '$walter/services/s3';
import { Checkmark, DocumentAttachment } from 'carbon-icons-svelte';

export async function checkStackTodo(fetchImpl: typeof fetch) {
    const files = await walter_s3_get_files('stack', fetchImpl);
    const todo = files.length > 0;

    if (todo) {
        return DocumentAttachment;
    } else {
        return Checkmark;
    }
}

import type { WalterSelectionEntry } from "$walter/lib";

export function entriesToString(value: WalterSelectionEntry[] | undefined) {
    return value?.map(entry => entry.id).sort().join(";");
}
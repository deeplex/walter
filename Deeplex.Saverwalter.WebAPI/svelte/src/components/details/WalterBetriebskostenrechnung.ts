import type { WalterKontaktEntry, WalterSelectionEntry } from '$walter/lib';

export function shouldFilterItem(item: WalterSelectionEntry, value: string) {
    if (!value) return true;
    return item.text.toLowerCase().includes(value.toLowerCase());
}

export function removeSelf(
    entries: WalterSelectionEntry[],
    entry: Partial<WalterKontaktEntry>
) {
    return entries.filter((e) => e.id !== entry.id);
}

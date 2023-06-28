import type { WalterSelectionEntry } from '$walter/lib';

export function shouldFilterItem(item: WalterSelectionEntry, value: string) {
    if (!value) return true;
    return item.text.toLowerCase().includes(value.toLowerCase());
}

import type { WalterSelectionEntry } from '$walter/lib';

export function shouldFilterItem(item: WalterSelectionEntry, value: string) {
    if (!value) return true;

    const text = item.text.toLowerCase();
    const values = `${value}`
        .toLowerCase()
        .split(';')
        .map((e) => e.trim());
    return values.every((val) => text.includes(val));
}

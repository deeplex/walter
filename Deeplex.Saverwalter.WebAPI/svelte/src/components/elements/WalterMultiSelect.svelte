<script lang="ts">
    import type { WalterSelectionEntry } from '$walter/lib';
    import { MultiSelect, TextInputSkeleton } from 'carbon-components-svelte';

    export let value: WalterSelectionEntry[] | undefined;
    export let titleText: string;
    export let entries: Promise<WalterSelectionEntry[]>;
    export let disabled = false;

    function select(e: CustomEvent) {
        value = e.detail.selected;
    }

    // svelte things item should be MultiSelectItem but importing is squiggly.
    function filterItem(item: any, value: string) {
        if (!value) return true;

        const text = item.text.toLowerCase();
        const values = `${value}`
            .toLowerCase()
            .split(';')
            .map((e) => e.trim());
        return values.every((val) => text.includes(val)) as any; // svelte things this should be string...
    }

</script>

{#await entries}
    <TextInputSkeleton />
{:then resolvedEntries}
    {#await value}
        <TextInputSkeleton />
    {:then x}
        <MultiSelect
            {disabled}
            placeholder={value?.map((e) => e.text).join(', ')}
            selectedIds={x?.map((e) => e.id)}
            style="padding-right: 1rem"
            items={resolvedEntries}
            on:select={select}
            {titleText}
            filterable
            {filterItem}
        />
    {/await}
{/await}

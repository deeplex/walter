<script lang="ts">
    import { ComboBox, TextInputSkeleton } from 'carbon-components-svelte';

    import type { WalterSelectionEntry } from '$walter/lib';

    export let value: WalterSelectionEntry | undefined;
    export let titleText: string;
    export let a: WalterSelectionEntry[];

    function shouldFilterItem(item: WalterSelectionEntry, value: string) {
        if (!value) return true;
        return item.text.toLowerCase().includes(value.toLowerCase());
    }

    function select(e: CustomEvent) {
        value = e.detail.selectedItem;
    }
</script>

{#await a}
    <TextInputSkeleton />
{:then items}
    {#await value}
        <TextInputSkeleton />
    {:then x}
        <ComboBox
            selectedId={x?.id}
            on:select={select}
            style="padding-right: 1rem"
            {items}
            value={x?.text}
            {titleText}
            {shouldFilterItem}
        />
    {/await}
{/await}

<script lang="ts">
    import { ComboBox, TextInputSkeleton } from 'carbon-components-svelte';

    import type { WalterSelectionEntry } from '$walter/lib';
    import { shouldFilterItem } from './WalterComboBox';

    export let value: WalterSelectionEntry | undefined;
    export let titleText: string;
    export let entry: WalterSelectionEntry[];

    function select(e: CustomEvent) {
        value = e.detail.selectedItem;
    }
</script>

{#await entry}
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

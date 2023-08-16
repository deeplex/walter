<script lang="ts">
    import { ComboBox, TextInputSkeleton } from 'carbon-components-svelte';

    import type { WalterSelectionEntry } from '$walter/lib';
    import { shouldFilterItem } from './WalterComboBox';

    export let value: WalterSelectionEntry | undefined;
    export let titleText: string;
    export let entries: Promise<WalterSelectionEntry[]>;
    export let readonly = false;
    export let required = false;

    function select(e: CustomEvent) {
        value = e.detail.selectedItem;
    }

</script>

{#await entries}
    <TextInputSkeleton />
{:then resolvedEntries}
    {#await value}
        <TextInputSkeleton />
    {:then x}
        <ComboBox
            invalid={required && !value}
            invalidText={`${titleText} ist ein notwendiges Feld.`}
            disabled={readonly}
            selectedId={x?.id}
            on:select={select}
            style="padding-right: 1rem"
            items={resolvedEntries}
            value={x?.text}
            {titleText}
            {shouldFilterItem}
        />
    {/await}
{/await}

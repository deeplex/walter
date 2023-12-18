<script lang="ts">
    import { WalterKontaktEntry, WalterSelectionEntry } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';
    import { WalterComboBox, WalterKontakt } from '..';
    import WalterQuickAddButton from './WalterQuickAddButton.svelte';

    export let readonly: boolean = false;
    export let required: boolean = false;
    export let value: WalterSelectionEntry | undefined;
    export let fetchImpl: typeof fetch;
    export let title: string | undefined = undefined;

    let entries = walter_selection.kontakte(fetchImpl);
    function onSubmit() {
        entries = walter_selection.kontakte(fetchImpl);
    }

    let addEntry: Partial<WalterKontaktEntry> = {};
</script>

<div
    style="
    flex: 1 1 auto !important;
    display: flex !important;
    flex-wrap: wrap !important"
>
    <WalterComboBox
        {required}
        {readonly}
        bind:value
        titleText={title || 'Person'}
        {entries}
    />

    {#if !readonly}
        <WalterQuickAddButton
            title="Personen"
            bind:addEntry
            addUrl={WalterKontaktEntry.ApiURL}
            {onSubmit}
        >
            <WalterKontakt entry={addEntry} {fetchImpl} />
        </WalterQuickAddButton>
    {/if}
</div>

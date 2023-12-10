<script lang="ts">
    import {
        WalterUmlagetypEntry,
        type WalterSelectionEntry
    } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';
    import { WalterComboBox, WalterUmlagetyp } from '..';
    import WalterQuickAddButton from './WalterQuickAddButton.svelte';

    export let readonly: boolean = false;
    export let required: boolean = false;
    export let value: WalterSelectionEntry | undefined;
    export let fetchImpl: typeof fetch;
    export let title: string | undefined = undefined;

    let entries = walter_selection.umlagetypen(fetchImpl);
    function onSubmit() {
        entries = walter_selection.umlagetypen(fetchImpl);
    }

    let addEntry = {};
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
            title="Umlagetypen"
            bind:addEntry
            addUrl={WalterUmlagetypEntry.ApiURL}
            {onSubmit}
        >
            <WalterUmlagetyp entry={addEntry} {fetchImpl} />
        </WalterQuickAddButton>
    {/if}
</div>

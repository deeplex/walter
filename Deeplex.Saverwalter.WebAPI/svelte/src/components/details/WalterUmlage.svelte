<script lang="ts">
    import {
        WalterComboBox,
        WalterMultiSelect,
        WalterTextArea
    } from '$walter/components';
    import { Row, TextInputSkeleton } from 'carbon-components-svelte';
    import type { WalterUmlageEntry } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';

    export let entry: Partial<WalterUmlageEntry> = {};
    export let fetchImpl: typeof fetch;

    const betriebskostentypen = walter_selection.betriebskostentypen(fetchImpl);
    const umlageschluessel = walter_selection.umlageschluessel(fetchImpl);
    const wohnungen = walter_selection.wohnungen(fetchImpl);
    const zaehler = walter_selection.zaehler(fetchImpl);
</script>

<Row>
    {#await betriebskostentypen}
        <TextInputSkeleton />
    {:then entries}
        <WalterComboBox
            entry={entries}
            bind:value={entry.typ}
            titleText="Typ"
        />
    {/await}
    {#await umlageschluessel}
        <TextInputSkeleton />
    {:then entries}
        <WalterComboBox
            entry={entries}
            bind:value={entry.schluessel}
            titleText="Umlageschlüssel"
        />
    {/await}
</Row>
<Row>
    {#await wohnungen}
        <TextInputSkeleton />
    {:then entries}
        <WalterMultiSelect
            bind:value={entry.selectedWohnungen}
            entry={entries}
            titleText="Wohnungen"
        />
    {/await}
</Row>
{#if entry.schluessel?.id === '3'}
    <Row>
        {#await zaehler}
            <TextInputSkeleton />
        {:then entries}
            <WalterMultiSelect
                bind:value={entry.selectedZaehler}
                entry={entries}
                titleText="Zähler"
            />
        {/await}
    </Row>
{/if}
<Row>
    <WalterTextArea labelText="Notiz" bind:value={entry.notiz} />
</Row>

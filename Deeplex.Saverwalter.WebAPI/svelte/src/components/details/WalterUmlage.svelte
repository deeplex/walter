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
    <WalterComboBox
        entries={betriebskostentypen}
        bind:value={entry.typ}
        titleText="Typ"
    />
    <WalterComboBox
        entries={umlageschluessel}
        bind:value={entry.schluessel}
        titleText="Umlageschlüssel"
    />
</Row>
<Row>
    <WalterMultiSelect
        bind:value={entry.selectedWohnungen}
        entries={wohnungen}
        titleText="Wohnungen"
    />
</Row>
{#if entry.schluessel?.id === '3'}
    <Row>
        <WalterMultiSelect
            bind:value={entry.selectedZaehler}
            entries={zaehler}
            titleText="Zähler"
        />
    </Row>
{/if}
<Row>
    <WalterTextArea labelText="Notiz" bind:value={entry.notiz} />
</Row>

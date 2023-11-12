<script lang="ts">
    import {
        WalterComboBox,
        WalterMultiSelectWohnung,
        WalterMultiSelect,
        WalterTextArea,

        WalterMultiSelectZaehler

    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterUmlageEntry } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';

    export let entry: Partial<WalterUmlageEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;

    const betriebskostentypen = walter_selection.betriebskostentypen(fetchImpl);
    const umlageschluessel = walter_selection.umlageschluessel(fetchImpl);
    const wohnungen = walter_selection.wohnungen(fetchImpl);
    const zaehler = walter_selection.zaehler(fetchImpl);
</script>

<Row>
    <WalterComboBox
        required
        {readonly}
        entries={betriebskostentypen}
        bind:value={entry.typ}
        titleText="Typ"
    />
    <WalterComboBox
        required
        {readonly}
        entries={umlageschluessel}
        bind:value={entry.schluessel}
        titleText="Umlageschlüssel"
    />
</Row>
<Row>
    <WalterMultiSelectWohnung
        {fetchImpl}
        {readonly}
        bind:value={entry.selectedWohnungen}
        titleText="Wohnungen"
    />
</Row>
<!-- If entry.schluessel is nach Verbrauch -->
{#if entry.schluessel?.id === '3'}
    <Row>
        <WalterMultiSelectZaehler
            {fetchImpl}
            {readonly}
            bind:value={entry.selectedZaehler}
            titleText="Zähler"
        />
    </Row>
{/if}

<Row>
    <WalterTextArea {readonly} labelText="Beschreibung" bind:value={entry.beschreibung} />
</Row>

<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>

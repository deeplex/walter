<script lang="ts">
    import {
        WalterComboBox,
        WalterMultiSelectWohnung,
        WalterTextArea,
        WalterMultiSelectZaehler,
        WalterComboBoxUmlagetyp
    } from '$walter/components';

    import WalterUmlageHKVO from './WalterUmlageHKVO.svelte';

    import { Row } from 'carbon-components-svelte';
    import type { WalterUmlageEntry } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';

    export let entry: Partial<WalterUmlageEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false;
    }

    const umlageschluessel = walter_selection.umlageschluessel(fetchImpl);
</script>

<Row>
    <WalterComboBoxUmlagetyp
        {readonly}
        {fetchImpl}
        required
        bind:value={entry.typ}
        title="Betriebskostentyp der Umlage"
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
    <WalterTextArea
        {readonly}
        labelText="Beschreibung"
        bind:value={entry.beschreibung}
    />
</Row>

<WalterUmlageHKVO
    bind:entry
    {fetchImpl}
    bind:selectedWohnungen={entry.selectedWohnungen}
/>

<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>

<script lang="ts">
    import {
        WalterComboBox,
        WalterMultiSelect,
        WalterTextArea
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterSelectionEntry, WalterUmlageEntry } from '$walter/lib';

    export let betriebskostentypen: WalterSelectionEntry[];
    export let umlageschluessel: WalterSelectionEntry[];
    export let wohnungen: WalterSelectionEntry[];
    export let zaehler: WalterSelectionEntry[];

    export let entry: Partial<WalterUmlageEntry> = {};
</script>

<Row>
    <WalterComboBox
        entry={betriebskostentypen}
        bind:value={entry.typ}
        titleText="Typ"
    />
    <WalterComboBox
        entry={umlageschluessel}
        bind:value={entry.schluessel}
        titleText="Umlageschlüssel"
    />
</Row>
<Row>
    <WalterMultiSelect
        bind:value={entry.selectedWohnungen}
        entry={wohnungen}
        titleText="Wohnungen"
    />
</Row>
{#if entry.schluessel?.id === '3'}
    <Row>
        <WalterMultiSelect
            bind:value={entry.selectedZaehler}
            entry={zaehler}
            titleText="Zähler"
        />
    </Row>
{/if}
<Row>
    <WalterTextArea labelText="Notiz" bind:value={entry.notiz} />
</Row>

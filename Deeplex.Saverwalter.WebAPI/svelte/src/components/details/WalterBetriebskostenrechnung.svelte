<script lang="ts">
    import {
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea
    } from '$walter/components';
    import { ComboBox, Row, TextInputSkeleton } from 'carbon-components-svelte';
    import type {
        WalterBetriebskostenrechnungEntry,
        WalterSelectionEntry
    } from '$walter/lib';
    import { shouldFilterItem } from './WalterBetriebskostenrechnung';

    export let entry: Partial<WalterBetriebskostenrechnungEntry> = {};
    export let betriebskostentypen: WalterSelectionEntry[];
    export let umlagen_wohnungen: WalterSelectionEntry[];

    let umlageEntries: WalterSelectionEntry[];
    updateUmlageEntries(entry.typ?.id);

    function selectTyp(e: CustomEvent) {
        updateUmlageEntries(e.detail.selectedItem.id);
        entry.typ = e.detail.selectedItem;
        entry.umlage = undefined;
    }

    function selectUmlage(e: CustomEvent) {
        entry.umlage = e.detail.selectedItem;
        // entry.typ = entry.umlage.filter;
    }

    function updateUmlageEntries(id: string | number | undefined) {
        umlageEntries = umlagen_wohnungen.filter((u) => u.filter === id);
    }
</script>

<Row>
    {#await betriebskostentypen}
        <TextInputSkeleton />
    {:then items}
        <ComboBox
            selectedId={entry?.typ?.id}
            on:select={selectTyp}
            style="padding-right: 1rem"
            {items}
            value={entry?.typ?.text}
            titleText="Betriebskostentyp der Umlage"
            {shouldFilterItem}
        />
    {/await}

    {#await umlagen_wohnungen}
        <TextInputSkeleton />
    {:then}
        <ComboBox
            disabled={!entry.typ}
            selectedId={entry.umlage?.id}
            on:select={selectUmlage}
            style="padding-right: 1rem"
            bind:items={umlageEntries}
            titleText="Wohnungen der Umlage"
            {shouldFilterItem}
        />
    {/await}
</Row>

<Row>
    <WalterNumberInput
        bind:value={entry.betreffendesJahr}
        hideSteppers={false}
        label="Betreffendes Jahr"
    />
    <WalterNumberInput bind:value={entry.betrag} label="Betrag" />
    <WalterDatePicker bind:value={entry.datum} labelText="Datum" />
</Row>
<Row>
    <WalterTextArea bind:value={entry.notiz} labelText="Notiz" />
</Row>

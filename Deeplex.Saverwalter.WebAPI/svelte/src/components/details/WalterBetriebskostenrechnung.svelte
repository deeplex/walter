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
    import { walter_selection } from '$walter/services/requests';

    export let entry: Partial<WalterBetriebskostenrechnungEntry> = {};
    export let fetchImpl: typeof fetch;

    const betriebskostentypen = walter_selection.betriebskostentypen(fetchImpl);
    const umlagen_wohnungen = walter_selection.umlagen_wohnungen(fetchImpl);

    let selectedUmlageId: string | number | undefined = undefined;

    let umlageEntries: WalterSelectionEntry[] = [];
    updateUmlageEntries(entry.typ?.id);

    function selectTyp(e: CustomEvent) {
        updateUmlageEntries(e.detail.selectedItem.id);
        entry.typ = e.detail.selectedItem;
        entry.umlage = undefined;
    }

    function selectUmlage(e: CustomEvent) {
        entry.umlage = e.detail.selectedItem;
        selectedUmlageId = entry.umlage?.id;
        // entry.typ = entry.umlage.filter;
    }

    async function updateUmlageEntries(id: string | number | undefined) {
        umlageEntries = await umlagen_wohnungen.then((fulfilled) =>
            fulfilled.filter((u) => u.filter === id)
        );
        selectedUmlageId = entry.umlage?.id;
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
    <ComboBox
        disabled={!entry.typ}
        bind:selectedId={selectedUmlageId}
        on:select={selectUmlage}
        style="padding-right: 1rem"
        bind:items={umlageEntries}
        placeholder="{umlageEntries.length} Umlagen für diesen Kostentypen gefunden"
        titleText="Wohnungen der Umlage"
        {shouldFilterItem}
    />
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

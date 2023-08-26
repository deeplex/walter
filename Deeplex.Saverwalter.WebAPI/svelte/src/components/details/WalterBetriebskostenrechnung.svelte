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
    import { convertDateCanadian } from '$walter/services/utils';

    export let entry: Partial<WalterBetriebskostenrechnungEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;

    const betriebskostentypen = walter_selection.betriebskostentypen(fetchImpl);
    const umlagen_wohnungen = walter_selection.umlagen_wohnungen(fetchImpl);

    let selectedUmlageId: string | number | undefined = entry.umlage?.id;

    let umlageEntries: WalterSelectionEntry[] = [];
    updateUmlageEntries(entry.typ?.id);

    function selectTyp(e: CustomEvent) {
        updateUmlageEntries(e.detail.selectedItem?.id);
        entry.typ = e.detail.selectedItem;
        console.log(e);
        // This should be only the case when manually changing the type.
        if ((e as any).explicitOriginalTarget.tagName === "DIV")
        {
            entry.umlage = undefined;
        }
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

    if (!entry.betreffendesJahr) {
        entry.betreffendesJahr = new Date().getFullYear() - 1;
    }
    if (!entry.datum) {
        entry.datum = convertDateCanadian(new Date());
    }
</script>

<Row>
    {#await betriebskostentypen}
        <TextInputSkeleton />
        <TextInputSkeleton />
    {:then items}
        <ComboBox
            invalid={!entry?.typ?.id}
            invalidText={"Betriebskostentyp der Umlage ist ein notwendiges Feld"}
            disabled={readonly}
            selectedId={entry?.typ?.id}
            on:select={selectTyp}
            style="padding-right: 1rem"
            {items}
            value={entry?.typ?.text}
            titleText="Betriebskostentyp der Umlage"
            {shouldFilterItem}
        />
        <ComboBox
            invalid={!selectedUmlageId}
            invalidText={"Wohnungen der Umlage ist ein notwendiges Feld"}
            disabled={readonly || !entry.typ}
            bind:selectedId={selectedUmlageId}
            on:select={selectUmlage}
            style="padding-right: 1rem"
            bind:items={umlageEntries}
            placeholder="{umlageEntries.length} Umlagen für diesen Kostentypen gefunden"
            titleText="Wohnungen der Umlage"
            {shouldFilterItem}
        />
    {/await}
</Row>

<Row>
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.betreffendesJahr}
        hideSteppers={false}
        label="Betreffendes Jahr"
    />
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.betrag}
        label="Betrag" />
    <WalterDatePicker
        required
        disabled={readonly}
        bind:value={entry.datum}
        labelText="Datum" />
</Row>
<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>

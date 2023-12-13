<script lang="ts">
    import {
        WalterDatePicker,
        WalterLinkTile,
        WalterLinks,
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
    import { onMount } from 'svelte';
    import { page } from '$app/stores';
    import WalterBetriebskostenrechnungen from '../lists/WalterBetriebskostenrechnungen.svelte';

    export let entry: Partial<WalterBetriebskostenrechnungEntry> = {};
    export let rechnungen: WalterBetriebskostenrechnungEntry[] = [];
    export let fetchImpl: typeof fetch;
    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false;
    }

    const umlagetypen = walter_selection.umlagetypen(fetchImpl);
    const umlagen_wohnungen = walter_selection.umlagen_wohnungen(fetchImpl);

    let umlageEntries: WalterSelectionEntry[] = [];

    onMount(async () => {
        await updateUmlageEntries(entry.typ?.id);
        entry.umlage = umlageEntries.find(
            (umlage) => umlage.id === entry.umlage?.id
        );
    });

    async function selectTyp(e: CustomEvent) {
        const id = `${e.detail.selectedItem.id}`;
        await updateUmlageEntries(id);
        if (entry.typ?.id !== `${id}`) {
            entry.umlage = undefined;
        }
        entry.typ = e.detail.selectedItem;
    }

    function selectUmlage(e: CustomEvent) {
        if (e.detail.selectedItem) {
            entry.umlage = e.detail.selectedItem;
        }
    }

    async function updateUmlageEntries(id: string | number | undefined) {
        umlageEntries = await umlagen_wohnungen.then((fulfilled) =>
            fulfilled.filter((u) => u.filter === id)
        );
    }

    if (!entry.betreffendesJahr) {
        entry.betreffendesJahr = new Date().getFullYear() - 1;
    }
    if (!entry.datum) {
        entry.datum = convertDateCanadian(new Date());
    }
</script>

<Row>
    {#await umlagetypen}
        <TextInputSkeleton />
        <TextInputSkeleton />
    {:then items}
        <ComboBox
            invalid={!entry.typ?.id}
            invalidText={'Betriebskostentyp der Umlage ist ein notwendiges Feld'}
            disabled={readonly}
            selectedId={entry.typ?.id}
            on:select={selectTyp}
            style="padding-right: 1rem"
            {items}
            value={entry?.typ?.text}
            titleText="Betriebskostentyp der Umlage"
            {shouldFilterItem}
        />
        <ComboBox
            invalid={!entry.umlage?.id}
            invalidText={'Wohnungen der Umlage ist ein notwendiges Feld'}
            disabled={readonly || !entry.typ}
            selectedId={entry.umlage?.id}
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
        label="Betrag"
    />
    <WalterDatePicker
        required
        disabled={readonly}
        bind:value={entry.datum}
        labelText="Datum"
    />
</Row>
<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>

{#if $page.url.pathname !== `/umlagen/${entry.umlage?.id}`}
    <WalterLinks>
        <WalterLinkTile
            name={'Umlage ansehen'}
            href={`/umlagen/${entry.umlage?.id}`}
        />
        {#if entry.umlage?.id}
            <WalterBetriebskostenrechnungen
                {fetchImpl}
                title="Rechnungen"
                rows={rechnungen.filter(
                    (e) => +e.umlage.id === +(entry.umlage?.id || 0)
                )}
            />
        {/if}
    </WalterLinks>
{/if}

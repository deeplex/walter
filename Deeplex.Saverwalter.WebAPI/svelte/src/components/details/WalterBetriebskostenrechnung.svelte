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
    import { S3URL } from '$walter/services/s3';

    export let entry: Partial<WalterBetriebskostenrechnungEntry> = {};
    export let rechnungen: WalterBetriebskostenrechnungEntry[] = [];
    export let fetchImpl: typeof fetch;
    export let readonly = false;

    const umlagetypen = walter_selection.umlagetypen(fetchImpl);
    let umlagen_wohnungen: WalterSelectionEntry[] = [];
    let umlageEntries: WalterSelectionEntry[] = [];
    let selectedUmlage = entry?.umlage?.id;

    onMount(() => {
        walter_selection.umlagen_wohnungen(fetchImpl).then((res) => {
            umlagen_wohnungen = res;
        });

        if (!entry.betreffendesJahr) {
            entry.betreffendesJahr = new Date().getFullYear() - 1;
        }
        if (!entry.datum) {
            entry.datum = convertDateCanadian(new Date());
        }
    });

    $: {
        readonly = entry?.permissions?.update === false;
        umlageEntries = umlagen_wohnungen.filter(
            (umlage) => umlage.filter === `${entry.typ?.id}`
        );
        selectedUmlage = umlageEntries.find(
            (umlage) => umlage.id === entry.umlage?.id
        )?.id;
    }

    function selectTyp(e: CustomEvent) {
        const id = `${e.detail.selectedItem.id}`;
        if (entry.typ?.id !== `${id}`) {
            entry.umlage = undefined;
        }
        entry.typ = e.detail.selectedItem;
    }

    function selectUmlage(e: CustomEvent) {
        if (e.detail.selectedItem) {
            entry.umlage = e.detail.selectedItem;
            selectedUmlage = e.detail.selectedItem.id;
        }
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
            bind:selectedId={selectedUmlage}
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
            s3ref={S3URL.umlage(`${entry.umlage?.id}`)}
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

<script lang="ts">
    import {
        WalterBetriebskostenrechnungEntry,
        type WalterUmlageEntry
    } from '$walter/lib';
    import { convertDateCanadian } from '$walter/services/utils';
    import {
        walter_data_rechnungentabelle,
        type WalterDataConfigType,
        type WalterDataPoint
    } from '../data/WalterData';
    import WalterDataHeatmapChart from '../data/WalterDataHeatmapChart.svelte';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import WalterBetriebskostenrechnung from './WalterBetriebskostenrechnung.svelte';
    import { Grid, Row } from 'carbon-components-svelte';

    export let config: WalterDataConfigType;
    export let umlagen: WalterUmlageEntry[];
    export let fetchImpl: typeof fetch;
    export let year: number;

    function updateEntry(
        umlageId: string,
        umlageTyp: string,
        rechnungTyp: string
    ) {
        addEntry = {
            datum: convertDateCanadian(new Date()),
            betreffendesJahr: year,
            typ: {
                id: rechnungTyp,
                text: umlageTyp
            },
            umlage: {
                id: umlageId,
                text: umlageTyp
            }
        };
    }

    function click(e: CustomEvent, config: WalterDataConfigType) {
        const targetWithData = e.target as { __data__?: WalterDataPoint };
        const data = targetWithData?.__data__;

        // NOTE: Blame the tab implementation for all the tables being triggered at once.
        // Because of that all the configs with different years are filtered here.
        if (!data) return;

        const group = config.data.filter((entry) => entry.group === data.group);

        const thisEntry = group.find((entry) => entry.key === data.key);

        if (!thisEntry) return;

        const umlageTyp = data.key;
        const rechnungTyp = thisEntry.id2!;
        const umlageId = thisEntry.id!;

        if (umlageId) {
            updateEntry(umlageId, umlageTyp!, rechnungTyp);
            addModalOpen = true;
        }
    }

    let addEntry: Partial<WalterBetriebskostenrechnungEntry> = {};
    let addModalOpen = false;
    let title = 'Umlage';
    let rechnungen = umlagen.flatMap((e) => e.betriebskostenrechnungen);

    function onSubmit(new_value: unknown) {
        const value = new_value as WalterBetriebskostenrechnungEntry;
        const umlage = umlagen.find((e) => e.id === +value.umlage?.id);
        if (!umlage) {
            console.warn('Umlage not found: ', value.umlage.id);
            return;
        }
        umlage!.betriebskostenrechnungen.push(value);
        rechnungen.push(value);
        config = walter_data_rechnungentabelle(umlagen, year);
    }
</script>

<WalterDataWrapperQuickAdd
    {onSubmit}
    bind:addEntry
    addUrl={WalterBetriebskostenrechnungEntry.ApiURL}
    bind:addModalOpen
    {title}
>
    <WalterBetriebskostenrechnung
        {fetchImpl}
        bind:entry={addEntry}
        {rechnungen}
    />
</WalterDataWrapperQuickAdd>

<div style="left: 0; min-height: 30em; display: block; min-width: 60em;">
    <h3>Umlagentabelle</h3>
    <div id="umlagentabelle">
        <WalterDataHeatmapChart {click} bind:config />
    </div>
</div>

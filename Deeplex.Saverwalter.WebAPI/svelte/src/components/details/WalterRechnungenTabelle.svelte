<script lang="ts">
    import type {
        WalterBetriebskostenrechnungEntry,
        WalterUmlageEntry
    } from '$walter/lib';
    import { convertDateCanadian } from '$walter/services/utils';
    import {
        walter_data_rechnungentabelle,
        type WalterDataConfigType
    } from '../data/WalterData';
    import WalterDataHeatmapChart from '../data/WalterDataHeatmapChart.svelte';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import WalterBetriebskostenrechnung from './WalterBetriebskostenrechnung.svelte';
    import { Grid } from 'carbon-components-svelte';

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
        const data = (e as any).target.__data__;

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
            updateEntry(umlageId, umlageTyp, rechnungTyp);
            addModalOpen = true;
        }
    }

    let addEntry: Partial<WalterBetriebskostenrechnungEntry> = {};
    let addModalOpen = false;
    let addUrl = `/api/betriebskostenrechnungen`;
    let title = 'Umlage';
    let rechnungen = umlagen.flatMap((e) => e.betriebskostenrechnungen);

    function onSubmit(new_value: any) {
        const umlage = umlagen.find((e) => e.id === +new_value.umlage?.id);
        if (!umlage) {
            console.warn('Umlage not found: ', new_value.umlage.id);
            return;
        }
        umlage!.betriebskostenrechnungen.push(new_value);
        rechnungen.push(new_value);
        config = walter_data_rechnungentabelle(umlagen, year);
    }
</script>

<WalterDataWrapperQuickAdd
    {onSubmit}
    bind:addEntry
    {addUrl}
    bind:addModalOpen
    {title}
>
    <WalterBetriebskostenrechnung
        {fetchImpl}
        bind:entry={addEntry}
        {rechnungen}
    />
</WalterDataWrapperQuickAdd>

<Grid>
    <h3>Umlagentabelle</h3>
    <WalterDataHeatmapChart {click} bind:config />
</Grid>

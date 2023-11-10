<script lang="ts">
    import type { WalterVertragEntry, WalterMieteEntry } from "$walter/lib";
    import { months, walter_data_miettabelle, type WalterDataConfigType } from "../data/WalterData";
    import WalterDataHeatmapChart from "../data/WalterDataHeatmapChart.svelte";
    import { convertDateCanadian } from "$walter/services/utils";
    import WalterDataWrapperQuickAdd from "../elements/WalterDataWrapperQuickAdd.svelte";
    import { WalterMiete } from "..";
    import { Grid } from "carbon-components-svelte";

    export let config: WalterDataConfigType;
    export let vertraege: WalterVertragEntry[];
    export let year: number;
    export let mieten: WalterMieteEntry[];

    function updateEntry(vertragId: string, monthIndex: number, wohnung: string, betrag: number)
    {
        addEntry = {
            zahlungsdatum: convertDateCanadian(new Date()),
            betreffenderMonat: convertDateCanadian(new Date(year, monthIndex, 1)),
            vertrag: {
                id: vertragId,
                text: wohnung
            },
            betrag: betrag
        }

        title = `${wohnung}`;
    }

    function click(e: CustomEvent, config: WalterDataConfigType)
	{
		const data = (e as any).target.__data__;

        // NOTE: Blame the tab implementation for all the tables being triggered at once.
        // Because of that all the configs with different years are filtered here.
		if (!data) return;

        let monthIndex = months.findIndex(month => month === data.key);

        if (monthIndex === -1) return;

        let lastMonthIndex = monthIndex === 0 ? months.length - 1 : monthIndex - 1;
        
        const group = config.data.filter(entry => entry.group === data.group);

        const lastEntry = group.find(entry =>
            entry.key === months[lastMonthIndex] &&
            entry.year === (monthIndex === 0 ? year - 1 : year));

        const thisEntry = group.find(entry =>
            entry.key === data.key &&
            entry.year === year);

        const vertragId = thisEntry?.id!;
        if (vertragId)
        {
            const wohnung = data.group;
            const lastValue = (lastEntry?.value as number) || 0;

            updateEntry(vertragId, monthIndex, wohnung, lastValue);
            addModalOpen = true;
        }
	}

    let addEntry: Partial<WalterMieteEntry> = {};
    let addModalOpen = false;
    let addUrl = `/api/mieten`;
    let title = "Unbekannter Vertrag";

    function onSubmit(new_value: any)
    {
        const vertrag = vertraege.find(e => e.id === +new_value.vertrag?.id);
        if (!vertrag)
        {
            console.warn("Vertrag not found: ", new_value.vertrag?.id);
            return;
        }
        vertrag!.mieten.push(new_value);
        config = walter_data_miettabelle(vertraege, year);
    }
</script>

<WalterDataWrapperQuickAdd
    {onSubmit}
    bind:addEntry
    {addUrl}
    bind:addModalOpen
    {title}>
    <WalterMiete entry={addEntry} mieten={mieten}/>
</WalterDataWrapperQuickAdd>

<Grid>
    <h3>Miettabelle {year}</h3>
    <WalterDataHeatmapChart {click} bind:config />
</Grid>
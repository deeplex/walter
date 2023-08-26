<script lang="ts">
    import type { WalterMieteEntry, WalterVertragEntry } from "$walter/lib";
    import { months, walter_data_miettabelle, type WalterDataConfigType } from "../data/WalterData";
    import WalterDataHeatmapChart from "../data/WalterDataHeatmapChart.svelte";
    import { convertDateCanadian } from "$walter/services/utils";
    import WalterDataWrapperQuickAdd from "../elements/WalterDataWrapperQuickAdd.svelte";
    import { WalterMiete } from "..";

    export let vertraege: WalterVertragEntry[];
    export let selectedYear: number;
    export let year: number;

    function updateEntry(vertradId: string, monthIndex: number, year: number, wohnung: string, betrag: number)
    {
        addEntry.betreffenderMonat = convertDateCanadian(new Date(year, monthIndex, 1));
        addEntry.vertrag = {
            id: vertradId,
            text: "UNKNOWN"
        };
        addEntry.betrag = betrag;

        title = `${wohnung}`;
    }

    function click(e: CustomEvent, config: WalterDataConfigType)
	{
		const data = (e as any).target.__data__;

        // NOTE: Blame the tab implementation for all the tables being triggered at once.
        // Because of that all the configs with different years are filtered here.
		if (!data || !selectedYear || year !== selectedYear) return;

        let monthIndex = months.findIndex(month => month === data.key);

        if (monthIndex === -1) return;

        let lastMonthIndex = monthIndex === 0 ? months.length - 1 : monthIndex - 1;
        
        const group = config.data.filter(entry => entry.group === data.group);

        const lastEntry = group.find(entry =>
            entry.key === months[lastMonthIndex] &&
            entry.year === (monthIndex === 0 ? selectedYear - 1 : selectedYear));

        const thisEntry = group.find(entry =>
            entry.key === data.key &&
            entry.year === selectedYear);

        const vertragId = thisEntry?.id!;
        if (vertragId)
        {
            const wohnung = data.group;
            const lastValue = (lastEntry?.value as number) || 0;

            updateEntry(vertragId, monthIndex, selectedYear, wohnung, lastValue);
            addModalOpen = true;
        }
	}

    let addEntry: Partial<WalterMieteEntry> = {
        zahlungsdatum: convertDateCanadian(new Date())
    };
    let addModalOpen = false;
    let addUrl = `/api/mieten`;
    let title = "Unbekannter Vertrag";
</script>

<WalterDataWrapperQuickAdd
    bind:addEntry
    {addUrl}
    bind:addModalOpen
    {title}>
    <WalterMiete entry={addEntry} />
</WalterDataWrapperQuickAdd>

<WalterDataHeatmapChart {click} config={walter_data_miettabelle(vertraege, year)} />
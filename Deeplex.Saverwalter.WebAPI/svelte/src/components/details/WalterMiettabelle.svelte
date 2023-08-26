<script lang="ts">
    import type { WalterMieteEntry, WalterVertragEntry } from "$walter/lib";
    import { Grid, Tab, TabContent, Tabs, Tile } from "carbon-components-svelte";
    import { months, walter_data_miettabelle, type WalterDataConfigType } from "../data/WalterData";
    import WalterDataHeatmapChart from "../data/WalterDataHeatmapChart.svelte";
    import { convertDateCanadian } from "$walter/services/utils";
    import WalterDataWrapperQuickAdd from "../elements/WalterDataWrapperQuickAdd.svelte";
    import WalterMiete from "./WalterMiete.svelte";

    export let vertraege: WalterVertragEntry[];

    const mieten = vertraege.flatMap(vertrag => vertrag.mieten)
        .sort((a, b) => new Date(a.betreffenderMonat).getTime() - new Date(b.betreffenderMonat).getTime());
    const years: number[] = [];
    for(let i = new Date(mieten[0].betreffenderMonat).getFullYear();
        i < new Date(mieten[mieten.length - 1].betreffenderMonat).getFullYear() + 1;
        ++i)
    {
        years.push(i);
    }

    let selected = years.findIndex(year => year === new Date().getFullYear()) || years.length - 1;

    let addEntry: Partial<WalterMieteEntry> = {
        zahlungsdatum: convertDateCanadian(new Date())
    };
    let addModalOpen = false;
    let addUrl = `/api/mieten`;
    let title = "Unbekannter Vertrag";

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

    function click(e: CustomEvent, config: WalterDataConfigType, year: number)
	{
		const data = (e as any).target.__data__;
        const selectedYear = years[selected];

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

        console.log(thisEntry?.id, selectedYear, months[monthIndex], (monthIndex === 0 ? selectedYear - 1 : selectedYear), months[lastMonthIndex]);

        const vertragId = thisEntry?.id!;

        if (vertragId)
        {
            const wohnung = data.group;
            const lastValue = (lastEntry?.value as number) || 0;

            updateEntry(vertragId, monthIndex, selectedYear, wohnung, lastValue);
            addModalOpen = true;
        }
	}
</script>

<WalterDataWrapperQuickAdd
    bind:addEntry
    {addUrl}
    bind:addModalOpen
    {title}>
    <WalterMiete entry={addEntry} />
</WalterDataWrapperQuickAdd>

<div  style="margin-top: 5em">
    <Grid>
        <Tile>
            <h2>Miettabelle</h2>
            <Tabs bind:selected>
                {#each years as year}
                <Tab label={`${year}`}/>
                {/each}
                <svelte:fragment slot="content">
                    {#each years as year}
                    <TabContent>
                        <WalterDataHeatmapChart
                            {year}
                            {click}
                            config={walter_data_miettabelle(vertraege, year)} />
                    </TabContent>
                    {/each}
                </svelte:fragment>
            </Tabs>
        </Tile>
    </Grid>
</div>
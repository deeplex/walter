<script lang="ts">
    import type { WalterBetriebskostenrechnungEntry, WalterUmlageEntry } from "$walter/lib";
    import { convertDateCanadian } from "$walter/services/utils";
    import { walter_data_rechnungentabelle, type WalterDataConfigType } from "../data/WalterData";
    import WalterDataHeatmapChart from "../data/WalterDataHeatmapChart.svelte";
    import WalterDataWrapperQuickAdd from "../elements/WalterDataWrapperQuickAdd.svelte";
    import WalterBetriebskostenrechnung from "./WalterBetriebskostenrechnung.svelte";
    import { Grid } from "carbon-components-svelte";

    export let umlagen: WalterUmlageEntry[];
    export let selectedYear: number;
    export let fetchImpl: typeof fetch;
    export let year: number;

    function updateEntry(umlageId: string, umlageTyp: string, rechnungTyp: string)
    {
        addEntry.betreffendesJahr = selectedYear;
        addEntry.typ = {
            id: rechnungTyp,
            text: umlageTyp,
        }
        addEntry.umlage = {
            id: umlageId,
            text: umlageTyp
        }
        console.log(addEntry);
    }

    function click(e: CustomEvent, config: WalterDataConfigType)
	{
		const data = (e as any).target.__data__;

        // NOTE: Blame the tab implementation for all the tables being triggered at once.
        // Because of that all the configs with different years are filtered here.
		if (!data || !selectedYear || year !== selectedYear) return;

        const group = config.data.filter(entry => entry.group === data.group);


        const thisEntry = group.find(entry =>
            entry.key === data.key);

        if (!thisEntry) return;

        const umlageTyp = data.key;
        const rechnungTyp = thisEntry.id2!;
        const umlageId = thisEntry.id!;

        if (umlageId)
        {
            updateEntry(umlageId, umlageTyp, rechnungTyp);
            addModalOpen = true;
        }
	}

    let addEntry: Partial<WalterBetriebskostenrechnungEntry> = {
        datum: convertDateCanadian(new Date()),
    };
    let addModalOpen = false;
    let addUrl = `/api/betriebskostenrechnungen`;
    let title = "Umlage";
</script>

<WalterDataWrapperQuickAdd
    bind:addEntry
    {addUrl}
    bind:addModalOpen
    {title}>
    <WalterBetriebskostenrechnung {fetchImpl} entry={addEntry} />
</WalterDataWrapperQuickAdd>


<Grid>
    <h3>Umlagentabelle</h3>
    <WalterDataHeatmapChart {click} config={walter_data_rechnungentabelle(umlagen, year)} />
</Grid>
<script lang="ts">
    import type { WalterBetriebskostenabrechnungEntry } from "$walter/types";
    import { AccordionItem } from "carbon-components-svelte";
    import { convertToBeforeAfterDataFromRechnungen, convertToDataFromRechnungen, convertToDiffDataFromRechnungen } from "./WalterData";
    import WalterDataPieChart from "./WalterDataPieChart.svelte";
    import WalterDataBarChartSimple from "./WalterDataBarChartSimple.svelte";

    export let abrechnung: WalterBetriebskostenabrechnungEntry;
    const rechnungen = abrechnung.abrechnungseinheiten.flatMap(einheit => einheit.rechnungen);
    const rechnungenFlat = convertToDataFromRechnungen(rechnungen);

    const rechnungenDiff = convertToDiffDataFromRechnungen(rechnungen);
    const rechnungenBeforeAfter = convertToBeforeAfterDataFromRechnungen(rechnungen);
</script>

<AccordionItem title="Übersicht">
    <WalterDataPieChart data={rechnungenFlat} title="Rechnungen" />
    <WalterDataBarChartSimple data={rechnungenDiff} title="Rechnungen im Vergleich zum Vorjahr"/>
    <WalterDataBarChartSimple data={rechnungenBeforeAfter} title="Differenz zum Vorjahr"/>    
</AccordionItem>

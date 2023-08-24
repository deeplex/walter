<script lang="ts">
    import type { WalterBetriebskostenabrechnungEntry } from "$walter/types";
    import { AccordionItem } from "carbon-components-svelte";
    import { walter_data_rechnungen_pairs, walter_data_rechnungen, walter_data_rechnungen_diff } from "./WalterData";
    import WalterDataPieChart from "./WalterDataPieChart.svelte";
    import WalterDataBarChartSimple from "./WalterDataBarChartSimple.svelte";

    export let abrechnung: WalterBetriebskostenabrechnungEntry;

    const rechnungen = abrechnung.abrechnungseinheiten.flatMap(einheit => einheit.rechnungen);
    const rechnungenFlat = walter_data_rechnungen("Rechnungen", rechnungen);
    const rechnungenDiff = walter_data_rechnungen_diff("Rechnungen im Vergleich zum Vorjahr", rechnungen);
    const rechnungenBeforeAfter = walter_data_rechnungen_pairs("Differenz zum Vorjahr", rechnungen);
</script>

<AccordionItem title="Übersicht">
    <WalterDataPieChart config={rechnungenFlat} />
    <WalterDataBarChartSimple config={rechnungenDiff} />
    <WalterDataBarChartSimple config={rechnungenBeforeAfter} />    
</AccordionItem>

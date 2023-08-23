<script lang="ts">
    import type { WalterBetriebskostenabrechnungEntry } from "$walter/types";
    import { Accordion, AccordionItem, ExpandableTile, Row } from "carbon-components-svelte";
    import WalterRechnungAenderung from "./WalterRechnungAenderung.svelte";
    import WalterRechnungAenderung2 from "./WalterRechnungAenderung2.svelte";
    import WalterRechnungVerteilung from "./WalterRechnungVerteilung.svelte";

    export let abrechnung: WalterBetriebskostenabrechnungEntry;
</script>

<AccordionItem title="Übersicht">
    <WalterRechnungVerteilung {abrechnung} />
    <WalterRechnungAenderung {abrechnung}/>
    <WalterRechnungAenderung2 {abrechnung}/>    
</AccordionItem>

<script lang="ts">
    import type { WalterBetriebskostenabrechnungEntry } from "$walter/types";
    import { Column, Row } from "carbon-components-svelte";
    import WalterRechnungAenderung from "./WalterRechnungAenderung.svelte";
    import WalterRechnungVerteilung from "./WalterRechnungVerteilung.svelte";

    export let abrechnung: WalterBetriebskostenabrechnungEntry;

    const zaehler = abrechnung.abrechnungseinheiten
        .flatMap(einheit => einheit.verbrauchAnteil
            .map(anteil => anteil.alleZaehler));
</script>

<div style="margin: 1em">
<Row>
    <Column sm={{ span: 0.5, offset: 0 }}>
        <WalterRechnungVerteilung {abrechnung} />
    </Column>
    <Column sm={{span: 0.5, offset: 0}}>
        <div style="margin: 2em"></div>
    </Column>
    <Column sm={{ span: 3, offset: 0 }}>
        <WalterRechnungAenderung {abrechnung}/>
    </Column>
</Row>
</div>

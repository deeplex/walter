<script lang="ts">
    import type { WalterBetriebskostenabrechnungEntry } from "$walter/types";
    import { WalterLinks, WalterVertraege, WalterWohnungen, WalterZaehlerList } from "..";

    export let fetchImpl: typeof fetch;
    export let abrechnung: WalterBetriebskostenabrechnungEntry;
</script>

<WalterLinks>
    <WalterWohnungen title="Aufteilung nach Wohnfläche (n. WF, verteilt auf die Wohnungen)" {fetchImpl} rows={abrechnung.wohnungen}/>
    <WalterWohnungen title="Aufteilung nach Nutzfläche (n. NF, verteilt auf die Wohnungen)" {fetchImpl} rows={abrechnung.wohnungen}/>
    <WalterVertraege title="Aufteilung nach Personenzahl (n. Pers., verteilt nach Personenzahl im Vertrag)" {fetchImpl} rows={abrechnung.vertraege}/>
    <WalterZaehlerList
        ablesedatum={new Date(abrechnung.zeitraum.abrechnungsende)}
        title="Aufteilung nach Verbrauch (n. Verb., verteilt nach Verbrauch laut Zählerständen)"
        {fetchImpl}
        rows={abrechnung.zaehler}/>
</WalterLinks>
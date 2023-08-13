<script lang="ts">
    import type { WalterBetriebskostenabrechnungEntry } from "$walter/types";
    import { StructuredList, StructuredListCell, StructuredListRow, Tile } from "carbon-components-svelte";
    import { WalterLinks, WalterVertraege, WalterWohnungen, WalterZaehlerList } from "..";

    export let fetchImpl: typeof fetch;
    export let abrechnung: WalterBetriebskostenabrechnungEntry;

    const rechnungen = abrechnung.abrechnungseinheiten.flatMap(einheit => einheit.rechnungen);
    const nWF = rechnungen.some(rechnung => rechnung.schluessel === "n. WF");
    const nNE = rechnungen.some(rechnung => rechnung.schluessel === "n. NE");
    const nNF = rechnungen.some(rechnung => rechnung.schluessel === "n. NF");
    const nPs = rechnungen.some(rechnung => rechnung.schluessel === "n. Pers.");
    const nVb = rechnungen.some(rechnung => rechnung.schluessel === "n. Verb.");
    const dir = rechnungen.some(rechnung => rechnung.anteil === abrechnung.zeitraum.zeitanteil);

    const wohnungSchluessel = [];
    if (nWF) wohnungSchluessel.push("n. WF");
    if (nNE) wohnungSchluessel.push("n. NE");
    if (nNF) wohnungSchluessel.push("n. NF");
    const wohnungText = wohnungSchluessel.join(", ");
</script>

<Tile>
    <h4>Genutzte Umlageschlüssel in dieser Abrechnung: </h4>
</Tile>
<StructuredList condensed style="margin-bottom: 0em">
    <StructuredListRow>
        <StructuredListCell head>Umlageschlüssel</StructuredListCell>
        <StructuredListCell head>Bedeutung</StructuredListCell>
    </StructuredListRow>
    {#if dir}
    <StructuredListRow>
        <StructuredListCell>Direkt</StructuredListCell>
        <StructuredListCell>Direkte Zuordnung</StructuredListCell>
    </StructuredListRow>
    {/if}
    {#if nWF}
    <StructuredListRow>
        <StructuredListCell>n. WF</StructuredListCell>
        <StructuredListCell>nach Wohnfläche in m²</StructuredListCell>
    </StructuredListRow>
    {/if}
    {#if nNF}
    <StructuredListRow>
        <StructuredListCell>n. NF</StructuredListCell>
        <StructuredListCell>nach Nutzfläche in m²</StructuredListCell>
    </StructuredListRow>
    {/if}
    {#if nNE}
    <StructuredListRow>
        <StructuredListCell>n. NE</StructuredListCell>
        <StructuredListCell>nach Anzahl der Wohn-/Nutzeinheiten</StructuredListCell>
    </StructuredListRow>
    {/if}
    {#if nPs}
    <StructuredListRow>
        <StructuredListCell>n. Pers.</StructuredListCell>
        <StructuredListCell>nach Personenzahl/Anzahl der Bewohner</StructuredListCell>
    </StructuredListRow>
    {/if}
    {#if nVb}
    <StructuredListRow>
        <StructuredListCell>n. Verb.</StructuredListCell>
        <StructuredListCell>nach Verbrauch</StructuredListCell>
    </StructuredListRow>
    {/if}
</StructuredList>

<WalterLinks>
    {#if nWF || nNF || nNE}
    <WalterWohnungen
        title="Aufteilung auf Wohnungen - für Umlageschlüssel {wohnungText}"
        {fetchImpl}
        rows={abrechnung.wohnungen}/>
    {/if}
    {#if nPs}
    <WalterVertraege
        title="Aufteilung auf Verträge - für Umlageschlüssel n. Pers."
        {fetchImpl}
        rows={abrechnung.vertraege}/>
    {/if}
    {#if nVb}
    <WalterZaehlerList
        title="Aufteilung auf Zähler - für Umlageschlüssel n. Verb."
        ablesedatum={new Date(abrechnung.zeitraum.abrechnungsende)}
        {fetchImpl}
        rows={abrechnung.zaehler}/>
    {/if}
</WalterLinks>
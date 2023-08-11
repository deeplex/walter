<script lang="ts">
    import { convertDateGerman, convertPercent } from "$walter/services/utils";
    import type { WalterVerbrauchAnteil } from "$walter/types/WalterBetriebskostenabrechnung.type";
    import { StructuredListCell, StructuredListRow } from "carbon-components-svelte";

    export let verbrauch: WalterVerbrauchAnteil;
    export let beginn: Date;
    export let ende: Date;

    const reference = Object.values(verbrauch.alleZaehler)[0][0].zaehler.filter;
</script>
{#each Object.entries(verbrauch.dieseZaehler) as [unit, verbrauchUnit]}
    {#each verbrauchUnit as v}
        <StructuredListRow>
            <StructuredListCell>
                {v.delta}{v.zaehler.filter} / {verbrauch.alleVerbrauch[unit]}{v.zaehler.filter}
            </StructuredListCell>
            <StructuredListCell></StructuredListCell>
            <StructuredListCell>{v.zaehler.text}</StructuredListCell>
    </StructuredListRow>
    {/each}
    <StructuredListRow>
        <StructuredListCell head>
            {verbrauch.dieseVerbrauch[unit]}{reference} / {verbrauch.alleVerbrauch[unit]}{reference}
        </StructuredListCell>
        <StructuredListCell>
            {convertDateGerman(new Date(beginn))} - {convertDateGerman(new Date(ende))}
        </StructuredListCell>
        <StructuredListCell head>{verbrauch.umlage.text}</StructuredListCell>
        <StructuredListCell>{convertPercent(verbrauch.anteil[unit])}</StructuredListCell>
    </StructuredListRow>
{/each}
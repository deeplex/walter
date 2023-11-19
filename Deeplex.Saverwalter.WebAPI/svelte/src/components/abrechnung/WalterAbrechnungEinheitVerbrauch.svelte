<script lang="ts">
    import {
        convertDateGerman,
        convertFixed2,
        convertPercent
    } from '$walter/services/utils';
    import type { WalterVerbrauchAnteil } from '$walter/types/WalterBetriebskostenabrechnung.type';
    import {
        StructuredListCell,
        StructuredListRow
    } from 'carbon-components-svelte';
    import { WalterLink } from '..';

    export let verbrauch: WalterVerbrauchAnteil;
    export let beginn: Date;
    export let ende: Date;
</script>

{#each Object.entries(verbrauch.dieseZaehler) as [unit, verbrauchUnit]}
    {#each verbrauchUnit as v}
        <StructuredListRow>
            <StructuredListCell>
                {convertFixed2(v.delta)}
                {v.zaehler.filter} / {convertFixed2(
                    verbrauch.alleVerbrauch[unit]
                )}
                {v.zaehler.filter}
            </StructuredListCell>
            <StructuredListCell></StructuredListCell>
            <StructuredListCell>
                <WalterLink href={`zaehler/${v.zaehler.id}`}>
                    {v.zaehler.text}
                </WalterLink>
            </StructuredListCell>
        </StructuredListRow>
    {/each}
    <StructuredListRow>
        <StructuredListCell head>
            {convertFixed2(verbrauch.dieseVerbrauch[unit])}
            {verbrauchUnit[0]?.zaehler?.filter || '?'} / {convertFixed2(
                verbrauch.alleVerbrauch[unit]
            )}
            {verbrauchUnit[0]?.zaehler?.filter || '?'}
        </StructuredListCell>
        <StructuredListCell>
            {convertDateGerman(new Date(beginn))} - {convertDateGerman(
                new Date(ende)
            )}
        </StructuredListCell>
        <StructuredListCell head>
            <WalterLink href={`umlagen/${verbrauch.umlage.id}`}>
                {verbrauch.umlage.text}
            </WalterLink>
        </StructuredListCell>
        <StructuredListCell
            >{convertPercent(verbrauch.anteil[unit])}</StructuredListCell
        >
    </StructuredListRow>
{/each}

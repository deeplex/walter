<script lang="ts">
    import {
        WalterAbrechnungEinheit,
        WalterAbrechnungGruppe,
        WalterAbrechnungResultat,
        WalterDatePicker,
        WalterZaehlerList
    } from '$walter/components';
    import { convertDateGerman, convertEuro } from '$walter/services/utils';
    import type { WalterBetriebskostenabrechnungKostengruppenEntry } from '$walter/types';
    import { Column, Loading, Row, TextInput, Tile, Truncate } from 'carbon-components-svelte';

    export let abrechnung: WalterBetriebskostenabrechnungKostengruppenEntry;
    export let fetchImpl: typeof fetch;
</script>

<Tile style="margin-top: 2rem">
{#await abrechnung}
    <div style="width:100%; display: flex; justify-content: center">
        <Loading withOverlay={false} />
    </div>
{:then}
    <Row>
        <TextInput
            placeholder="Nutzungsbeginn"
            labelText="Nutzungsbeginn"
            readonly
            value={convertDateGerman(new Date(abrechnung.nutzungsbeginn))}
        />
        <TextInput
            placeholder="Nutzungsende"
            labelText="Nutzungsende"
            readonly
            value={convertDateGerman(new Date(abrechnung.nutzungsende))}
        />
    </Row>

    <WalterAbrechnungResultat entry={abrechnung} />
    {#each abrechnung.kostengruppen as gruppe}
        <hr />
        <WalterAbrechnungEinheit
            entry={gruppe}
            abrechnungstage={abrechnung.abrechnungszeitspanne}
        />
        <WalterAbrechnungGruppe
            rows={gruppe.kostenpunkte}
            year={abrechnung.jahr}
        />
        <Tile>
            <h5 style="display: flex; justify-content: center">
                Zwischensumme: {convertEuro(gruppe.betragKalteNebenkosten)}
            </h5>
        </Tile>
        {/each}
        {#if abrechnung.zaehler.length}
            <hr />
            <Tile>
                <h4>Zähler</h4>
            </Tile>
            <WalterZaehlerList {fetchImpl} rows={abrechnung.zaehler} />
        {/if}
        <div style="margin: 3em"/>
{/await}
</Tile>

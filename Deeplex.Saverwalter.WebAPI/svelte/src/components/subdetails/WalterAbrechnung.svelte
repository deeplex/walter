<script lang="ts">
    import {
        WalterAbrechnungEinheitKalt,
        WalterAbrechnungEinheitWarm,
        WalterAbrechnungGruppe,
        WalterAbrechnungHeizkosten,
        WalterAbrechnungNotes,
        WalterAbrechnungResultat,
    } from '$walter/components';
    import { convertDateGerman, convertEuro } from '$walter/services/utils';
    import type { WalterBetriebskostenabrechnungEntry } from '$walter/types';
    import { Column, Loading, Row, TextInput, Tile } from 'carbon-components-svelte';
    import WalterAbrechnungNebenkosten from './WalterAbrechnungNebenkosten.svelte';

    export let abrechnung: WalterBetriebskostenabrechnungEntry;
    export let title: string | undefined;
    export let fetchImpl: typeof fetch;
</script>

<Tile style="margin-top: 2rem">
{#await abrechnung}
    <div style="width:100%; display: flex; justify-content: center">
        <Loading withOverlay={false} />
    </div>
{:then}
    
    <Row>
        <Column>
            {#if title}
                <Tile>
                    <h4>{title} - {abrechnung.zeitraum.jahr}</h4>
                </Tile>
            {/if}
        </Column>
        <Column sm={1}>
            <Tile>
                <h4>Resultat: {convertEuro(Math.abs(abrechnung.result))}</h4>
                <p>{abrechnung.result > 0 ? "bekommt der Mieter" : "bekommt der Vermieter"}</p>
            </Tile>
        </Column>
    </Row>

    <Row>
        {#if abrechnung.notes.length > 0}
            <WalterAbrechnungNotes {abrechnung}/>
        {/if}
    </Row>

    <Row>
        <TextInput
            placeholder="Nutzungsbeginn"
            labelText="Nutzungsbeginn"
            readonly
            value={convertDateGerman(new Date(abrechnung.zeitraum.nutzungsbeginn))}
        />
        <TextInput
            placeholder="Nutzungsende"
            labelText="Nutzungsende"
            readonly
            value={convertDateGerman(new Date(abrechnung.zeitraum.nutzungsende))}
        />
    </Row>

    <WalterAbrechnungNebenkosten {fetchImpl} {abrechnung} />

    <Tile>
        <h4>Kalte Nebenkosten</h4>
    </Tile>
    
    {#each abrechnung.abrechnungseinheiten as einheit}
        <hr />
        <WalterAbrechnungEinheitKalt
            einheit={einheit}
            zeitraum={abrechnung.zeitraum}
        />
        <WalterAbrechnungGruppe
            rows={einheit.rechnungen}
            year={abrechnung.zeitraum.jahr}
        />
        <Tile>
            <h5 style="display: flex; justify-content: center">
                Zwischensumme: {convertEuro(einheit.betragKalt)}
            </h5>
        </Tile>
    {/each}
    {#if abrechnung.abrechnungseinheiten.some(e => e.heizkostenberechnungen.length)}
    <Tile><h4>Warme Nebenkosten</h4></Tile>
    {/if}
    {#each abrechnung.abrechnungseinheiten as einheit}
        {#if einheit.heizkostenberechnungen.length}
            <WalterAbrechnungEinheitWarm
                {einheit}
                zeitraum={abrechnung.zeitraum}
            />
            {#each einheit.heizkostenberechnungen as heizkosten}
                <WalterAbrechnungHeizkosten {heizkosten}/>
            {/each}
        {/if}
    {/each}

    <Tile><h4>Gesamtergebnis der Abrechnung:</h4></Tile>
    <WalterAbrechnungResultat entry={abrechnung} />
{/await}
</Tile>

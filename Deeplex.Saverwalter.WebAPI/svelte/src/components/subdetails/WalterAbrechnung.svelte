<script lang="ts">
    import {
        WalterAbrechnungEinheit,
        WalterAbrechnungGruppe,
        WalterAbrechnungHeizkosten,
        WalterAbrechnungNotes,
        WalterAbrechnungResultat,
        WalterZaehlerList
    } from '$walter/components';
    import { convertDateGerman, convertEuro } from '$walter/services/utils';
    import type { WalterBetriebskostenabrechnungEntry } from '$walter/types';
    import { Column, Loading, Row, TextInput, Tile } from 'carbon-components-svelte';

    export let abrechnung: WalterBetriebskostenabrechnungEntry;
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
            {#if abrechnung.notes.length > 0}
                <WalterAbrechnungNotes {abrechnung}/>
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

    <Tile>
        <h4>Kalte Nebenkosten</h4>
    </Tile>
    {#each abrechnung.abrechnungseinheiten as einheit}
        <hr />
        <WalterAbrechnungEinheit
            entry={einheit}
            abrechnungstage={abrechnung.zeitraum.abrechnungszeitraum}
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
    <Tile><h4>Warme Nebenkosten</h4></Tile>
    {#each abrechnung.abrechnungseinheiten as einheit}
        {#if einheit.heizkostenberechnungen.length}
            <WalterAbrechnungEinheit
                entry={einheit}
                abrechnungstage={abrechnung.zeitraum.abrechnungszeitraum}
            />
            {#each einheit.heizkostenberechnungen as heizkosten}
                <WalterAbrechnungHeizkosten {heizkosten}/>
            {/each}
        {/if}
    {/each}

    <Tile><h4>Gesamtergebnis der Abrechnung:</h4></Tile>
    <WalterAbrechnungResultat entry={abrechnung} />

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

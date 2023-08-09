<script lang="ts">
    import { convertM2 } from '$walter/services/utils';
    import type { WalterAbrechnungseinheit } from '$walter/types';
    import type {  WalterZeitraum } from '$walter/types/WalterBetriebskostenabrechnung.type';
    import {
        Row,
        StructuredList,
        StructuredListBody,
        StructuredListCell,
        StructuredListHead,
        StructuredListRow,
        Tile
    } from 'carbon-components-svelte';
    import WalterAbrechnungEinheitPart from './WalterAbrechnungEinheitPart.svelte';
    import WalterAbrechnungEinheitHead from './WalterAbrechnungEinheitHead.svelte';
    import WalterAbrechnungEinheitVerbrauch from './WalterAbrechnungEinheitVerbrauch.svelte';

    export let einheit: WalterAbrechnungseinheit;
    export let zeitraum: WalterZeitraum;
</script>

<Tile>
    <h4>Abrechnungseinheit: {einheit.bezeichnung}</h4>
</Tile>

<Row>
<StructuredList style="margin: 2em" condensed>
    <StructuredListHead>
        <StructuredListRow>
            <StructuredListCell head>Nutzeinheiten</StructuredListCell>
            <StructuredListCell head>Wohnfläche</StructuredListCell>
            <StructuredListCell head>Nutzfläche</StructuredListCell>
            <StructuredListCell head>Bewohner</StructuredListCell>
            <StructuredListCell head>Nutzungsintervall</StructuredListCell>
            <StructuredListCell head>Tage</StructuredListCell>
        </StructuredListRow>
    </StructuredListHead>
    <StructuredListBody>
        {#each einheit.personenZeitanteil as intervall, index}
            <StructuredListRow>
                <StructuredListCell
                    >{!index
                        ? einheit.gesamtEinheiten
                        : ''}</StructuredListCell
                >
                <StructuredListCell
                    >{!index
                        ? convertM2(einheit.gesamtWohnflaeche)
                        : ''}</StructuredListCell
                >
                <StructuredListCell
                    >{!index
                        ? convertM2(einheit.gesamtNutzflaeche)
                        : ''}</StructuredListCell
                >
                <StructuredListCell
                    >{intervall.gesamtPersonenzahl}</StructuredListCell
                >
                <StructuredListCell
                    >{new Date(intervall.beginn).toLocaleDateString(
                        'de-DE'
                    )} - {new Date(intervall.ende).toLocaleDateString(
                        'de-DE'
                    )}</StructuredListCell
                >
                <StructuredListCell
                    >{intervall.tage} / {zeitraum.nutzungszeitraum}</StructuredListCell
                >
            </StructuredListRow>
        {/each}
    </StructuredListBody>
</StructuredList>
</Row>

<Row>
    <StructuredList condensed style="margin: 2em">
        <WalterAbrechnungEinheitHead />
        <StructuredListBody>
            <StructuredListRow>
                <StructuredListCell head>Bei Umlage nach Nutzfläche (n. NF)</StructuredListCell>
            </StructuredListRow>
            <WalterAbrechnungEinheitPart
                beginn={zeitraum.nutzungsbeginn}
                ende={zeitraum.nutzungsende}
                value={-1000}
                gesamt={einheit.gesamtNutzflaeche}
                anteil={einheit.nFZeitanteil}
                tage={zeitraum.nutzungszeitraum}
                />
           
                <StructuredListRow>
                <StructuredListCell head>Bei Umlage nach Verbrauch (n. Verb.)</StructuredListCell>
                <StructuredListCell></StructuredListCell>
                <StructuredListCell head>Zählernummer / Typ</StructuredListCell>
                </StructuredListRow>
                    {#each einheit.verbrauchAnteil.filter(e => e.umlage.text === "Heizkosten") as verbrauch}
                        <WalterAbrechnungEinheitVerbrauch 
                            {verbrauch}
                            beginn={zeitraum.nutzungsbeginn}
                            ende={zeitraum.nutzungsende} />
                    {/each}
        </StructuredListBody>
    </StructuredList>
</Row>

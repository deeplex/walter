<script lang="ts">
    import { convertM2 } from '$walter/services/utils';
    import type { WalterAbrechnungseinheit } from '$walter/types';
    import type {  WalterZeitraum } from '$walter/types/WalterBetriebskostenabrechnung.type';
    import {
        WalterAbrechnungEinheitHead,
        WalterAbrechnungEinheitPart,
        WalterAbrechnungEinheitVerbrauch
    } from '$walter/components'
    import {
        Row,
        StructuredList,
        StructuredListBody,
        StructuredListCell,
        StructuredListHead,
        StructuredListRow,
        Tile
    } from 'carbon-components-svelte';

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
        {#if einheit.rechnungen.some(r => r.schluessel == "n. WF")}
        <StructuredListRow>
            <StructuredListCell head>Bei Umlage nach Wohnfläche (n. WF)</StructuredListCell>
        </StructuredListRow>
        <WalterAbrechnungEinheitPart
            beginn={zeitraum.nutzungsbeginn}
            ende={zeitraum.nutzungsende}
            value={convertM2(einheit.gesamtWohnflaeche * einheit.wfZeitanteil / zeitraum.zeitanteil)}
            gesamt={convertM2(einheit.gesamtWohnflaeche)}
            anteil={einheit.wfZeitanteil}
            tage={zeitraum.nutzungszeitraum}
            />
        {/if}
        {#if einheit.rechnungen.some(r => r.schluessel == "n. NF")}
            <StructuredListRow>
                <StructuredListCell head>Bei Umlage nach Nutzfläche (n. NF)</StructuredListCell>
            </StructuredListRow>
            <WalterAbrechnungEinheitPart
                beginn={zeitraum.nutzungsbeginn}
                ende={zeitraum.nutzungsende}
                value={convertM2(einheit.gesamtNutzflaeche * einheit.nfZeitanteil / zeitraum.zeitanteil)}
                gesamt={convertM2(einheit.gesamtNutzflaeche)}
                anteil={einheit.nfZeitanteil}
                tage={zeitraum.nutzungszeitraum}
                />
        {/if}
        {#if einheit.rechnungen.some(r => r.schluessel == "n. NE")}
            <StructuredListRow>
                <StructuredListCell head>Bei Umlage nach Nutzeinheiten (n. NE)</StructuredListCell>
            </StructuredListRow>
            <WalterAbrechnungEinheitPart
                beginn={zeitraum.nutzungsbeginn}
                ende={zeitraum.nutzungsende}
                value={einheit.gesamtEinheiten * einheit.neZeitanteil / zeitraum.zeitanteil}
                gesamt={einheit.gesamtEinheiten}
                anteil={einheit.neZeitanteil}
                tage={zeitraum.nutzungszeitraum}
                />
        {/if}
        {#if einheit.rechnungen.some(r => r.schluessel == "n. Pers.")}
            <StructuredListRow>
                <StructuredListCell head>Bei Umlage nach Personenzahl (n. Pers.)</StructuredListCell>
            </StructuredListRow>
            {#each einheit.personenZeitanteil as personen}
                <WalterAbrechnungEinheitPart
                    beginn={personen.beginn}
                    ende={personen.ende}
                    value={personen.personenzahl}
                    gesamt={personen.gesamtPersonenzahl}
                    anteil={personen.anteil}
                    tage={personen.tage}
                />
            {/each}
        {/if}
        {#if einheit.rechnungen.some(r => r.schluessel == "n. Verb.")}
            <StructuredListRow>
                <StructuredListCell head>Bei Umlage nach Verbrauch (n. Verb.)</StructuredListCell>
                <StructuredListCell></StructuredListCell>
                <StructuredListCell head>Zählernummer / Typ</StructuredListCell>
            </StructuredListRow>
            {#each einheit.verbrauchAnteil.filter(e => e.umlage.text !== "Heizkosten") as verbrauch}
                <WalterAbrechnungEinheitVerbrauch
                    {verbrauch}
                    beginn={zeitraum.nutzungsbeginn}
                    ende={zeitraum.nutzungsende} />
            {/each}
        {/if}
        </StructuredListBody>
    </StructuredList>
</Row>

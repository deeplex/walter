<!-- Copyright (C) 2023-2024  Kai Lawrence -->
<!--
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
-->

<script lang="ts">
    import { convertM2 } from '$walter/services/utils';
    import type { WalterAbrechnungseinheit } from '$walter/types';
    import type { WalterZeitraum } from '$walter/types/WalterBetriebskostenabrechnung.type';
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
                    <StructuredListCell>
                        {!index ? einheit.gesamtEinheiten : ''}
                    </StructuredListCell>
                    <StructuredListCell>
                        {!index ? convertM2(einheit.gesamtWohnflaeche) : ''}
                    </StructuredListCell>
                    <StructuredListCell>
                        {!index ? convertM2(einheit.gesamtNutzflaeche) : ''}
                    </StructuredListCell>
                    <StructuredListCell>
                        {intervall.gesamtPersonenzahl}
                    </StructuredListCell>
                    <StructuredListCell>
                        {new Date(intervall.beginn).toLocaleDateString('de-DE')}
                        -
                        {new Date(intervall.ende).toLocaleDateString('de-DE')}
                    </StructuredListCell>
                    <StructuredListCell>
                        {intervall.tage} / {zeitraum.nutzungszeitraum}
                    </StructuredListCell>
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
                <StructuredListCell head
                    >Bei Umlage nach Nutzfläche (n. NF)</StructuredListCell
                >
            </StructuredListRow>
            <WalterAbrechnungEinheitPart
                beginn={zeitraum.nutzungsbeginn}
                ende={zeitraum.nutzungsende}
                value={convertM2(
                    (einheit.gesamtNutzflaeche * einheit.nfZeitanteil) /
                        zeitraum.zeitanteil
                )}
                gesamt={convertM2(einheit.gesamtNutzflaeche)}
                anteil={einheit.nfZeitanteil}
                tage={zeitraum.nutzungszeitraum}
            />

            <StructuredListRow>
                <StructuredListCell head
                    >Bei Umlage nach Verbrauch (n. Verb.)</StructuredListCell
                >
                <StructuredListCell></StructuredListCell>
                <StructuredListCell head>Zählernummer / Typ</StructuredListCell>
            </StructuredListRow>
            {#each einheit.verbrauchAnteil.filter((e) => e.umlage.text === 'Heizkosten') as verbrauch}
                <WalterAbrechnungEinheitVerbrauch
                    {verbrauch}
                    beginn={zeitraum.nutzungsbeginn}
                    ende={zeitraum.nutzungsende}
                />
            {/each}
        </StructuredListBody>
    </StructuredList>
</Row>

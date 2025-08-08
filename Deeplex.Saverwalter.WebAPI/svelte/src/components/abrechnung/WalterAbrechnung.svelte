<!-- Copyright (C) 2023-2025  Kai Lawrence -->
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
    import {
        WalterAbrechnungEinheitKalt,
        WalterAbrechnungEinheitWarm,
        WalterAbrechnungGruppe,
        WalterAbrechnungHeizkosten,
        WalterAbrechnungNotes,
        WalterAbrechnungResultat,
        WalterAltesAbrechnungsresultat,
        WalterMieten,
        WalterAbrechnungNebenkosten,
        WalterKontakte
    } from '$walter/components';
    import {
        convertDateCanadian,
        convertDateGerman,
        convertEuro
    } from '$walter/services/utils';
    import type { WalterBetriebskostenabrechnungEntry } from '$walter/types';
    import {
        Accordion,
        Column,
        Loading,
        Row,
        TextInput,
        Tile
    } from 'carbon-components-svelte';
    import { WalterKontaktEntry, type WalterMieteEntry } from '$walter/lib';
    import WalterData from '../data/WalterData.svelte';
    import WalterAbrechnungsresultate from '../lists/WalterAbrechnungsresultate.svelte';

    export let abrechnung: WalterBetriebskostenabrechnungEntry;
    export let title: string | undefined;
    export let fetchImpl: typeof fetch;

    const lastMiete = abrechnung.mieten.length
        ? abrechnung.mieten[abrechnung.mieten.length - 1]
        : undefined;
    const dateMiete = lastMiete
        ? new Date(lastMiete.betreffenderMonat)
        : new Date();
    dateMiete.setDate(
        dateMiete.getDate() +
            new Date(dateMiete.getFullYear(), dateMiete.getMonth(), 0).getDate()
    );
    if (dateMiete < abrechnung.zeitraum.abrechnungsbeginn) {
        dateMiete.setDate(abrechnung.zeitraum.abrechnungsbeginn.getDate());
    }
    const mietEntry: Partial<WalterMieteEntry> = {
        vertrag: abrechnung.vertrag,
        zahlungsdatum: convertDateCanadian(new Date()),
        betrag: lastMiete?.betrag,
        betreffenderMonat: convertDateCanadian(dateMiete)
    };

    const mieter = WalterKontaktEntry.GetAll<WalterKontaktEntry>(
        fetchImpl
    ).then((res) =>
        res.filter((kontakt) =>
            abrechnung.mieter.some((m) => m.id === kontakt.id)
        )
    );
</script>

<Tile style="margin-top: 2rem">
    {#await abrechnung}
        <div style="width:100%; display: flex; justify-content: center">
            <Loading withOverlay={false} />
        </div>
    {:then}
        <WalterAltesAbrechnungsresultat resultat={abrechnung.resultat} />
        <hr />
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
                    <h4>
                        Resultat: {convertEuro(Math.abs(abrechnung.result))}
                    </h4>
                    <p>
                        {abrechnung.result > 0
                            ? 'bekommt der Mieter'
                            : 'bekommt der Vermieter'}
                    </p>
                </Tile>
            </Column>
        </Row>

        <Row>
            {#if abrechnung.notes.length > 0}
                <WalterAbrechnungNotes {abrechnung} />
            {/if}
        </Row>

        <Row>
            <TextInput
                placeholder="Nutzungsbeginn"
                labelText="Nutzungsbeginn"
                readonly
                value={convertDateGerman(
                    new Date(abrechnung.zeitraum.nutzungsbeginn)
                )}
            />
            <TextInput
                placeholder="Nutzungsende"
                labelText="Nutzungsende"
                readonly
                value={convertDateGerman(
                    new Date(abrechnung.zeitraum.nutzungsende)
                )}
            />
        </Row>

        <Accordion>
            <WalterData {abrechnung} />
            {#await mieter then mieter}
                <WalterKontakte title="Mieter" rows={mieter} {fetchImpl} />
            {/await}
            <WalterMieten
                entry={mietEntry}
                title="Gezahlte Mieten ({abrechnung.gezahltMiete}€)"
                rows={abrechnung.mieten}
            />
        </Accordion>

        <WalterAbrechnungNebenkosten {fetchImpl} {abrechnung} />

        <Tile>
            <h4>Kalte Nebenkosten</h4>
        </Tile>

        {#each abrechnung.abrechnungseinheiten as einheit}
            <hr />
            <WalterAbrechnungEinheitKalt
                {einheit}
                zeitraum={abrechnung.zeitraum}
            />
            <WalterAbrechnungGruppe
                {fetchImpl}
                rows={einheit.rechnungen}
                year={abrechnung.zeitraum.jahr}
            />
            <Tile>
                <h5 style="display: flex; justify-content: center">
                    Zwischensumme: {convertEuro(einheit.betragKalt)}
                </h5>
            </Tile>
        {/each}
        {#if abrechnung.abrechnungseinheiten.some((e) => e.heizkostenberechnungen.length)}
            <Tile><h4>Warme Nebenkosten</h4></Tile>
        {/if}
        {#each abrechnung.abrechnungseinheiten as einheit}
            {#if einheit.heizkostenberechnungen.length}
                <WalterAbrechnungEinheitWarm
                    {einheit}
                    zeitraum={abrechnung.zeitraum}
                />
                {#each einheit.heizkostenberechnungen as heizkosten}
                    <WalterAbrechnungHeizkosten {heizkosten} />
                {/each}
            {/if}
        {/each}

        <Tile><h4>Gesamtergebnis der Abrechnung:</h4></Tile>
        <WalterAbrechnungResultat entry={abrechnung} />
    {/await}
</Tile>

<!-- Copyright (C) 2023-2026  Kai Lawrence -->
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
    import type { NkZeileInfo, ZaehlerVerbrauchInfo } from './AbrechnungslaufTypes';
    import { convertEuro, convertPercent } from '$walter/services/utils';
    import {
        StructuredList,
        StructuredListBody,
        StructuredListCell,
        StructuredListHead,
        StructuredListRow,
        Tile
    } from 'carbon-components-svelte';

    export let zeilen: NkZeileInfo[];
    export let vertragId: number;
    export let nutzungVon: string;
    export let nutzungBis: string;
    export let nutzungstage: number;
    export let abrechnungstage: number;

    const formatDate = (d: string) => {
        const [y, m, day] = d.split('-');
        return `${day}.${m}.${y}`;
    };
    const nutzungsIntervall = `${formatDate(nutzungVon)} – ${formatDate(nutzungBis)}`;
    const zeitanteil = nutzungstage / abrechnungstage;

    type HkvoZeile = {
        zeile: NkZeileInfo;
        gesamtbetrag: number;
        meinBetrag: number;
        para9_2: number;
        p7: number;
        p8: number;
        heizVerbrauchAnteil: number | null;
        wwVerbrauchAnteil: number | null;
        heizZaehler: ZaehlerVerbrauchInfo[];
        wwZaehler: ZaehlerVerbrauchInfo[];
        wfZeitanteil: number;
    };

    const hkvoZeilen: HkvoZeile[] = zeilen
        .filter((z) => z.para9_2 != null)
        .map((z) => {
            const anteil = z.anteile.find((a) => a.vertragId === vertragId);
            const wfZeitanteil = anteil?.anteilFaktor ?? 0;
            return {
                zeile: z,
                gesamtbetrag: z.betrag,
                meinBetrag: z.betrag * (anteil?.anteilFaktor ?? 0),
                para9_2: z.para9_2!,
                p7: z.p7!,
                p8: z.p8!,
                heizVerbrauchAnteil: anteil?.heizVerbrauchAnteil ?? null,
                wwVerbrauchAnteil: anteil?.wwVerbrauchAnteil ?? null,
                heizZaehler: anteil?.heizZaehler ?? [],
                wwZaehler: anteil?.wwZaehler ?? [],
                wfZeitanteil,
            };
        });

    // Dedupliziert nach umlageId: eine HKVO-Zeile pro Rechnung, aber §9(2) einmal pro Umlage zeigen
    const gezeigteUmlageIds = new Set<number>();

    const betragWarm = hkvoZeilen.reduce((s, hz) => {
        const heizBetrag = hz.gesamtbetrag * (1 - hz.para9_2);
        const wwBetrag = hz.gesamtbetrag * hz.para9_2;
        const vbHeizAnteil = hz.heizVerbrauchAnteil != null ? hz.p7 * hz.heizVerbrauchAnteil : 0;
        const wfHeizAnteil = (1 - hz.p7) * hz.wfZeitanteil;
        const vbWWAnteil = hz.wwVerbrauchAnteil != null ? hz.p8 * hz.wwVerbrauchAnteil : 0;
        const wfWWAnteil = (1 - hz.p8) * hz.wfZeitanteil;
        return s + heizBetrag * (vbHeizAnteil + wfHeizAnteil) + wwBetrag * (vbWWAnteil + wfWWAnteil);
    }, 0);
</script>

<Tile><h4>Warme Betriebskosten (HKVO)</h4></Tile>

{#each hkvoZeilen as hz}
    {@const showP9 = !gezeigteUmlageIds.has(hz.zeile.umlageId)}
    {#if showP9}
        {@const _add = gezeigteUmlageIds.add(hz.zeile.umlageId)}
        <!-- §9(2) Formel-Anzeige -->
        <Tile>
            <p>
                <strong>§9 Abs. 2 HKVO</strong> – Warmwasseranteil:
                2,5 × V/Q × (t<sub>w</sub> − 10 °C) = <strong>{convertPercent(hz.para9_2)}</strong>
            </p>
        </Tile>
    {/if}

    <!-- §7 Heizkosten -->
    {@const heizBetrag = hz.gesamtbetrag * (1 - hz.para9_2)}
    {@const wfHeizAnteil = (1 - hz.p7) * hz.wfZeitanteil}
    {@const vbHeizAnteil = hz.heizVerbrauchAnteil != null ? hz.p7 * hz.heizVerbrauchAnteil : 0}

    <StructuredList condensed>
        <StructuredListHead>
            <StructuredListRow>
                <StructuredListCell head>
                    §7 Heizung ({convertPercent(1 - hz.para9_2)} von {convertEuro(hz.gesamtbetrag)})
                </StructuredListCell>
                <StructuredListCell head>Anteil</StructuredListCell>
                <StructuredListCell head>Nutzungsintervall</StructuredListCell>
                <StructuredListCell head>Betrag</StructuredListCell>
                <StructuredListCell head>Ihr Anteil</StructuredListCell>
                <StructuredListCell head>Ihr Verbrauch</StructuredListCell>
                <StructuredListCell head>Ihre Kosten</StructuredListCell>
            </StructuredListRow>
        </StructuredListHead>
        <StructuredListBody>
            {#if hz.heizZaehler.length > 0}
                {@const heizGesamt = hz.heizZaehler.reduce((s, z) => s + z.verbrauch, 0)}
                <StructuredListRow>
                    <StructuredListCell>nach Verbrauch</StructuredListCell>
                    <StructuredListCell>{convertPercent(hz.p7)}</StructuredListCell>
                    <StructuredListCell>{nutzungsIntervall}</StructuredListCell>
                    <StructuredListCell>{convertEuro(heizBetrag * hz.p7)}</StructuredListCell>
                    <StructuredListCell>{convertPercent(vbHeizAnteil)}</StructuredListCell>
                    <StructuredListCell>
                        {heizGesamt.toFixed(2)} kWh
                        {#if hz.zeile.gesamtWaerme != null}
                            / {hz.zeile.gesamtWaerme.toFixed(2)} kWh
                        {/if}
                    </StructuredListCell>
                    <StructuredListCell>{convertEuro(heizBetrag * vbHeizAnteil)}</StructuredListCell>
                </StructuredListRow>
                {#each hz.heizZaehler as z}
                    <StructuredListRow>
                        <StructuredListCell style="padding-left: 2rem">Zähler {z.kennnummer}</StructuredListCell>
                        <StructuredListCell></StructuredListCell>
                        <StructuredListCell></StructuredListCell>
                        <StructuredListCell></StructuredListCell>
                        <StructuredListCell></StructuredListCell>
                        <StructuredListCell>{z.verbrauch.toFixed(2)} {z.einheit}</StructuredListCell>
                        <StructuredListCell></StructuredListCell>
                    </StructuredListRow>
                {/each}
            {/if}
            <StructuredListRow>
                <StructuredListCell>nach Wohnfläche</StructuredListCell>
                <StructuredListCell>{convertPercent(1 - hz.p7)}</StructuredListCell>
                <StructuredListCell>{nutzungsIntervall}</StructuredListCell>
                <StructuredListCell>{convertEuro(heizBetrag * (1 - hz.p7))}</StructuredListCell>
                <StructuredListCell>{convertPercent(wfHeizAnteil)}</StructuredListCell>
                <StructuredListCell></StructuredListCell>
                <StructuredListCell>{convertEuro(heizBetrag * wfHeizAnteil)}</StructuredListCell>
            </StructuredListRow>
        </StructuredListBody>
    </StructuredList>

    <!-- §8 Warmwasser -->
    {@const wwBetrag = hz.gesamtbetrag * hz.para9_2}
    {@const wfWWAnteil = (1 - hz.p8) * hz.wfZeitanteil}
    {@const vbWWAnteil = hz.wwVerbrauchAnteil != null ? hz.p8 * hz.wwVerbrauchAnteil : 0}

    <StructuredList condensed>
        <StructuredListHead>
            <StructuredListRow>
                <StructuredListCell head>
                    §8 Warmwasser ({convertPercent(hz.para9_2)} von {convertEuro(hz.gesamtbetrag)})
                </StructuredListCell>
                <StructuredListCell head>Anteil</StructuredListCell>
                <StructuredListCell head>Nutzungsintervall</StructuredListCell>
                <StructuredListCell head>Betrag</StructuredListCell>
                <StructuredListCell head>Ihr Anteil</StructuredListCell>
                <StructuredListCell head>Ihr Verbrauch</StructuredListCell>
                <StructuredListCell head>Ihre Kosten</StructuredListCell>
            </StructuredListRow>
        </StructuredListHead>
        <StructuredListBody>
            {#if hz.wwZaehler.length > 0}
                {@const wwGesamt = hz.wwZaehler.reduce((s, z) => s + z.verbrauch, 0)}
                <StructuredListRow>
                    <StructuredListCell>nach Verbrauch</StructuredListCell>
                    <StructuredListCell>{convertPercent(hz.p8)}</StructuredListCell>
                    <StructuredListCell>{nutzungsIntervall}</StructuredListCell>
                    <StructuredListCell>{convertEuro(wwBetrag * hz.p8)}</StructuredListCell>
                    <StructuredListCell>{convertPercent(vbWWAnteil)}</StructuredListCell>
                    <StructuredListCell>
                        {wwGesamt.toFixed(2)} m³
                        {#if hz.zeile.gesamtWW != null}
                            / {hz.zeile.gesamtWW.toFixed(2)} m³
                        {/if}
                    </StructuredListCell>
                    <StructuredListCell>{convertEuro(wwBetrag * vbWWAnteil)}</StructuredListCell>
                </StructuredListRow>
                {#each hz.wwZaehler as z}
                    <StructuredListRow>
                        <StructuredListCell style="padding-left: 2rem">Zähler {z.kennnummer}</StructuredListCell>
                        <StructuredListCell></StructuredListCell>
                        <StructuredListCell></StructuredListCell>
                        <StructuredListCell></StructuredListCell>
                        <StructuredListCell></StructuredListCell>
                        <StructuredListCell>{z.verbrauch.toFixed(2)} {z.einheit}</StructuredListCell>
                        <StructuredListCell></StructuredListCell>
                    </StructuredListRow>
                {/each}
            {/if}
            <StructuredListRow>
                <StructuredListCell>nach Wohnfläche</StructuredListCell>
                <StructuredListCell>{convertPercent(1 - hz.p8)}</StructuredListCell>
                <StructuredListCell>{nutzungsIntervall}</StructuredListCell>
                <StructuredListCell>{convertEuro(wwBetrag * (1 - hz.p8))}</StructuredListCell>
                <StructuredListCell>{convertPercent(wfWWAnteil)}</StructuredListCell>
                <StructuredListCell></StructuredListCell>
                <StructuredListCell>{convertEuro(wwBetrag * wfWWAnteil)}</StructuredListCell>
            </StructuredListRow>
        </StructuredListBody>
    </StructuredList>

    <Tile>
        <strong>{hz.zeile.bezeichnung}:</strong>
        {convertEuro(hz.gesamtbetrag)} gesamt · Ihr Anteil: {convertEuro(hz.meinBetrag)}
    </Tile>
{/each}

<Tile>
    <h5 style="display: flex; justify-content: center;">
        Warme Betriebskosten: {convertEuro(betragWarm)}
    </h5>
</Tile>

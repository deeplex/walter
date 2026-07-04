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
    import type {
        NkZeileInfo,
        P9DetailsInfo,
        ZaehlerVerbrauchInfo
    } from './AbrechnungslaufTypes';
    import { hkvoKosten } from './AbrechnungslaufTypes';
    import { convertEuro, convertPercent } from '$walter/services/utils';
    import {
        StructuredList,
        StructuredListBody,
        StructuredListCell,
        StructuredListHead,
        StructuredListRow
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

    const formatKommazahl = (n: number, digits = 2) =>
        n.toLocaleString('de-DE', {
            minimumFractionDigits: digits,
            maximumFractionDigits: digits
        });

    type HkvoZeile = {
        zeile: NkZeileInfo;
        gesamtbetrag: number;
        meinBetrag: number;
        para9_2: number;
        p9Details: P9DetailsInfo | null;
        p7: number;
        p8: number;
        heizVerbrauchAnteil: number | null;
        wwVerbrauchAnteil: number | null;
        heizZaehler: ZaehlerVerbrauchInfo[];
        wwZaehler: ZaehlerVerbrauchInfo[];
        nfZeitanteil: number;
    };

    const hkvoZeilen: HkvoZeile[] = zeilen
        .filter((z) => z.para9_2 != null)
        .map((z) => {
            const anteil = z.anteile.find((a) => a.vertragId === vertragId);
            const nfZeitanteil = anteil?.nfZeitanteil ?? 0;
            return {
                zeile: z,
                gesamtbetrag: z.betrag,
                meinBetrag: anteil ? hkvoKosten(z, anteil) : 0,
                para9_2: z.para9_2!,
                p9Details: z.p9Details,
                p7: z.p7!,
                p8: z.p8!,
                heizVerbrauchAnteil: anteil?.heizVerbrauchAnteil ?? null,
                wwVerbrauchAnteil: anteil?.wwVerbrauchAnteil ?? null,
                heizZaehler: anteil?.heizZaehler ?? [],
                wwZaehler: anteil?.wwZaehler ?? [],
                nfZeitanteil
            };
        });

    // Dedupliziert nach umlageId: eine HKVO-Zeile pro Rechnung, aber §9(2) einmal pro Umlage zeigen
    const gezeigteUmlageIds = new Set<number>();

    const betragWarm = hkvoZeilen.reduce(
        (s, hz) =>
            s +
            hkvoKosten(
                hz.zeile,
                hz.zeile.anteile.find((a) => a.vertragId === vertragId)!
            ),
        0
    );
</script>

<div style="padding: 1rem 1rem 0;">
    <h4>Warme Betriebskosten (HKVO)</h4>
</div>

{#each hkvoZeilen as hz}
    {@const showP9 = !gezeigteUmlageIds.has(hz.zeile.umlageId)}
    {#if showP9}
        {@const _add = gezeigteUmlageIds.add(hz.zeile.umlageId)}
        {#if hz.para9_2 > 0}
            <div class="p9-block">
                <strong>§9 Abs. 2 HKVO – Warmwasseranteil</strong>
                <div class="p9-formel">
                    <span>2,5</span>
                    <span class="frac frac--einheit">
                        <span>kWh</span>
                        <span>m³ · K</span>
                    </span>
                    <span>×</span>
                    <span class="frac">
                        <span>V</span>
                        <span>Q</span>
                    </span>
                    <span>× (t<sub>w</sub> − 10 °C)</span>
                    {#if hz.p9Details}
                        {@const p9 = hz.p9Details}
                        <span>&nbsp;=&nbsp; 2,5 ×</span>
                        <span class="frac">
                            <span>{formatKommazahl(p9.v)} m³</span>
                            <span>{formatKommazahl(p9.q)} kWh</span>
                        </span>
                        <span>× ({p9.tw} − 10) °C</span>
                    {/if}
                    <span>&nbsp;=&nbsp;</span>
                    <strong class="p9-ergebnis"
                        >{convertPercent(hz.para9_2)}</strong
                    >
                </div>
                {#if hz.p9Details}
                    {@const p9 = hz.p9Details}
                    <div class="p9-legende">
                        <span>
                            <strong>Q</strong> = {formatKommazahl(p9.q)} kWh — Allgemeinzähler
                            {p9.allgemeinZaehler} ({formatDate(
                                p9.qAnfangsdatum
                            )} – {formatDate(p9.qEnddatum)})
                        </span>
                        <span>
                            <strong>V</strong> = {formatKommazahl(p9.v)} m³ — Warmwasserzähler:
                            {#each p9.wwZaehler as z, i}
                                {i > 0 ? ', ' : ''}{z.kennnummer} ({formatKommazahl(
                                    z.verbrauch
                                )}
                                {z.einheit})
                            {/each}
                        </span>
                        <span>
                            <strong>t<sub>w</sub></strong> = {p9.tw} °C — angenommene
                            Warmwassertemperatur
                        </span>
                    </div>
                {/if}
            </div>
        {:else}
            <p style="padding: 0.5rem 1rem; color: var(--cds-text-helper);">
                {#if hz.p9Details}
                    Keine Warmwasser-Trennung nach §9 Abs. 2 HKVO
                    (Allgemeinzähler
                    {hz.p9Details.allgemeinZaehler}: Q = {formatKommazahl(
                        hz.p9Details.q
                    )} kWh, V = {formatKommazahl(hz.p9Details.v)} m³) – der gesamte
                    Betrag wird als Heizung (§7) abgerechnet.
                {:else}
                    Keine Warmwasser-Trennung nach §9 Abs. 2 HKVO (kein
                    AllgemeinWärme-Zähler konfiguriert) – der gesamte Betrag
                    wird als Heizung (§7) abgerechnet.
                {/if}
            </p>
        {/if}
    {/if}

    <!-- §7 Heizkosten -->
    {@const heizBetrag = hz.gesamtbetrag * (1 - hz.para9_2)}
    {@const nfHeizAnteil = (1 - hz.p7) * hz.nfZeitanteil}
    {@const vbHeizAnteil =
        hz.heizVerbrauchAnteil != null ? hz.p7 * hz.heizVerbrauchAnteil : 0}

    <StructuredList condensed>
        <StructuredListHead>
            <StructuredListRow>
                <StructuredListCell head>
                    §7 Heizung ({convertPercent(1 - hz.para9_2)} von {convertEuro(
                        hz.gesamtbetrag
                    )})
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
                {@const heizGesamt = hz.heizZaehler.reduce(
                    (s, z) => s + z.verbrauch,
                    0
                )}
                <StructuredListRow>
                    <StructuredListCell>nach Verbrauch</StructuredListCell>
                    <StructuredListCell
                        >{convertPercent(hz.p7)}</StructuredListCell
                    >
                    <StructuredListCell>{nutzungsIntervall}</StructuredListCell>
                    <StructuredListCell
                        >{convertEuro(heizBetrag * hz.p7)}</StructuredListCell
                    >
                    <StructuredListCell
                        >{convertPercent(vbHeizAnteil)}</StructuredListCell
                    >
                    <StructuredListCell>
                        {heizGesamt.toFixed(2)} kWh
                        {#if hz.zeile.gesamtWaerme != null}
                            / {hz.zeile.gesamtWaerme.toFixed(2)} kWh
                        {/if}
                    </StructuredListCell>
                    <StructuredListCell
                        >{convertEuro(
                            heizBetrag * vbHeizAnteil
                        )}</StructuredListCell
                    >
                </StructuredListRow>
            {/if}
            <StructuredListRow>
                <StructuredListCell>nach Nutzfläche</StructuredListCell>
                <StructuredListCell
                    >{convertPercent(1 - hz.p7)}</StructuredListCell
                >
                <StructuredListCell>{nutzungsIntervall}</StructuredListCell>
                <StructuredListCell
                    >{convertEuro(heizBetrag * (1 - hz.p7))}</StructuredListCell
                >
                <StructuredListCell
                    >{convertPercent(nfHeizAnteil)}</StructuredListCell
                >
                <StructuredListCell></StructuredListCell>
                <StructuredListCell
                    >{convertEuro(
                        heizBetrag * nfHeizAnteil
                    )}</StructuredListCell
                >
            </StructuredListRow>
        </StructuredListBody>
    </StructuredList>

    <!-- §8 Warmwasser (nur bei §9(2)-Trennung; sonst fällt alles auf §7) -->
    {#if hz.para9_2 > 0}
        {@const wwBetrag = hz.gesamtbetrag * hz.para9_2}
        {@const nfWWAnteil = (1 - hz.p8) * hz.nfZeitanteil}
        {@const vbWWAnteil =
            hz.wwVerbrauchAnteil != null ? hz.p8 * hz.wwVerbrauchAnteil : 0}

        <StructuredList condensed>
            <StructuredListHead>
                <StructuredListRow>
                    <StructuredListCell head>
                        §8 Warmwasser ({convertPercent(hz.para9_2)} von {convertEuro(
                            hz.gesamtbetrag
                        )})
                    </StructuredListCell>
                    <StructuredListCell head>Anteil</StructuredListCell>
                    <StructuredListCell head
                        >Nutzungsintervall</StructuredListCell
                    >
                    <StructuredListCell head>Betrag</StructuredListCell>
                    <StructuredListCell head>Ihr Anteil</StructuredListCell>
                    <StructuredListCell head>Ihr Verbrauch</StructuredListCell>
                    <StructuredListCell head>Ihre Kosten</StructuredListCell>
                </StructuredListRow>
            </StructuredListHead>
            <StructuredListBody>
                {#if hz.wwZaehler.length > 0}
                    {@const wwGesamt = hz.wwZaehler.reduce(
                        (s, z) => s + z.verbrauch,
                        0
                    )}
                    <StructuredListRow>
                        <StructuredListCell>nach Verbrauch</StructuredListCell>
                        <StructuredListCell
                            >{convertPercent(hz.p8)}</StructuredListCell
                        >
                        <StructuredListCell
                            >{nutzungsIntervall}</StructuredListCell
                        >
                        <StructuredListCell
                            >{convertEuro(wwBetrag * hz.p8)}</StructuredListCell
                        >
                        <StructuredListCell
                            >{convertPercent(vbWWAnteil)}</StructuredListCell
                        >
                        <StructuredListCell>
                            {wwGesamt.toFixed(2)} m³
                            {#if hz.zeile.gesamtWW != null}
                                / {hz.zeile.gesamtWW.toFixed(2)} m³
                            {/if}
                        </StructuredListCell>
                        <StructuredListCell
                            >{convertEuro(
                                wwBetrag * vbWWAnteil
                            )}</StructuredListCell
                        >
                    </StructuredListRow>
                {/if}
                <StructuredListRow>
                    <StructuredListCell>nach Nutzfläche</StructuredListCell>
                    <StructuredListCell
                        >{convertPercent(1 - hz.p8)}</StructuredListCell
                    >
                    <StructuredListCell>{nutzungsIntervall}</StructuredListCell>
                    <StructuredListCell
                        >{convertEuro(
                            wwBetrag * (1 - hz.p8)
                        )}</StructuredListCell
                    >
                    <StructuredListCell
                        >{convertPercent(nfWWAnteil)}</StructuredListCell
                    >
                    <StructuredListCell></StructuredListCell>
                    <StructuredListCell
                        >{convertEuro(
                            wwBetrag * nfWWAnteil
                        )}</StructuredListCell
                    >
                </StructuredListRow>
            </StructuredListBody>
        </StructuredList>
    {/if}

    <!-- Zähler-Aufschlüsselung — gleiche Tabellenform wie bei den kalten Kosten -->
    {#if hz.heizZaehler.length > 0 || hz.wwZaehler.length > 0}
        <StructuredList condensed>
            <StructuredListHead>
                <StructuredListRow>
                    <StructuredListCell head>Verbrauch</StructuredListCell>
                    <StructuredListCell head>Zähler</StructuredListCell>
                    <StructuredListCell head>Mein Verbrauch</StructuredListCell>
                    <StructuredListCell head>Gesamt</StructuredListCell>
                    <StructuredListCell head>Anteil</StructuredListCell>
                </StructuredListRow>
            </StructuredListHead>
            <StructuredListBody>
                {#if hz.heizZaehler.length > 0}
                    <StructuredListRow>
                        <StructuredListCell head>Wärme (§7)</StructuredListCell>
                        <StructuredListCell></StructuredListCell>
                        <StructuredListCell></StructuredListCell>
                        <StructuredListCell>
                            {#if hz.zeile.gesamtWaerme != null}
                                {hz.zeile.gesamtWaerme.toFixed(2)} kWh
                            {/if}
                        </StructuredListCell>
                        <StructuredListCell
                            >{convertPercent(
                                hz.heizVerbrauchAnteil ?? 0
                            )}</StructuredListCell
                        >
                    </StructuredListRow>
                    {#each hz.heizZaehler as z}
                        <StructuredListRow>
                            <StructuredListCell></StructuredListCell>
                            <StructuredListCell
                                >Zähler {z.kennnummer}</StructuredListCell
                            >
                            <StructuredListCell
                                >{z.verbrauch.toFixed(2)}
                                {z.einheit}</StructuredListCell
                            >
                            <StructuredListCell></StructuredListCell>
                            <StructuredListCell></StructuredListCell>
                        </StructuredListRow>
                    {/each}
                {/if}
                {#if hz.wwZaehler.length > 0}
                    <StructuredListRow>
                        <StructuredListCell head
                            >Warmwasser (§8)</StructuredListCell
                        >
                        <StructuredListCell></StructuredListCell>
                        <StructuredListCell></StructuredListCell>
                        <StructuredListCell>
                            {#if hz.zeile.gesamtWW != null}
                                {hz.zeile.gesamtWW.toFixed(2)} m³
                            {/if}
                        </StructuredListCell>
                        <StructuredListCell
                            >{convertPercent(
                                hz.wwVerbrauchAnteil ?? 0
                            )}</StructuredListCell
                        >
                    </StructuredListRow>
                    {#each hz.wwZaehler as z}
                        <StructuredListRow>
                            <StructuredListCell></StructuredListCell>
                            <StructuredListCell
                                >Zähler {z.kennnummer}</StructuredListCell
                            >
                            <StructuredListCell
                                >{z.verbrauch.toFixed(2)}
                                {z.einheit}</StructuredListCell
                            >
                            <StructuredListCell></StructuredListCell>
                            <StructuredListCell></StructuredListCell>
                        </StructuredListRow>
                    {/each}
                {/if}
            </StructuredListBody>
        </StructuredList>
    {/if}

    <p style="padding: 0.5rem 1rem;">
        <strong>{hz.zeile.bezeichnung}:</strong>
        {convertEuro(hz.gesamtbetrag)} gesamt · Ihr Anteil: {convertEuro(
            hz.meinBetrag
        )}
    </p>
{/each}

<p style="text-align: center; font-weight: 600; padding: 0.75rem 1rem;">
    Warme Betriebskosten: {convertEuro(betragWarm)}
</p>

<style>
    .p9-block {
        padding: 0.5rem 1rem 0.75rem;
    }

    .p9-formel {
        display: flex;
        align-items: center;
        flex-wrap: wrap;
        gap: 0.4rem;
        margin: 0.5rem 0;
        font-size: 1rem;
    }

    /* Mathematischer Bruch: Zähler über Nenner mit Bruchstrich */
    .frac {
        display: inline-flex;
        flex-direction: column;
        align-items: center;
        vertical-align: middle;
        line-height: 1.3;
        text-align: center;
    }

    .frac > span {
        padding: 0 0.4rem;
        white-space: nowrap;
    }

    .frac > span:last-child {
        border-top: 1px solid currentColor;
    }

    .frac--einheit {
        font-size: 0.8rem;
        color: var(--cds-text-helper);
    }

    .p9-ergebnis {
        font-size: 1.1rem;
    }

    .p9-legende {
        display: flex;
        flex-direction: column;
        gap: 0.15rem;
        color: var(--cds-text-helper);
        font-size: 0.875rem;
    }
</style>

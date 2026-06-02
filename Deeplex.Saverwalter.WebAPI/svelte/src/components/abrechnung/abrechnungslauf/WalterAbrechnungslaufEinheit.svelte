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
        AbrechnungseinheitInfo,
        PersonenZeitanteilInfo
    } from './AbrechnungslaufTypes';
    import WalterAbrechnungslaufHeizkosten from './WalterAbrechnungslaufHeizkosten.svelte';
    import WalterDataWrapperQuickAdd from '../../elements/WalterDataWrapperQuickAdd.svelte';
    import WalterTransaktion from '../../details/WalterTransaktion.svelte';
    import type { TransaktionsInput } from '$walter/lib';
    import { emptyTransaktionsInput } from '$walter/lib';
    import {
        convertEuro,
        convertM2,
        convertPercent
    } from '$walter/services/utils';
    import {
        DataTable,
        Row,
        StructuredList,
        StructuredListBody,
        StructuredListCell,
        StructuredListHead,
        StructuredListRow,
        Toolbar,
        ToolbarContent
    } from 'carbon-components-svelte';

    export let einheit: AbrechnungseinheitInfo;
    export let vertragId: number;
    export let personenZeitanteile: PersonenZeitanteilInfo[];
    export let nutzungVon: string;
    export let nutzungBis: string | null;
    export let jahr: number;
    export let fetchImpl: typeof fetch;

    let bkModalOpen = false;
    let buchungsInput: TransaktionsInput = emptyTransaktionsInput();

    function openBKModal(umlageId: number) {
        buchungsInput = {
            ...emptyTransaktionsInput(),
            betriebskostenEingaenge: [
                { betrag: 0, umlageId, betreffendesJahr: jahr }
            ]
        };
        bkModalOpen = true;
    }

    const isLeapYear = (y: number) =>
        (y % 4 === 0 && y % 100 !== 0) || y % 400 === 0;
    $: abrechnungstage = isLeapYear(jahr) ? 366 : 365;

    $: jahrStart = `${jahr}-01-01`;
    $: jahrEnde = `${jahr}-12-31`;
    $: vonInJahr = nutzungVon < jahrStart ? jahrStart : nutzungVon;
    $: bisRaw = nutzungBis ?? jahrEnde;
    $: bisInJahr = bisRaw > jahrEnde ? jahrEnde : bisRaw;
    $: nutzungstage =
        Math.round(
            (new Date(bisInJahr).getTime() - new Date(vonInJahr).getTime()) /
                86400000
        ) + 1;
    $: zeitanteil = nutzungstage / abrechnungstage;

    const formatDate = (d: string | null) => {
        if (!d) return 'heute';
        const [y, m, day] = d.split('-');
        return `${day}.${m}.${y}`;
    };

    $: anteilFaktorFuerSchluessel = (schluessel: string): number => {
        const zeile = einheit.nkZeilen.find((z) => z.schluessel === schluessel);
        return (
            zeile?.anteile.find((a) => a.vertragId === vertragId)
                ?.anteilFaktor ?? 0
        );
    };

    $: wfZeitanteil = anteilFaktorFuerSchluessel('n. WF');
    $: nfZeitanteil = anteilFaktorFuerSchluessel('n. NF');
    $: meaZeitanteil = anteilFaktorFuerSchluessel('n. MEA');
    $: neZeitanteil = anteilFaktorFuerSchluessel('n. NE');

    $: alleZeilen = einheit.nkZeilen.filter((z) =>
        z.anteile.some((a) => a.vertragId === vertragId)
    );
    $: kalteZeilen = alleZeilen.filter((z) => z.para9_2 == null);
    $: hkvoZeilen = alleZeilen.filter((z) => z.para9_2 != null);

    $: meineZeilen = kalteZeilen.map((z) => {
        const anteil = z.anteile.find((a) => a.vertragId === vertragId);
        const faktor = anteil?.anteilFaktor ?? 0;
        return {
            id: `${z.umlageId}-${z.buchungssatzId ?? 'none'}`,
            umlageId: z.umlageId,
            rechnungId: z.buchungssatzId,
            typ: z.bezeichnung,
            typId: z.umlagetypId,
            schluessel: z.schluessel,
            gesamtBetrag: z.betrag,
            betragLetztesJahr: z.betragLetztesJahr,
            anteil: faktor,
            betrag: z.betrag * faktor
        };
    });

    $: betragKalt = meineZeilen.reduce((s, r) => s + r.betrag, 0);

    $: hasWF = meineZeilen.some((r) => r.schluessel === 'n. WF');
    $: hasNF = meineZeilen.some((r) => r.schluessel === 'n. NF');
    $: hasNE = meineZeilen.some((r) => r.schluessel === 'n. NE');
    $: hasMEA = meineZeilen.some((r) => r.schluessel === 'n. MEA');
    $: hasPs = meineZeilen.some((r) => r.schluessel === 'n. Pers.');

    const schluesselLabel: Record<string, string> = {
        'n. WF': 'Wohnfläche',
        'n. NF': 'Nutzfläche',
        'n. NE': 'Nutzeinheiten',
        'n. MEA': 'Miteigentumsanteile',
        'n. Pers.': 'Personenzahl',
        'n. Verb.': 'Verbrauch'
    };
</script>

<div style="padding: 1rem 1rem 0;">
    <h4>Abrechnungseinheit: {einheit.wohnungNamen}</h4>
</div>

<!-- Personenzeitanteil / Einheit-Dimensionen -->
<Row>
    <StructuredList style="margin: 2em" condensed>
        <StructuredListHead>
            <StructuredListRow>
                <StructuredListCell head>Nutzeinheiten</StructuredListCell>
                <StructuredListCell head>Wohnfläche</StructuredListCell>
                <StructuredListCell head>Nutzfläche</StructuredListCell>
                <StructuredListCell head>Miteigentumsanteil</StructuredListCell>
                <StructuredListCell head>Bewohner</StructuredListCell>
                <StructuredListCell head>Nutzungsintervall</StructuredListCell>
                <StructuredListCell head>Tage</StructuredListCell>
            </StructuredListRow>
        </StructuredListHead>
        <StructuredListBody>
            {#each personenZeitanteile as intervall, index}
                <StructuredListRow>
                    <StructuredListCell
                        >{!index
                            ? einheit.gesamtNutzeinheit
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
                        >{!index
                            ? einheit.gesamtMiteigentumsanteile
                            : ''}</StructuredListCell
                    >
                    <StructuredListCell
                        >{intervall.gesamtPersonenzahl}</StructuredListCell
                    >
                    <StructuredListCell>
                        {formatDate(intervall.beginn)} – {formatDate(
                            intervall.ende
                        )}
                    </StructuredListCell>
                    <StructuredListCell
                        >{intervall.tage} / {nutzungstage}</StructuredListCell
                    >
                </StructuredListRow>
            {/each}
        </StructuredListBody>
    </StructuredList>
</Row>

<!-- Schlüssel-Zeitanteil-Aufschlüsselung -->
{#if hasWF || hasNF || hasNE || hasMEA || hasPs}
    <Row>
        <StructuredList condensed style="margin: 2em">
            <StructuredListHead>
                <StructuredListRow>
                    <StructuredListCell head>Größe / Gesamt</StructuredListCell>
                    <StructuredListCell head>Zeitraum</StructuredListCell>
                    <StructuredListCell head>Tage</StructuredListCell>
                    <StructuredListCell head>Anteil</StructuredListCell>
                </StructuredListRow>
            </StructuredListHead>
            <StructuredListBody>
                {#if hasWF}
                    <StructuredListRow>
                        <StructuredListCell head
                            >Wohnfläche (n. WF)</StructuredListCell
                        >
                    </StructuredListRow>
                    <StructuredListRow>
                        <StructuredListCell>
                            {convertM2(
                                zeitanteil > 0
                                    ? (einheit.gesamtWohnflaeche *
                                          wfZeitanteil) /
                                          zeitanteil
                                    : 0
                            )}
                            / {convertM2(einheit.gesamtWohnflaeche)}
                        </StructuredListCell>
                        <StructuredListCell
                            >{formatDate(vonInJahr)} – {formatDate(
                                bisInJahr
                            )}</StructuredListCell
                        >
                        <StructuredListCell>{nutzungstage}</StructuredListCell>
                        <StructuredListCell
                            >{convertPercent(wfZeitanteil)}</StructuredListCell
                        >
                    </StructuredListRow>
                {/if}
                {#if hasNF}
                    <StructuredListRow>
                        <StructuredListCell head
                            >Nutzfläche (n. NF)</StructuredListCell
                        >
                    </StructuredListRow>
                    <StructuredListRow>
                        <StructuredListCell>
                            {convertM2(
                                zeitanteil > 0
                                    ? (einheit.gesamtNutzflaeche *
                                          nfZeitanteil) /
                                          zeitanteil
                                    : 0
                            )}
                            / {convertM2(einheit.gesamtNutzflaeche)}
                        </StructuredListCell>
                        <StructuredListCell
                            >{formatDate(vonInJahr)} – {formatDate(
                                bisInJahr
                            )}</StructuredListCell
                        >
                        <StructuredListCell>{nutzungstage}</StructuredListCell>
                        <StructuredListCell
                            >{convertPercent(nfZeitanteil)}</StructuredListCell
                        >
                    </StructuredListRow>
                {/if}
                {#if hasMEA}
                    <StructuredListRow>
                        <StructuredListCell head
                            >Miteigentumsanteile (n. MEA)</StructuredListCell
                        >
                    </StructuredListRow>
                    <StructuredListRow>
                        <StructuredListCell>
                            {zeitanteil > 0
                                ? (
                                      (einheit.gesamtMiteigentumsanteile *
                                          meaZeitanteil) /
                                      zeitanteil
                                  ).toFixed(2)
                                : 0}
                            / {einheit.gesamtMiteigentumsanteile}
                        </StructuredListCell>
                        <StructuredListCell
                            >{formatDate(vonInJahr)} – {formatDate(
                                bisInJahr
                            )}</StructuredListCell
                        >
                        <StructuredListCell>{nutzungstage}</StructuredListCell>
                        <StructuredListCell
                            >{convertPercent(meaZeitanteil)}</StructuredListCell
                        >
                    </StructuredListRow>
                {/if}
                {#if hasNE}
                    <StructuredListRow>
                        <StructuredListCell head
                            >Nutzeinheiten (n. NE)</StructuredListCell
                        >
                    </StructuredListRow>
                    <StructuredListRow>
                        <StructuredListCell>
                            {zeitanteil > 0
                                ? (
                                      (einheit.gesamtNutzeinheit *
                                          neZeitanteil) /
                                      zeitanteil
                                  ).toFixed(2)
                                : 0}
                            / {einheit.gesamtNutzeinheit}
                        </StructuredListCell>
                        <StructuredListCell
                            >{formatDate(vonInJahr)} – {formatDate(
                                bisInJahr
                            )}</StructuredListCell
                        >
                        <StructuredListCell>{nutzungstage}</StructuredListCell>
                        <StructuredListCell
                            >{convertPercent(neZeitanteil)}</StructuredListCell
                        >
                    </StructuredListRow>
                {/if}
                {#if hasPs}
                    <StructuredListRow>
                        <StructuredListCell head
                            >Personenzahl (n. Pers.)</StructuredListCell
                        >
                    </StructuredListRow>
                    {#each personenZeitanteile as p}
                        <StructuredListRow>
                            <StructuredListCell
                                >{p.personenzahl} / {p.gesamtPersonenzahl}</StructuredListCell
                            >
                            <StructuredListCell
                                >{formatDate(p.beginn)} – {formatDate(
                                    p.ende
                                )}</StructuredListCell
                            >
                            <StructuredListCell>{p.tage}</StructuredListCell>
                            <StructuredListCell
                                >{convertPercent(p.anteil)}</StructuredListCell
                            >
                        </StructuredListRow>
                    {/each}
                {/if}
            </StructuredListBody>
        </StructuredList>
    </Row>
{/if}

<!-- Kostentabelle -->
<DataTable
    headers={[
        { key: 'typ', value: 'Kostenanteil' },
        { key: 'schluessel', value: 'Schlüssel' },
        { key: 'gesamtBetrag', value: 'Betrag' },
        { key: 'betragLetztesJahr', value: 'Betrag (letztes Jahr)' },
        { key: 'anteil', value: 'Anteil' },
        { key: 'betrag', value: 'Kosten' }
    ]}
    rows={meineZeilen.map((r) => ({
        ...r,
        gesamtBetrag: convertEuro(r.gesamtBetrag),
        betragLetztesJahr: convertEuro(r.betragLetztesJahr),
        anteil: convertPercent(r.anteil),
        betrag: convertEuro(r.betrag)
    }))}
    size="short"
>
    <Toolbar><ToolbarContent /></Toolbar>
    <svelte:fragment slot="cell" let:cell let:row>
        {#if cell.key === 'typ'}
            {#if row.rechnungId}
                <a href="/umlagen/{row.umlageId}">{cell.value}</a>
            {:else}
                <button
                    style="color: var(--cds-support-error); font-weight: 600; background: none; border: none; cursor: pointer; padding: 0; font: inherit;"
                    on:click={() => openBKModal(row.umlageId)}
                    >{cell.value}</button
                >
            {/if}
        {:else if cell.key === 'schluessel'}
            <span title={schluesselLabel[cell.value] ?? cell.value}
                >{cell.value}</span
            >
        {:else}
            {cell.value}
        {/if}
    </svelte:fragment>
</DataTable>

<p style="text-align: center; font-weight: 600; padding: 0.75rem 1rem;">
    Kalte Nebenkosten: {convertEuro(betragKalt)}
</p>

<WalterDataWrapperQuickAdd
    title="Betriebskostenrechnung"
    addUrl="/api/transaktionen/buchen"
    bind:addEntry={buchungsInput}
    bind:addModalOpen={bkModalOpen}
>
    <WalterTransaktion {fetchImpl} bind:buchung={buchungsInput} />
</WalterDataWrapperQuickAdd>

{#if hkvoZeilen.length > 0}
    <WalterAbrechnungslaufHeizkosten
        zeilen={hkvoZeilen}
        {vertragId}
        nutzungVon={vonInJahr}
        nutzungBis={bisInJahr}
        {nutzungstage}
        {abrechnungstage}
    />
{/if}

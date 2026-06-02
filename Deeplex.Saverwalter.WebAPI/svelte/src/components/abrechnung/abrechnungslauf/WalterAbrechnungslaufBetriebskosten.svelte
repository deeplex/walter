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
        AbrechnungslaufGruppeResult,
        NkAnteilInfo
    } from './AbrechnungslaufTypes';
    import {
        Accordion,
        AccordionItem,
        DataTable,
        Toolbar,
        ToolbarContent
    } from 'carbon-components-svelte';

    export let gruppe: AbrechnungslaufGruppeResult;

    const euro = (v: number) =>
        v.toLocaleString('de-DE', { style: 'currency', currency: 'EUR' });

    const nkAnteilMismatch = (a: NkAnteilInfo) =>
        a.geplanterBetrag != null &&
        a.gebuchterBetrag != null &&
        Math.abs(a.geplanterBetrag - a.gebuchterBetrag) > 0.005;

    const nkAnteilStatus = (a: NkAnteilInfo) => {
        if (nkAnteilMismatch(a))
            return `Gebucht: ${euro(a.gebuchterBetrag ?? 0)}`;
        if (a.geplanterBetrag != null && a.gebuchterBetrag != null)
            return 'Gebucht';
        return 'Nicht gebucht';
    };

    const nkAnteilDisplayBetrag = (a: NkAnteilInfo) =>
        a.geplanterBetrag ?? a.gebuchterBetrag ?? 0;

    type BkZeile = {
        id: string;
        lineId: number;
        bezeichnung: string;
        wohnungNamen: string;
        wohnungId: number | null;
        umlageId: number;
        umlagetypId: number;
        rechnungId: number | null;
        rechnungsBetrag: number;
        empfaengerName: string;
        vertragId: number | null;
        hatVerteilung: boolean;
        istGebucht: boolean;
        anteilBetrag: { value: number; formatted: string };
        status: string;
        betragAbweichend: boolean;
    };

    function mapZeilen(g: AbrechnungslaufGruppeResult): BkZeile[] {
        const resultatByVertrag = new Map(
            g.resultate
                .filter((r) => r.vertragId != null)
                .map((r) => [r.vertragId, r])
        );
        let i = 0;
        let lineId = 0;
        return g.abrechnungseinheiten.flatMap((einheit) =>
            einheit.nkZeilen.flatMap((z) => {
                const currentLineId = lineId++;
                return z.anteile.length > 0
                    ? z.anteile.map<BkZeile>((a) => {
                          const r =
                              a.vertragId != null
                                  ? resultatByVertrag.get(a.vertragId)
                                  : undefined;
                          const wohnungNamen = r
                              ? r.wohnungBezeichnung
                                    .split(' – ')
                                    .slice(1)
                                    .join(' – ')
                              : einheit.wohnungNamen;
                          return {
                              id: `bk-${i++}`,
                              lineId: currentLineId,
                              bezeichnung: z.bezeichnung,
                              wohnungNamen,
                              wohnungId: r?.wohnungId ?? null,
                              umlageId: z.umlageId,
                              umlagetypId: z.umlagetypId,
                              rechnungId: z.buchungssatzId,
                              rechnungsBetrag: z.betrag,
                              empfaengerName: a.bezeichnung,
                              vertragId: a.vertragId,
                              hatVerteilung: true,
                              istGebucht:
                                  z.buchungssatzId != null &&
                                  a.gebuchterBetrag != null,
                              anteilBetrag: {
                                  value: nkAnteilDisplayBetrag(a),
                                  formatted: euro(nkAnteilDisplayBetrag(a))
                              },
                              status: nkAnteilStatus(a),
                              betragAbweichend: nkAnteilMismatch(a)
                          };
                      })
                    : [
                          {
                              id: `bk-${i++}`,
                              lineId: currentLineId,
                              bezeichnung: z.bezeichnung,
                              wohnungNamen: einheit.wohnungNamen,
                              wohnungId: null,
                              umlageId: z.umlageId,
                              umlagetypId: z.umlagetypId,
                              rechnungId: null,
                              rechnungsBetrag: z.betrag,
                              empfaengerName: '',
                              vertragId: null,
                              hatVerteilung: false,
                              istGebucht: false,
                              anteilBetrag: { value: 0, formatted: '–' },
                              status: '⚠ fehlt',
                              betragAbweichend: false
                          } as BkZeile
                      ];
            })
        );
    }

    function groupByTyp(rows: BkZeile[]) {
        const grouped = new Map<string, BkZeile[]>();
        for (const row of rows) {
            const list = grouped.get(row.bezeichnung) ?? [];
            list.push(row);
            grouped.set(row.bezeichnung, list);
        }
        return [...grouped.entries()]
            .sort(([a], [b]) => a.localeCompare(b, 'de-DE'))
            .map(([bezeichnung, list]) => {
                const lineSums = new Map<
                    number,
                    {
                        rechnungsBetrag: number;
                        verteilt: number;
                        hatVerteilung: boolean;
                    }
                >();
                for (const row of list) {
                    const existing = lineSums.get(row.lineId) ?? {
                        rechnungsBetrag: row.rechnungsBetrag,
                        verteilt: 0,
                        hatVerteilung: false
                    };
                    existing.rechnungsBetrag = row.rechnungsBetrag;
                    existing.hatVerteilung =
                        existing.hatVerteilung || row.hatVerteilung;
                    if (row.hatVerteilung)
                        existing.verteilt += row.anteilBetrag.value;
                    lineSums.set(row.lineId, existing);
                }
                const lineValues = [...lineSums.values()];
                const rechnungsbetrag = lineValues.reduce(
                    (s, l) => s + l.rechnungsBetrag,
                    0
                );
                const verteilt = lineValues.reduce((s, l) => s + l.verteilt, 0);
                const hasAusstehend = list.some(
                    (row) => !row.hatVerteilung || !row.istGebucht
                );
                const hasRowMismatch = list.some((row) => row.betragAbweichend);
                const hasMismatch =
                    hasRowMismatch ||
                    (!hasAusstehend &&
                        Math.abs(verteilt - rechnungsbetrag) > 0.005);
                const statusText = hasAusstehend
                    ? 'Nicht gebucht'
                    : hasMismatch
                      ? `Gebucht: ${euro(verteilt)}`
                      : 'Gebucht';
                return {
                    title: `${bezeichnung} - Rechnung: ${euro(rechnungsbetrag)} - ${statusText}`,
                    statusKind: hasMismatch
                        ? 'mismatch'
                        : hasAusstehend
                          ? 'pending'
                          : 'booked',
                    rows: list.sort((a, b) => {
                        const wc = a.wohnungNamen.localeCompare(
                            b.wohnungNamen,
                            'de-DE'
                        );
                        return wc !== 0
                            ? wc
                            : a.empfaengerName.localeCompare(
                                  b.empfaengerName,
                                  'de-DE'
                              );
                    })
                };
            });
    }

    const typGruppen = groupByTyp(mapZeilen(gruppe));
</script>

<Accordion>
    {#each typGruppen as typGruppe}
        <AccordionItem>
            <span
                slot="title"
                style={typGruppe.statusKind === 'mismatch'
                    ? 'color: var(--cds-support-error); font-weight: 600;'
                    : ''}
            >
                {typGruppe.title}
            </span>
            <DataTable
                headers={[
                    { key: 'wohnungNamen', value: 'Wohnung' },
                    { key: 'empfaengerName', value: 'Mieter / Eigenanteil' },
                    { key: 'anteilBetrag', value: 'Anteil' },
                    { key: 'status', value: 'Status' }
                ]}
                rows={typGruppe.rows}
                size="short"
                style="margin-bottom: 0.5rem;"
            >
                <Toolbar><ToolbarContent /></Toolbar>
                <svelte:fragment slot="cell" let:cell let:row>
                    {#if cell.key === 'wohnungNamen'}
                        {#if row.wohnungId != null}
                            <a
                                href="/wohnungen/{row.wohnungId}"
                                style={row.rechnungId == null
                                    ? 'color: var(--cds-support-error);'
                                    : ''}>{cell.value}</a
                            >
                        {:else}
                            <span
                                style={row.rechnungId == null
                                    ? 'color: var(--cds-support-error);'
                                    : ''}
                            >
                                {cell.value}
                            </span>
                        {/if}
                    {:else if cell.key === 'empfaengerName'}
                        {#if row.vertragId != null}
                            <a href="/vertraege/{row.vertragId}">{cell.value}</a
                            >
                        {:else}
                            <span
                                style={row.rechnungId == null
                                    ? 'color: var(--cds-support-error);'
                                    : ''}
                            >
                                {cell.value}
                            </span>
                        {/if}
                    {:else if cell.key === 'anteilBetrag'}
                        <span
                            style={row.betragAbweichend
                                ? 'color: var(--cds-support-error); font-weight: 600;'
                                : ''}
                        >
                            {cell.value.formatted}
                        </span>
                    {:else if cell.key === 'status'}
                        <span
                            style={row.betragAbweichend
                                ? 'color: var(--cds-support-error); font-weight: 600;'
                                : ''}
                        >
                            {cell.value}
                        </span>
                    {:else}
                        {cell.value}
                    {/if}
                </svelte:fragment>
            </DataTable>
        </AccordionItem>
    {/each}
</Accordion>

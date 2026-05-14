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
    import type { AbrechnungslaufGruppeResult } from './AbrechnungslaufTypes';
    import {
        WalterDataBarChartSimple,
        WalterDataDonutChart
    } from '$walter/components';
    import type { WalterDataConfigType } from '$walter/components/data/WalterData';
    import WalterAbrechnungslaufNebenkosten from './WalterAbrechnungslaufNebenkosten.svelte';

    export let gruppe: AbrechnungslaufGruppeResult;

    const euro = (v: number) =>
        v.toLocaleString('de-DE', { style: 'currency', currency: 'EUR' });

    const chartBase = {
        legend: { enabled: false },
        height: '280px',
        tooltip: { truncation: { threshold: 999 } },
        heatmap: undefined
    };
    const donutCenter = {
        donut: {
            center: {
                label: '€',
                numberFormatter: (v: number) =>
                    v.toLocaleString('de-DE', { maximumFractionDigits: 0 })
            }
        }
    };
    const donutPercentCenter = {
        donut: {
            center: {
                label: '%',
                numberFormatter: (v: number) =>
                    v.toLocaleString('de-DE', { maximumFractionDigits: 0 })
            }
        }
    };
    const barAxes = {
        axes: {
            left: { mapsTo: 'group', scaleType: 'labels' },
            bottom: { mapsTo: 'value', includeZero: true }
        }
    };

    function buildKostenBar(): {
        kosten: WalterDataConfigType;
        vorjahr: WalterDataConfigType | null;
    } {
        const byTyp = new Map<
            string,
            { betrag: number; letztesJahr: number }
        >();
        for (const einheit of gruppe.abrechnungseinheiten) {
            for (const z of einheit.nkZeilen) {
                const prev = byTyp.get(z.bezeichnung) ?? {
                    betrag: 0,
                    letztesJahr: 0
                };
                byTyp.set(z.bezeichnung, {
                    betrag: prev.betrag + z.betrag,
                    letztesJahr: prev.letztesJahr + z.betragLetztesJahr
                });
            }
        }
        const sorted = [...byTyp.entries()].sort(
            (a, b) => b[1].betrag - a[1].betrag
        );
        const kosten: WalterDataConfigType = {
            data: sorted.map(([group, v]) => ({ group, value: v.betrag })),
            options: { ...chartBase, ...barAxes, title: 'Kostenverteilung' }
        };
        const hatVorjahr = sorted.some(([, v]) => v.letztesJahr > 0);
        const vorjahr: WalterDataConfigType | null = hatVorjahr
            ? {
                  data: sorted.map(([group, v]) => ({
                      group,
                      value: v.betrag - v.letztesJahr
                  })),
                  options: {
                      ...chartBase,
                      ...barAxes,
                      title: 'Differenz zum Vorjahr'
                  }
              }
            : null;
        return { kosten, vorjahr };
    }

    function buildGesamtkostenDonut(): WalterDataConfigType {
        const byPartei = new Map<string, number>();
        for (const einheit of gruppe.abrechnungseinheiten) {
            for (const z of einheit.nkZeilen) {
                for (const a of z.anteile) {
                    const label = a.bezeichnung || 'Eigenanteil';
                    const betrag = a.geplanterBetrag ?? a.gebuchterBetrag ?? 0;
                    byPartei.set(label, (byPartei.get(label) ?? 0) + betrag);
                }
            }
        }
        const data = [...byPartei.entries()]
            .sort((a, b) => b[1] - a[1])
            .filter(([, v]) => v > 0)
            .map(([group, value]) => ({ group, value }));
        return {
            data,
            options: { ...chartBase, ...donutCenter, title: 'Gesamtkosten' }
        };
    }

    function buildVorauszahlungenDonut(): WalterDataConfigType {
        const byPartei = new Map<string, number>();
        for (const r of gruppe.resultate) {
            const label = r.mieterBezeichnung || 'Eigenanteil';
            byPartei.set(label, (byPartei.get(label) ?? 0) + r.vorauszahlung);
        }
        return {
            data: [...byPartei.entries()]
                .filter(([, v]) => v > 0)
                .map(([group, value]) => ({ group, value })),
            options: { ...chartBase, ...donutCenter, title: 'Vorauszahlungen' }
        };
    }

    function buildSchluesselCharts(): WalterDataConfigType[] {
        const bySchluessel = new Map<string, Map<string, number>>();
        for (const einheit of gruppe.abrechnungseinheiten) {
            for (const z of einheit.nkZeilen) {
                if (z.schluessel === 'n. Verb.') continue;
                const map =
                    bySchluessel.get(z.schluessel) ?? new Map<string, number>();
                for (const a of z.anteile) {
                    const label = a.bezeichnung || 'Eigenanteil';
                    const betrag = a.geplanterBetrag ?? a.gebuchterBetrag ?? 0;
                    map.set(label, (map.get(label) ?? 0) + betrag);
                }
                bySchluessel.set(z.schluessel, map);
            }
        }
        return [...bySchluessel.entries()]
            .filter(([, m]) => m.size > 1)
            .map(([schluessel, m]) => ({
                data: [...m.entries()]
                    .map(([group, value]) => ({ group, value }))
                    .filter((d) => d.value > 0),
                options: { ...chartBase, ...donutCenter, title: schluessel }
            }));
    }

    function buildVerbrauchsCharts(): WalterDataConfigType[] {
        const byUmlage = new Map<
            number,
            { bezeichnung: string; anteile: Map<string, number> }
        >();
        for (const einheit of gruppe.abrechnungseinheiten) {
            for (const z of einheit.nkZeilen) {
                if (z.schluessel !== 'n. Verb.') continue;
                const entry = byUmlage.get(z.umlageId) ?? {
                    bezeichnung: z.bezeichnung,
                    anteile: new Map()
                };
                for (const a of z.anteile) {
                    const label = a.bezeichnung || 'Eigenanteil';
                    const betrag = a.geplanterBetrag ?? a.gebuchterBetrag ?? 0;
                    entry.anteile.set(
                        label,
                        (entry.anteile.get(label) ?? 0) + betrag
                    );
                }
                byUmlage.set(z.umlageId, entry);
            }
        }
        return [...byUmlage.values()]
            .filter(({ anteile }) => anteile.size > 1)
            .map(({ bezeichnung, anteile }) => ({
                data: [...anteile.entries()]
                    .map(([group, value]) => ({ group, value }))
                    .filter((d) => d.value > 0),
                options: {
                    ...chartBase,
                    ...donutCenter,
                    title: `Verbrauch: ${bezeichnung}`
                }
            }));
    }

    function buildWarmwasserAnteilCharts(): WalterDataConfigType[] {
        const byUmlage = new Map<
            number,
            {
                bezeichnung: string;
                gesamtbetrag: number;
                wwAnteilBetrag: number;
            }
        >();

        for (const einheit of gruppe.abrechnungseinheiten) {
            for (const z of einheit.nkZeilen) {
                if (z.para9_2 == null) continue;

                const prev = byUmlage.get(z.umlageId) ?? {
                    bezeichnung: z.bezeichnung,
                    gesamtbetrag: 0,
                    wwAnteilBetrag: 0
                };

                byUmlage.set(z.umlageId, {
                    bezeichnung: prev.bezeichnung,
                    gesamtbetrag: prev.gesamtbetrag + z.betrag,
                    wwAnteilBetrag: prev.wwAnteilBetrag + z.betrag * z.para9_2
                });
            }
        }

        const wwColor = {
            color: { scale: { Warmwasser: '#fa4d56', Heizung: '#ff832b' } }
        };

        return [...byUmlage.values()]
            .filter((u) => u.gesamtbetrag > 0)
            .map((u, index) => {
                const wwAnteil = Math.min(
                    Math.max((u.wwAnteilBetrag / u.gesamtbetrag) * 100, 0),
                    100
                );
                const heizAnteil = Math.max(100 - wwAnteil, 0);
                return {
                    data: [
                        { group: 'Warmwasser', value: wwAnteil },
                        { group: 'Heizung', value: heizAnteil }
                    ],
                    options: {
                        ...chartBase,
                        ...donutPercentCenter,
                        ...wwColor,
                        legend: { enabled: index === 0 },
                        title: `WW-Anteil: ${u.bezeichnung}`
                    }
                } as WalterDataConfigType;
            });
    }

    const donutPalette = [
        '#6929c4',
        '#1192e8',
        '#005d5d',
        '#9f1853',
        '#fa4d56',
        '#570408',
        '#198038',
        '#002d9c',
        '#ee538b',
        '#b28600',
        '#009d9a',
        '#012749',
        '#8a3800',
        '#a56eff'
    ];

    function applySharedColorScale(
        charts: WalterDataConfigType[]
    ): WalterDataConfigType[] {
        const allGroups = new Set<string>();
        for (const c of charts) {
            for (const d of c.data as { group: string }[]) {
                allGroups.add(d.group);
            }
        }
        const scale: Record<string, string> = {};
        let i = 0;
        for (const g of allGroups) {
            scale[g] = donutPalette[i++ % donutPalette.length];
        }
        return charts.map((c, index) => ({
            ...c,
            options: {
                ...c.options,
                color: { scale },
                legend:
                    index === 0
                        ? {
                              enabled: true,
                              position: 'left',
                              truncation: { type: 'none' }
                          }
                        : { enabled: false }
            }
        }));
    }

    const { kosten, vorjahr } = buildKostenBar();
    const warmwasserAnteilCharts = buildWarmwasserAnteilCharts();
    const _rawSchluessel = buildSchluesselCharts();
    const _rawVerbrauch = buildVerbrauchsCharts();
    const _coloredDonuts = applySharedColorScale([
        buildGesamtkostenDonut(),
        buildVorauszahlungenDonut(),
        ..._rawSchluessel,
        ..._rawVerbrauch
    ]);
    const gesamtkostenChart = _coloredDonuts[0];
    const vorauszahlungenChart = _coloredDonuts[1];
    const schluesselCharts = _coloredDonuts.slice(2, 2 + _rawSchluessel.length);
    const verbrauchsCharts = _coloredDonuts.slice(2 + _rawSchluessel.length);
</script>

<div
    style="
        margin: 1rem 0 1.5rem;
        padding: 1.25rem 1.5rem 1.5rem;
        background: var(--cds-layer-01);
        border-top: 3px solid var(--cds-interactive);
    "
>
    <p
        style="
            font-size: 0.75rem;
            font-weight: 600;
            letter-spacing: 0.08em;
            text-transform: uppercase;
            color: var(--cds-text-secondary);
            margin: 0 0 1.25rem;
        "
    >
        Kostenübersicht
    </p>

    <WalterAbrechnungslaufNebenkosten
        zeilen={gruppe.abrechnungseinheiten.flatMap((e) => e.nkZeilen)}
    />

    <div
        style="display: grid; grid-template-columns: repeat(auto-fill, minmax(20rem, 1fr)); gap: 2rem;"
    >
        <WalterDataBarChartSimple config={kosten} />
        {#if vorjahr}
            <WalterDataBarChartSimple config={vorjahr} />
        {/if}
        {#each warmwasserAnteilCharts as config}
            <WalterDataDonutChart {config} />
        {/each}
        <WalterDataDonutChart config={gesamtkostenChart} />
        <WalterDataDonutChart config={vorauszahlungenChart} />
        {#each schluesselCharts as config}
            <WalterDataDonutChart {config} />
        {/each}
        {#each verbrauchsCharts as config}
            <WalterDataDonutChart {config} />
        {/each}
    </div>
</div>

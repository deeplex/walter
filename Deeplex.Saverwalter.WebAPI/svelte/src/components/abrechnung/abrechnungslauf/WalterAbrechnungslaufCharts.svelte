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
    import {
        AlluvialChart,
        BarChartStacked,
        BarChartSimple,
        ScaleTypes,
        type AlluvialChartOptions,
        type BarChartOptions,
        type ChartTabularData
    } from '@carbon/charts-svelte';
    import '@carbon/charts-svelte/styles.css';
    import { onDestroy, type ComponentProps } from 'svelte';

    export let gruppe: AbrechnungslaufGruppeResult;

    // Sankey/Alluvial: Umlagen (links) → Verträge / Eigenanteil (rechts).
    function buildSankey(): {
        data: ChartTabularData;
        options: AlluvialChartOptions;
        height: string;
    } | null {
        const flowByQuelle = new Map<string, Map<string, number>>();
        const quellen = new Set<string>();
        const parteien = new Set<string>();
        for (const einheit of gruppe.abrechnungseinheiten) {
            for (const z of einheit.nkZeilen) {
                const quelle = z.bezeichnung;
                for (const a of z.anteile) {
                    const partei = a.bezeichnung || 'Eigenanteil';
                    const betrag = a.geplanterBetrag ?? a.gebuchterBetrag ?? 0;
                    if (betrag <= 0.005) continue;
                    const perPartei =
                        flowByQuelle.get(quelle) ?? new Map<string, number>();
                    perPartei.set(
                        partei,
                        (perPartei.get(partei) ?? 0) + betrag
                    );
                    flowByQuelle.set(quelle, perPartei);
                    quellen.add(quelle);
                    parteien.add(partei);
                }
            }
        }
        if (flowByQuelle.size === 0) return null;

        const data = [...flowByQuelle.entries()].flatMap(
            ([source, perPartei]) =>
                [...perPartei.entries()].map(([target, value]) => ({
                    source,
                    target,
                    value: Math.round(value * 100) / 100
                }))
        );
        // Alphabetisch als Seed für die Sankey-Anordnung (die Engine ordnet
        // anschließend zur Kreuzungsminimierung, daher nur best effort).
        const alpha = (a: string, b: string) => a.localeCompare(b, 'de');
        const nodes = [
            ...[...quellen]
                .sort(alpha)
                .map((name) => ({ name, category: 'Quelle' })),
            ...[...parteien]
                .sort(alpha)
                .map((name) => ({ name, category: 'Partei' }))
        ];
        const height = `${Math.max(520, Math.max(quellen.size, parteien.size) * 56)}px`;
        return {
            data,
            options: {
                title: 'Kostenfluss: Umlagen → Verträge / Eigenanteil',
                alluvial: { nodes },
                height,
                tooltip: {
                    // Namen nicht kürzen – der Platz ist da.
                    truncation: { type: 'none' },
                    valueFormatter: (v: number | string) =>
                        typeof v === 'number' ? euro(v) : `${v} €`
                }
            },
            height
        };
    }

    const sankey = buildSankey();

    // Carbon-Event-Strings (die Events-Enums sind zur Laufzeit vorhanden,
    // aber nicht im TS-Typ exportiert).
    const ALLUVIAL_NODE_MOUSEOVER = 'alluvial-node-mouseover';
    const TOOLTIP_SHOW = 'show-tooltip';

    // Carbon zeigt beim Hovern über einen Node (Umlage oder Partei) kein
    // Tooltip – nur über den Linien. Wir hängen uns an NODE_MOUSEOVER und
    // dispatchen selbst ein Tooltip mit allen verbundenen Flüssen + Summe.
    // (MOVE/HIDE feuert Carbon bereits selbst auf Node-Ebene.)
    type SankeyLink = {
        value: number;
        source?: { name?: string };
        target?: { name?: string };
    };
    type SankeyNode = {
        name: string;
        sourceLinks?: SankeyLink[];
        targetLinks?: SankeyLink[];
    };
    type SankeyHover = CustomEvent<{
        datum?: SankeyNode;
        event: Event;
        element: Element;
    }>;
    type ChartEvents = {
        addEventListener(
            type: string,
            listener: (e: SankeyHover) => void
        ): void;
        removeEventListener(
            type: string,
            listener: (e: SankeyHover) => void
        ): void;
        dispatchEvent(type: string, detail: object): void;
    };
    // Die Chart-Instanz der Bibliothek; ihr `services` ist dort ungetypt.
    let sankeyChart: ComponentProps<AlluvialChart>['chart'] = null;
    let sankeyEvents: ChartEvents | undefined;

    function onSankeyNodeMouseover(e: SankeyHover) {
        const node = e.detail?.datum;
        if (!node) return;
        const flows = [
            ...(node.sourceLinks ?? []).map((l) => ({
                name: l.target?.name ?? '',
                value: l.value
            })),
            ...(node.targetLinks ?? []).map((l) => ({
                name: l.source?.name ?? '',
                value: l.value
            }))
        ].sort((a, b) => a.name.localeCompare(b.name, 'de'));
        if (flows.length === 0) return;
        const summe = flows.reduce((s, f) => s + f.value, 0);
        const items = [
            { label: node.name, value: summe, bold: true },
            ...flows.map((f) => ({ label: f.name, value: f.value }))
        ];
        sankeyEvents?.dispatchEvent(TOOLTIP_SHOW, {
            event: e.detail.event,
            hoveredElement: e.detail.element,
            items
        });
    }

    // Sobald die Chart-Instanz via bind:chart durchgereicht ist, einhängen.
    let sankeyHooked = false;
    $: if (sankeyChart && !sankeyHooked) {
        sankeyHooked = true;
        sankeyEvents = sankeyChart.services?.events;
        sankeyEvents?.addEventListener(
            ALLUVIAL_NODE_MOUSEOVER,
            onSankeyNodeMouseover
        );
    }

    onDestroy(() =>
        sankeyEvents?.removeEventListener(
            ALLUVIAL_NODE_MOUSEOVER,
            onSankeyNodeMouseover
        )
    );

    // #1 – Kosten je Vertrag, gestapelt nach Umlage.
    function buildKostenJeVertrag(): {
        data: ChartTabularData;
        options: BarChartOptions;
    } | null {
        // Je (Partei, Umlage) ein Betrag, in der Reihenfolge des ersten Auftretens.
        const agg = new Map<
            string,
            { key: string; group: string; value: number }
        >();
        const parteien = new Set<string>();
        for (const einheit of gruppe.abrechnungseinheiten) {
            for (const z of einheit.nkZeilen) {
                for (const a of z.anteile) {
                    const partei = a.bezeichnung || 'Eigenanteil';
                    const betrag = a.geplanterBetrag ?? a.gebuchterBetrag ?? 0;
                    if (betrag <= 0.005) continue;
                    const key = JSON.stringify([partei, z.bezeichnung]);
                    const eintrag = agg.get(key) ?? {
                        key: partei,
                        group: z.bezeichnung,
                        value: 0
                    };
                    eintrag.value += betrag;
                    agg.set(key, eintrag);
                    parteien.add(partei);
                }
            }
        }
        if (agg.size === 0) return null;
        const data = [...agg.values()].map(({ key, group, value }) => ({
            key,
            group,
            value: Math.round(value * 100) / 100
        }));
        return {
            data,
            options: {
                title: 'Kosten je Vertrag (gestapelt nach Umlage)',
                axes: {
                    left: { mapsTo: 'key', scaleType: ScaleTypes.LABELS },
                    bottom: { mapsTo: 'value', stacked: true }
                },
                height: `${Math.max(240, parteien.size * 48 + 80)}px`,
                legend: { position: 'right', truncation: { type: 'none' } },
                tooltip: {
                    valueFormatter: (v: number) => euro(v),
                    truncation: { threshold: 999 }
                }
            }
        };
    }

    // #2 – Saldo je Vertrag: + Nachzahlung (rot) / − Guthaben (blau).
    function buildSaldoBar(): {
        data: ChartTabularData;
        options: BarChartOptions;
    } | null {
        const byMieter = new Map<string, number>();
        for (const r of gruppe.resultate) {
            if (r.vertragId == null) continue; // Leerstand/Eigenanteil hier ausblenden
            byMieter.set(
                r.mieterBezeichnung,
                (byMieter.get(r.mieterBezeichnung) ?? 0) + r.saldo
            );
        }
        if (byMieter.size === 0) return null;
        const data = [...byMieter.entries()]
            .map(([group, value]) => ({
                group,
                value: Math.round(value * 100) / 100
            }))
            .sort((a, b) => b.value - a.value);
        const scale: Record<string, string> = {};
        for (const d of data)
            scale[d.group] = d.value > 0 ? '#fa4d56' : '#0f62fe';
        return {
            data,
            options: {
                ...chartBase,
                axes: {
                    left: { mapsTo: 'group', scaleType: ScaleTypes.LABELS },
                    bottom: { mapsTo: 'value', includeZero: true }
                },
                color: { scale },
                height: `${Math.max(200, data.length * 40 + 60)}px`,
                title: 'Saldo je Vertrag (+ Nachzahlung / − Guthaben)',
                tooltip: { valueFormatter: (v: number) => euro(v) }
            }
        };
    }

    // #8 – Belegungs-Timeline je Wohnung (Mieter + Leerstand über das Jahr).
    type TimelineSegment = {
        label: string;
        left: number;
        width: number;
        leerstand: boolean;
        color: string;
    };
    function buildTimeline(): {
        wohnung: string;
        jahr: number;
        segments: TimelineSegment[];
    }[] {
        const tag = 86400000;
        const byWohnung = new Map<string, typeof gruppe.resultate>();
        for (const r of gruppe.resultate) {
            const arr = byWohnung.get(r.wohnungBezeichnung) ?? [];
            arr.push(r);
            byWohnung.set(r.wohnungBezeichnung, arr);
        }
        const farbeFuer = (label: string) => {
            let h = 0;
            for (let i = 0; i < label.length; i++)
                h = (h * 31 + label.charCodeAt(i)) >>> 0;
            return donutPalette[h % donutPalette.length];
        };
        return [...byWohnung.entries()].map(([wohnung, rs]) => {
            const jahr = rs[0].jahr;
            const start = Date.UTC(jahr, 0, 1);
            const jahresTage = (Date.UTC(jahr + 1, 0, 1) - start) / tag;
            const segments: TimelineSegment[] = rs
                .map((r) => {
                    const von = Math.max(Date.parse(r.nutzungVon), start);
                    const bisRaw = r.nutzungBis
                        ? Date.parse(r.nutzungBis)
                        : Date.UTC(jahr, 11, 31);
                    const bis = Math.min(bisRaw, Date.UTC(jahr, 11, 31));
                    const leerstand = r.mieterBezeichnung === 'Leerstand';
                    return {
                        label: r.mieterBezeichnung,
                        left: ((von - start) / tag / jahresTage) * 100,
                        width: (((bis - von) / tag + 1) / jahresTage) * 100,
                        leerstand,
                        color: leerstand
                            ? 'var(--cds-layer-active-01)'
                            : farbeFuer(r.mieterBezeichnung)
                    };
                })
                .sort((a, b) => a.left - b.left);
            return { wohnung, jahr, segments };
        });
    }

    const euro = (v: number) =>
        v.toLocaleString('de-DE', { style: 'currency', currency: 'EUR' });

    const chartBase = {
        legend: { enabled: false },
        height: '220px',
        tooltip: { truncation: { threshold: 999 } }
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
            left: { mapsTo: 'group', scaleType: ScaleTypes.LABELS },
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
            .map((u) => {
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
                        legend: { enabled: true, position: 'right' },
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

    const { kosten, vorjahr } = buildKostenBar();
    const warmwasserAnteilCharts = buildWarmwasserAnteilCharts();
    const kostenJeVertrag = buildKostenJeVertrag();
    const saldoBar = buildSaldoBar();
    const timeline = buildTimeline();
    const monatsLinien = Array.from(
        { length: 11 },
        (_, i) => ((i + 1) / 12) * 100
    );
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
        style="display: grid; grid-template-columns: repeat(auto-fit, minmax(30rem, 1fr)); gap: 2rem; align-items: start; margin-bottom: 2rem;"
    >
        {#if sankey}
            <div style="grid-column: 1 / -1; overflow: visible; min-width: 0;">
                <AlluvialChart
                    bind:chart={sankeyChart}
                    data={sankey.data}
                    options={sankey.options}
                />
            </div>
        {/if}

        {#if kostenJeVertrag}
            <div style="overflow-x: auto; min-width: 0;">
                <BarChartStacked
                    data={kostenJeVertrag.data}
                    options={kostenJeVertrag.options}
                />
            </div>
        {/if}

        {#if saldoBar}
            <div style="overflow: visible; min-width: 0;">
                <BarChartSimple
                    data={saldoBar.data}
                    options={saldoBar.options}
                />
            </div>
        {/if}

        {#if timeline.length > 0}
            <div style="min-width: 0;">
                <p
                    style="font-size: 0.875rem; font-weight: 600; margin: 0 0 0.75rem;"
                >
                    Belegung über das Jahr
                </p>
                {#each timeline as row}
                    <div
                        style="display: flex; align-items: center; gap: 0.75rem; margin-bottom: 0.4rem;"
                    >
                        <div
                            style="width: 9rem; flex-shrink: 0; font-size: 0.8125rem; color: var(--cds-text-secondary); text-align: right; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;"
                            title={row.wohnung}
                        >
                            {row.wohnung}
                        </div>
                        <div
                            style="position: relative; flex: 1; height: 1.75rem; background: var(--cds-layer-01); border: 1px solid var(--cds-border-subtle-01);"
                        >
                            {#each monatsLinien as m}
                                <div
                                    style="position: absolute; top: 0; bottom: 0; left: {m}%; width: 1px; background: var(--cds-border-subtle-01);"
                                ></div>
                            {/each}
                            {#each row.segments as s}
                                <div
                                    title="{s.label} ({s.width.toFixed(0)}%)"
                                    style="position: absolute; top: 2px; bottom: 2px; left: {s.left}%; width: {s.width}%; background: {s.color}; border-radius: 2px; display: flex; align-items: center; overflow: hidden; {s.leerstand
                                        ? 'background-image: repeating-linear-gradient(45deg, transparent, transparent 4px, rgba(0,0,0,0.12) 4px, rgba(0,0,0,0.12) 8px);'
                                        : ''}"
                                >
                                    <span
                                        style="font-size: 0.6875rem; padding: 0 0.35rem; white-space: nowrap; color: {s.leerstand
                                            ? 'var(--cds-text-secondary)'
                                            : '#fff'};"
                                    >
                                        {s.label}
                                    </span>
                                </div>
                            {/each}
                        </div>
                    </div>
                {/each}
                <div
                    style="display: flex; margin-left: 9.75rem; font-size: 0.6875rem; color: var(--cds-text-secondary); margin-top: 0.25rem;"
                >
                    {#each ['Jan', 'Feb', 'Mär', 'Apr', 'Mai', 'Jun', 'Jul', 'Aug', 'Sep', 'Okt', 'Nov', 'Dez'] as mo}
                        <span style="flex: 1; text-align: center;">{mo}</span>
                    {/each}
                </div>
            </div>
        {/if}

        {#each warmwasserAnteilCharts as config}
            <div style="overflow: visible; min-width: 0;">
                <div style="max-width: 24rem;">
                    <WalterDataDonutChart {config} />
                </div>
            </div>
        {/each}
    </div>

    <!-- Reihe 1: Balken (Kostenverteilung, Differenz zum Vorjahr). -->
    <div
        style="display: flex; gap: 2rem; align-items: stretch; margin-bottom: 1.5rem;"
    >
        <div style="flex: 1 1 0; min-width: 0;">
            <WalterDataBarChartSimple config={kosten} />
        </div>
        {#if vorjahr}
            <div style="flex: 1 1 0; min-width: 0;">
                <WalterDataBarChartSimple config={vorjahr} />
            </div>
        {/if}
    </div>
</div>

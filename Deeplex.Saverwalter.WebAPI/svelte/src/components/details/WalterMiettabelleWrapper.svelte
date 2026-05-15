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
    import {
        WalterMieteEntry,
        WalterZaehlerstandEntry,
        type WalterUmlageEntry,
        type WalterVertragEntry,
        type WalterZaehlerEntry
    } from '$walter/lib';
    import {
        Tab,
        Tabs,
        StructuredList,
        StructuredListBody,
        StructuredListRow
    } from 'carbon-components-svelte';
    import { WalterDataTable } from '$walter/components';
    import { convertDateCanadian } from '$walter/services/utils';
    import WalterMiettabelle from './WalterMiettabelle.svelte';
    import WalterZaehlerstand from './WalterZaehlerstand.svelte';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import WalterRechnungenTabelle from './WalterRechnungenTabelle.svelte';
    import {
        walter_data_miettabelle,
        walter_data_rechnungentabelle
    } from '../data/WalterData';

    export let vertraege: WalterVertragEntry[] = [];
    export let umlagen: WalterUmlageEntry[] = [];
    export let fetchImpl: typeof fetch;
    export let vertraegeReady = true;
    export let umlagenReady = true;

    const currentYear = new Date().getFullYear();
    let mieten: WalterMieteEntry[] = [];
    let years: number[] = [];
    let selectedYear = currentYear;
    let selected = 0;

    $: mieten = [...vertraege]
        .flatMap((vertrag) => vertrag.mieten || [])
        .sort(
            (mieteA, mieteB) =>
                new Date(mieteA.betreffenderMonat).getTime() -
                new Date(mieteB.betreffenderMonat).getTime()
        );

    $: {
        const umlageRechnungen = umlagen.flatMap(
            (umlage) => umlage.betriebskostenrechnungen || []
        );
        const discoveredYears = new Set<number>([currentYear]);

        mieten.forEach((miete) =>
            discoveredYears.add(new Date(miete.betreffenderMonat).getFullYear())
        );
        umlageRechnungen.forEach((rechnung) =>
            discoveredYears.add(rechnung.betreffendesJahr)
        );
        vertraege.forEach((vertrag) => {
            discoveredYears.add(new Date(vertrag.beginn).getFullYear());
            if (vertrag.ende) {
                discoveredYears.add(new Date(vertrag.ende).getFullYear());
            }
        });

        const sortedYears = [...discoveredYears].sort((a, b) => a - b);
        const minYear = sortedYears[0] ?? currentYear;
        const maxYear = sortedYears[sortedYears.length - 1] ?? currentYear;

        const nextYears: number[] = [];
        for (let i = minYear; i < maxYear + 1; ++i) {
            nextYears.push(i);
        }
        years = nextYears;

        if (!years.includes(selectedYear)) {
            selectedYear = years.at(-1) ?? currentYear;
        }
        selected = Math.max(0, years.findIndex((year) => year === selectedYear));
    }

    $: selectedYear = years[selected] ?? currentYear;

    type MeterTask = {
        zaehler: WalterZaehlerEntry;
    };

    function hasMeterReadingForYear(zaehler: WalterZaehlerEntry, year: number) {
        const hasInList = (zaehler.staende || []).some(
            (stand) => new Date(stand.datum).getFullYear() === year
        );
        const lastYear = zaehler.lastZaehlerstand
            ? new Date(zaehler.lastZaehlerstand.datum).getFullYear()
            : undefined;

        return hasInList || lastYear === year;
    }

    function buildMeterTasks(year: number): MeterTask[] {
        const byId = new Map<number, WalterZaehlerEntry>();
        umlagen.forEach((umlage) => {
            (umlage.zaehler || []).forEach((zaehler) => {
                if (!byId.has(zaehler.id)) {
                    byId.set(zaehler.id, zaehler);
                }
            });
        });

        return [...byId.values()]
            .filter((zaehler) => !!zaehler.permissions?.update)
            .filter((zaehler) => {
                if (!zaehler.ende) {
                    return true;
                }
                return new Date(zaehler.ende).getFullYear() >= year;
            })
            .filter((zaehler) => !hasMeterReadingForYear(zaehler, year))
            .sort((a, b) => `${a.kennnummer}`.localeCompare(`${b.kennnummer}`))
            .map((zaehler) => ({ zaehler }));
    }

    let meterTasks: MeterTask[] = [];

    $: {
        umlagen;
        meterTasks = buildMeterTasks(selectedYear);
    }

    const meterTableHeaders = [
        { key: 'kennnummer', value: 'Kennnummer' },
        { key: 'type', value: 'Typ' },
        { key: 'button', value: 'Aktion' }
    ];

    $: meterTableRows = meterTasks.map((task) => ({
        id: `meter-${task.zaehler.id}`,
        kennnummer: task.zaehler.kennnummer,
        type: task.zaehler.typ?.text || 'Zähler',
        button: (e: CustomEvent) => {
            e.stopPropagation();
            openMeterQuickAdd(task);
        }
    }));

    let addZaehlerstandOpen = false;
    let addZaehlerstandEntry: Partial<WalterZaehlerstandEntry> = {};

    function toTaskDate(year: number) {
        if (year === currentYear) {
            return convertDateCanadian(new Date());
        }
        return convertDateCanadian(new Date(year, 11, 31));
    }

    function openMeterQuickAdd(task: MeterTask) {
        addZaehlerstandEntry = {
            datum: toTaskDate(selectedYear),
            stand: task.zaehler.lastZaehlerstand?.stand,
            einheit: task.zaehler.lastZaehlerstand?.einheit,
            zaehler: {
                id: `${task.zaehler.id}`,
                text: task.zaehler.kennnummer
            }
        };
        addZaehlerstandOpen = true;
    }

    function onSubmitZaehlerstand(newValue: unknown) {
        const value = {
            ...(newValue as WalterZaehlerstandEntry),
            zaehler:
                (newValue as WalterZaehlerstandEntry).zaehler ||
                addZaehlerstandEntry.zaehler
        } as WalterZaehlerstandEntry;

        umlagen.forEach((umlage) => {
            (umlage.zaehler || []).forEach((zaehler) => {
                if (zaehler.id === +value.zaehler?.id) {
                    zaehler.staende = [...(zaehler.staende || []), value];
                    zaehler.lastZaehlerstand = value;
                }
            });
        });

        umlagen = [...umlagen];
        meterTasks = buildMeterTasks(selectedYear);
        addZaehlerstandOpen = false;
        addZaehlerstandEntry = {};
    }
</script>

<div class="homepage-heatmap-layout">
    <WalterDataWrapperQuickAdd
        title={addZaehlerstandEntry.zaehler?.text || 'Zählerstand'}
        bind:addEntry={addZaehlerstandEntry}
        addUrl={WalterZaehlerstandEntry.ApiURL}
        bind:addModalOpen={addZaehlerstandOpen}
        onSubmit={onSubmitZaehlerstand}
    >
        <WalterZaehlerstand {fetchImpl} entry={addZaehlerstandEntry} />
    </WalterDataWrapperQuickAdd>

    <!-- svelte-ignore missing-declaration -->
    <Tabs id="homepagetabs" bind:selected type="container">
        {#each years as year}
            <Tab label={`${year}`} />
        {/each}
        <svelte:fragment slot="content">
            <div style="height: 5em; width: 100%; display: block" />

            {#if umlagenReady}
                <WalterDataTable
                    layout="accordion"
                    accordionTitle="Zählerstände fällig"
                    rows={meterTableRows}
                    headers={meterTableHeaders}
                />
            {:else}
                <div class="section-placeholder">
                    <h4>Zählerstände fällig lädt...</h4>
                    <p>Umlagen und Zähler werden geladen.</p>
                </div>
            {/if}

            <StructuredList>
                <StructuredListBody>
                    <StructuredListRow>
                        {#if vertraegeReady}
                            <WalterMiettabelle
                                config={walter_data_miettabelle(vertraege, selectedYear)}
                                year={selectedYear}
                                {mieten}
                                {vertraege}
                                {fetchImpl}
                            />
                        {:else}
                            <div class="section-placeholder chart-placeholder">
                                <h4>Miettabelle lädt...</h4>
                                <p>Die Vertragsdaten werden vorbereitet.</p>
                            </div>
                        {/if}
                    </StructuredListRow>
                    <StructuredListRow>
                        {#if umlagenReady}
                            <WalterRechnungenTabelle
                                config={walter_data_rechnungentabelle(umlagen, selectedYear)}
                                year={selectedYear}
                                {fetchImpl}
                                {umlagen}
                            />
                        {:else}
                            <div class="section-placeholder chart-placeholder">
                                <h4>Rechnungstabelle lädt...</h4>
                                <p>Die Umlage-Daten werden vorbereitet.</p>
                            </div>
                        {/if}
                    </StructuredListRow>
                </StructuredListBody>
            </StructuredList>
        </svelte:fragment>
    </Tabs>
</div>

<style>
    .homepage-heatmap-layout {
        width: min(100%, 100em);
    }

    .section-placeholder {
        background: #f4f4f4;
        border: 1px solid #e0e0e0;
        border-radius: 0.5rem;
        margin: 0 0 1rem 0;
        padding: 0.875rem 1rem;
    }

    .section-placeholder h4 {
        color: #161616;
        font-size: 0.95rem;
        font-weight: 600;
        margin: 0 0 0.25rem 0;
    }

    .section-placeholder p {
        color: #525252;
        font-size: 0.85rem;
        margin: 0;
    }

    .chart-placeholder {
        margin: 0.5rem 0;
    }
</style>

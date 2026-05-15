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
    import { WalterDataWrapper } from '$walter/components';
    import { convertDateCanadian } from '$walter/services/utils';
    import WalterMiettabelle from './WalterMiettabelle.svelte';
    import WalterZaehlerstand from './WalterZaehlerstand.svelte';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import WalterRechnungenTabelle from './WalterRechnungenTabelle.svelte';
    import WalterBuchung from './WalterBuchung.svelte';
    import {
        walter_data_miettabelle,
        walter_data_rechnungentabelle
    } from '../data/WalterData';
    import { invalidateAll } from '$app/navigation';
    import type { TransaktionsInput } from '$walter/lib';
    import { emptyTransaktionsInput } from '$walter/lib';

    export let vertraege: WalterVertragEntry[];
    export let umlagen: WalterUmlageEntry[];
    export let fetchImpl: typeof fetch;

    let mieten = vertraege
        .flatMap((vertrag) => vertrag.mieten)
        .sort(
            (mieteA, mieteB) =>
                new Date(mieteA.betreffenderMonat).getTime() -
                new Date(mieteB.betreffenderMonat).getTime()
        );
    const currentYear = new Date().getFullYear();
    const currentMonth = new Date().getMonth();
    const umlageRechnungen = umlagen.flatMap(
        (umlage) => umlage.betriebskostenrechnungen
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
    const years: number[] = [];
    for (let i = minYear; i < maxYear + 1; ++i) {
        years.push(i);
    }

    let selected = years.findIndex((year) => year === new Date().getFullYear());
    selected = selected === -1 ? years.length - 1 : selected;

    type RentTask = {
        vertrag: WalterVertragEntry;
        monthIndex: number;
        monthDate: Date;
        amount: number;
        mieterAuflistung: string;
    };

    type MeterTask = {
        zaehler: WalterZaehlerEntry;
    };

    type InvoiceTask = {
        umlage: WalterUmlageEntry;
    };

    function getGrundmieteForDate(vertrag: WalterVertragEntry, date: Date) {
        const version = [...(vertrag.versionen || [])]
            .sort(
                (a, b) =>
                    new Date(a.beginn).getTime() - new Date(b.beginn).getTime()
            )
            .filter(
                (entry) => new Date(entry.beginn).getTime() <= date.getTime()
            )
            .at(-1);

        return version?.grundmiete || 0;
    }

    function getLatestMieteBefore(
        vertrag: WalterVertragEntry,
        date: Date
    ): WalterMieteEntry | undefined {
        return [...(vertrag.mieten || [])]
            .filter(
                (miete) =>
                    new Date(miete.betreffenderMonat).getTime() < date.getTime()
            )
            .sort(
                (a, b) =>
                    new Date(a.betreffenderMonat).getTime() -
                    new Date(b.betreffenderMonat).getTime()
            )
            .at(-1);
    }

    function buildRentTasks(year: number): RentTask[] {
        return vertraege
            .filter((vertrag) => !!vertrag.permissions?.update)
            .map((vertrag) => {
                const start = new Date(vertrag.beginn);
                const end = vertrag.ende ? new Date(vertrag.ende) : undefined;

                if (start.getFullYear() > year) {
                    return undefined;
                }
                if (end && end.getFullYear() < year) {
                    return undefined;
                }

                const startMonth =
                    start.getFullYear() === year ? start.getMonth() : 0;
                const endMonth =
                    end && end.getFullYear() === year ? end.getMonth() : 11;

                if (endMonth < startMonth) {
                    return undefined;
                }

                const existingMonths = new Set(
                    (vertrag.mieten || [])
                        .filter(
                            (miete) =>
                                new Date(
                                    miete.betreffenderMonat
                                ).getFullYear() === year
                        )
                        .map((miete) =>
                            new Date(miete.betreffenderMonat).getMonth()
                        )
                );

                const searchStart =
                    year === currentYear
                        ? Math.max(startMonth, currentMonth)
                        : startMonth;

                for (
                    let monthIndex = searchStart;
                    monthIndex <= endMonth;
                    monthIndex += 1
                ) {
                    if (existingMonths.has(monthIndex)) {
                        continue;
                    }

                    const monthDate = new Date(year, monthIndex, 1);
                    const latestMiete = getLatestMieteBefore(
                        vertrag,
                        monthDate
                    );
                    const amount =
                        latestMiete?.betrag ||
                        getGrundmieteForDate(vertrag, monthDate);
                    const mieterAuflistung =
                        vertrag.mieterAuflistung || 'Keine Mieter';

                    return {
                        vertrag,
                        monthIndex,
                        monthDate,
                        amount,
                        mieterAuflistung
                    };
                }

                return undefined;
            })
            .filter((task): task is RentTask => !!task)
            .sort(
                (a, b) =>
                    a.monthDate.getTime() - b.monthDate.getTime() ||
                    `${a.vertrag.wohnung?.text || ''}`.localeCompare(
                        `${b.vertrag.wohnung?.text || ''}`
                    )
            );
    }

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

    function buildInvoiceTasks(year: number): InvoiceTask[] {
        return umlagen
            .filter((umlage) => !!umlage.permissions?.update)
            .filter(
                (umlage) =>
                    !(umlage.betriebskostenrechnungen || []).some(
                        (rechnung) => rechnung.betreffendesJahr === year
                    )
            )
            .sort((a, b) => {
                const textA = `${a.typ?.text || ''} ${a.wohnungenBezeichnung || a.beschreibung || ''}`;
                const textB = `${b.typ?.text || ''} ${b.wohnungenBezeichnung || b.beschreibung || ''}`;

                return textA.localeCompare(textB);
            })
            .map((umlage) => ({ umlage }));
    }

    $: selectedYear = years[selected] ?? currentYear;

    // Task lists - these must be declared first so they exist when table rows use them
    let rentTasks: RentTask[] = [];
    let meterTasks: MeterTask[] = [];
    let invoiceTasks: InvoiceTask[] = [];

    function refreshTaskLists(year: number) {
        rentTasks = buildRentTasks(year);
        meterTasks = buildMeterTasks(year);
        invoiceTasks = buildInvoiceTasks(year);
    }

    // Reactive statements that recalculate when data or year changes
    // Dependencies must be read in the block for Svelte to track them
    $: {
        vertraege;
        mieten;
        umlagen;
        selectedYear;

        refreshTaskLists(selectedYear);
    }

    // Table headers and data transformation for WalterDataTable
    const rentTableHeaders = [
        { key: 'wohnung', value: 'Wohnung' },
        { key: 'mieterAuflistung', value: 'Mieter' },
        { key: 'month', value: 'Monat' },
        { key: 'amount', value: 'Betrag' },
        { key: 'button', value: 'Aktion' }
    ];

    const meterTableHeaders = [
        { key: 'kennnummer', value: 'Kennnummer' },
        { key: 'type', value: 'Typ' },
        { key: 'button', value: 'Aktion' }
    ];

    const invoiceTableHeaders = [
        { key: 'typ', value: 'Typ' },
        { key: 'wohnungen', value: 'Wohnungen' },
        { key: 'button', value: 'Aktion' }
    ];

    $: rentTableRows = rentTasks.map((task) => ({
        id: `rent-${task.vertrag.id}-${task.monthIndex}`,
        wohnung: task.vertrag.wohnung?.text || `Vertrag ${task.vertrag.id}`,
        mieterAuflistung: task.mieterAuflistung,
        month: new Date(task.monthDate).toLocaleDateString('de-DE', {
            month: 'short',
            year: 'numeric'
        }),
        amount: `${task.amount.toFixed(2)} €`,
        button: (e: CustomEvent) => {
            e.stopPropagation();
            openRentQuickAdd(task);
        }
    }));

    $: meterTableRows = meterTasks.map((task) => ({
        id: `meter-${task.zaehler.id}`,
        kennnummer: task.zaehler.kennnummer,
        type: task.zaehler.typ?.text || 'Zähler',
        button: (e: CustomEvent) => {
            e.stopPropagation();
            openMeterQuickAdd(task);
        }
    }));

    $: invoiceTableRows = invoiceTasks.map((task) => ({
        id: `invoice-${task.umlage.id}`,
        typ: task.umlage.typ?.text || 'Umlage',
        wohnungen:
            task.umlage.wohnungenBezeichnung ||
            task.umlage.beschreibung ||
            `Umlage ${task.umlage.id}`,
        button: (e: CustomEvent) => {
            e.stopPropagation();
            openInvoiceQuickAdd(task);
        }
    }));

    let addMieteOpen = false;
    let addZaehlerstandOpen = false;
    let addRechnungOpen = false;

    let addMieteTitle = 'Mietzahlung';
    let addZaehlerstandEntry: Partial<WalterZaehlerstandEntry> = {};
    let addRechnungTitle = 'Betriebskostenrechnung';

    let buchungsMieteInput: TransaktionsInput = emptyTransaktionsInput();
    let buchungsRechnungInput: TransaktionsInput = emptyTransaktionsInput();

    function toTaskDate(year: number) {
        if (year === currentYear) {
            return convertDateCanadian(new Date());
        }
        return convertDateCanadian(new Date(year, 11, 31));
    }

    function openRentQuickAdd(task: RentTask) {
        addMieteTitle =
            task.vertrag.wohnung?.text || `Vertrag ${task.vertrag.id}`;
        buchungsMieteInput = {
            ...emptyTransaktionsInput(),
            mieten: [
                {
                    kaltmiete: 0,
                    nkVorauszahlung: 0,
                    vertragId: task.vertrag.id as number
                }
            ]
        };
        addMieteOpen = true;
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

    function openInvoiceQuickAdd(task: InvoiceTask) {
        addRechnungTitle =
            task.umlage.typ?.text ||
            task.umlage.beschreibung ||
            'Betriebskostenrechnung';
        buchungsRechnungInput = {
            ...emptyTransaktionsInput(),
            betriebskostenEingaenge: [
                {
                    betrag: 0,
                    umlageId: task.umlage.id as number,
                    betreffendesJahr: selectedYear
                }
            ]
        };
        addRechnungOpen = true;
    }

    async function onSubmitMiete() {
        await invalidateAll();
        refreshTaskLists(selectedYear);
    }

    async function onSubmitRechnung() {
        await invalidateAll();
        refreshTaskLists(selectedYear);
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
        refreshTaskLists(selectedYear);
        addZaehlerstandOpen = false;
        addZaehlerstandEntry = {};
    }
</script>

<div class="homepage-heatmap-layout">
    <WalterDataWrapperQuickAdd
        title={addMieteTitle}
        addUrl="/api/transaktionen/buchen"
        bind:addEntry={buchungsMieteInput}
        bind:addModalOpen={addMieteOpen}
        onSubmit={onSubmitMiete}
    >
        <WalterBuchung {fetchImpl} bind:buchung={buchungsMieteInput} />
    </WalterDataWrapperQuickAdd>

    <WalterDataWrapperQuickAdd
        title={addZaehlerstandEntry.zaehler?.text || 'Zählerstand'}
        bind:addEntry={addZaehlerstandEntry}
        addUrl={WalterZaehlerstandEntry.ApiURL}
        bind:addModalOpen={addZaehlerstandOpen}
        onSubmit={onSubmitZaehlerstand}
    >
        <WalterZaehlerstand {fetchImpl} entry={addZaehlerstandEntry} />
    </WalterDataWrapperQuickAdd>

    <WalterDataWrapperQuickAdd
        title={addRechnungTitle}
        addUrl="/api/transaktionen/buchen"
        bind:addEntry={buchungsRechnungInput}
        bind:addModalOpen={addRechnungOpen}
        onSubmit={onSubmitRechnung}
    >
        <WalterBuchung {fetchImpl} bind:buchung={buchungsRechnungInput} />
    </WalterDataWrapperQuickAdd>

    <!-- svelte-ignore missing-declaration -->
    <Tabs id="homepagetabs" bind:selected type="container">
        {#each years as year}
            <Tab label={`${year}`} />
        {/each}
        <svelte:fragment slot="content">
            <div style="height: 5em; width: 100%; display: block" />

            <WalterDataWrapper
                title={`Mieten fällig (${rentTableRows.length})`}
                rows={rentTableRows}
                headers={rentTableHeaders}
            />

            <WalterDataWrapper
                title={`Zählerstände fällig (${meterTableRows.length})`}
                rows={meterTableRows}
                headers={meterTableHeaders}
            />

            <WalterDataWrapper
                title={`Rechnungen erwartet (${invoiceTableRows.length})`}
                rows={invoiceTableRows}
                headers={invoiceTableHeaders}
            />

            <StructuredList>
                <StructuredListBody>
                    <StructuredListRow>
                        <WalterMiettabelle
                            config={walter_data_miettabelle(
                                vertraege,
                                selectedYear
                            )}
                            year={selectedYear}
                            {mieten}
                            {vertraege}
                            {fetchImpl}
                        />
                    </StructuredListRow>
                    <StructuredListRow>
                        <WalterRechnungenTabelle
                            config={walter_data_rechnungentabelle(
                                umlagen,
                                selectedYear
                            )}
                            year={selectedYear}
                            {fetchImpl}
                            {umlagen}
                        />
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
</style>

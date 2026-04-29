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
        WalterBetriebskostenrechnungEntry,
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
    import WalterMiete from './WalterMiete.svelte';
    import WalterZaehlerstand from './WalterZaehlerstand.svelte';
    import WalterBetriebskostenrechnung from './WalterBetriebskostenrechnung.svelte';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import WalterRechnungenTabelle from './WalterRechnungenTabelle.svelte';
    import {
        walter_data_miettabelle,
        walter_data_rechnungentabelle
    } from '../data/WalterData';
    import { openModal } from '$walter/store';

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

    let addMieteEntry: Partial<WalterMieteEntry> = {};
    let addZaehlerstandEntry: Partial<WalterZaehlerstandEntry> = {};
    let addRechnungEntry: Partial<WalterBetriebskostenrechnungEntry> = {};

    function toTaskDate(year: number) {
        if (year === currentYear) {
            return convertDateCanadian(new Date());
        }

        return convertDateCanadian(new Date(year, 11, 31));
    }

    function openRentQuickAdd(task: RentTask) {
        addMieteEntry = {
            zahlungsdatum: convertDateCanadian(new Date()),
            betreffenderMonat: convertDateCanadian(task.monthDate),
            vertrag: {
                id: `${task.vertrag.id}`,
                text: task.vertrag.wohnung?.text || `Vertrag ${task.vertrag.id}`
            },
            betrag: task.amount
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
        addRechnungEntry = {
            datum: convertDateCanadian(new Date()),
            betreffendesJahr: selectedYear,
            typ: task.umlage.typ
                ? {
                      id: task.umlage.typ.id,
                      text: task.umlage.typ.text
                  }
                : undefined,
            umlage: {
                id: task.umlage.id,
                text:
                    task.umlage.typ?.text ||
                    task.umlage.beschreibung ||
                    'Umlage'
            }
        };

        addRechnungOpen = true;
    }

    function onSubmitMiete(newValue: unknown) {
        const value = {
            ...(newValue as WalterMieteEntry),
            vertrag:
                (newValue as WalterMieteEntry).vertrag || addMieteEntry.vertrag
        } as WalterMieteEntry;
        const vertrag = vertraege.find(
            (entry) => entry.id === +value.vertrag?.id
        );
        if (!vertrag) {
            return;
        }

        vertrag.mieten.push(value);
        mieten = [...mieten, value].sort(
            (a, b) =>
                new Date(a.betreffenderMonat).getTime() -
                new Date(b.betreffenderMonat).getTime()
        );
        vertraege = [...vertraege];
        refreshTaskLists(selectedYear);
        addMieteOpen = false;
        addMieteEntry = {};
    }

    function dateToMonthKey(value: string | undefined) {
        if (!value) {
            return undefined;
        }

        const parsed = new Date(value);
        if (Number.isNaN(parsed.getTime())) {
            return undefined;
        }

        return `${parsed.getFullYear()}-${`${parsed.getMonth() + 1}`.padStart(2, '0')}`;
    }

    function confirmDuplicateRentMonth(entryToCheck: unknown) {
        const miete = entryToCheck as Partial<WalterMieteEntry>;
        const vertragId = +(miete.vertrag?.id || 0);
        const monthKey = dateToMonthKey(miete.betreffenderMonat);

        const hasDuplicate =
            !miete.id &&
            vertragId > 0 &&
            !!monthKey &&
            mieten.some(
                (row) =>
                    +row.vertrag?.id === vertragId &&
                    dateToMonthKey(row.betreffenderMonat) === monthKey
            );

        if (!hasDuplicate) {
            return Promise.resolve(true);
        }

        return new Promise<boolean>((resolve) => {
            openModal({
                modalHeading: 'Miete bereits vorhanden',
                content:
                    'Für diesen Vertrag gibt es im ausgewählten Monat bereits mindestens eine Miete. Möchtest du trotzdem speichern?',
                primaryButtonText: 'Trotzdem speichern',
                submit: async () => {
                    resolve(true);
                    return true;
                },
                cancel: () => resolve(false),
                danger: false
            });
        });
    }

    function onDeleteMonthMiete(value: WalterMieteEntry) {
        const vertrag = vertraege.find(
            (entry) => entry.id === +value.vertrag?.id
        );
        if (vertrag) {
            vertrag.mieten = (vertrag.mieten || []).filter(
                (miete) => miete.id !== value.id
            );
        }

        mieten = mieten.filter((miete) => miete.id !== value.id);
        vertraege = [...vertraege];
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

    function onSubmitRechnung(newValue: unknown) {
        const value = {
            ...(newValue as WalterBetriebskostenrechnungEntry),
            umlage:
                (newValue as WalterBetriebskostenrechnungEntry).umlage ||
                addRechnungEntry.umlage
        } as WalterBetriebskostenrechnungEntry;
        const umlage = umlagen.find((entry) => entry.id === +value.umlage?.id);
        if (!umlage) {
            return;
        }

        umlage.betriebskostenrechnungen.push(value);
        umlagen = [...umlagen];
        refreshTaskLists(selectedYear);
        addRechnungOpen = false;
        addRechnungEntry = {};
    }
</script>

<div class="homepage-heatmap-layout">
    <WalterDataWrapperQuickAdd
        title={addMieteEntry.vertrag?.text || 'Miete'}
        bind:addEntry={addMieteEntry}
        addUrl={WalterMieteEntry.ApiURL}
        bind:addModalOpen={addMieteOpen}
        beforeSubmit={confirmDuplicateRentMonth}
        onSubmit={onSubmitMiete}
    >
        <WalterMiete
            entry={addMieteEntry}
            {mieten}
            {vertraege}
            {onDeleteMonthMiete}
            onRequestCloseModal={() => (addMieteOpen = false)}
        />
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
        title={addRechnungEntry.umlage?.text || 'Rechnung'}
        bind:addEntry={addRechnungEntry}
        addUrl={WalterBetriebskostenrechnungEntry.ApiURL}
        bind:addModalOpen={addRechnungOpen}
        onSubmit={onSubmitRechnung}
    >
        <WalterBetriebskostenrechnung
            {fetchImpl}
            entry={addRechnungEntry}
            rechnungen={umlagen.flatMap(
                (entry) => entry.betriebskostenrechnungen
            )}
        />
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

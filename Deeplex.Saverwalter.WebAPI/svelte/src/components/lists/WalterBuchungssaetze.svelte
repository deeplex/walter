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
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import { WalterDataTable } from '$walter/components';
    import { WalterBuchungssatzEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';
    import { convertEuro } from '$walter/services/utils';

    export let fetchImpl: typeof fetch;
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    // Feste Zeilen (z.B. die Buchungssätze einer Transaktion). Wenn nicht
    // gesetzt, lädt die Tabelle serverseitig paginiert — optional auf ein
    // Buchungskonto gefiltert.
    export let rows: WalterBuchungssatzEntry[] | undefined = undefined;
    export let kontoId: number | undefined = undefined;
    // Zeigt im Kontoblatt den OPOS-Status je Satz — nur für Ausgleichskonten
    // sinnvoll, auf Summenkonten gibt es keine offenen Posten.
    export let oposStatus = false;

    // Mit Konto-Filter wird die Liste zum Kontoblatt: Soll/Haben aus Sicht
    // des Kontos statt der Konten-Auflistung des ganzen Satzes.
    const headers =
        kontoId !== undefined
            ? [
                  { key: 'nummer', value: 'Nr.' },
                  { key: 'buchungsdatum', value: 'Datum' },
                  { key: 'beschreibung', value: 'Beschreibung' },
                  { key: 'kontoSoll', value: 'Soll' },
                  { key: 'kontoHaben', value: 'Haben' },
                  ...(oposStatus
                      ? [{ key: 'opos', value: 'OPOS', sort: false as const }]
                      : []),
                  { key: 'status', value: 'Status', sort: false as const }
              ]
            : [
                  { key: 'nummer', value: 'Nr.' },
                  { key: 'buchungsdatum', value: 'Datum' },
                  { key: 'beschreibung', value: 'Beschreibung' },
                  { key: 'sollKonten', value: 'Soll' },
                  { key: 'habenKonten', value: 'Haben' },
                  { key: 'betrag', value: 'Betrag' },
                  { key: 'status', value: 'Status', sort: false as const }
              ];

    function statusTag(satz: WalterBuchungssatzEntry) {
        if (satz.istStorniert) {
            return { text: 'Storniert', tag: 'red' };
        }
        if (satz.istStorno) {
            return { text: 'Storno', tag: 'purple' };
        }
        return '';
    }

    function oposTag(satz: WalterBuchungssatzEntry) {
        if (satz.kontoOffen === undefined) {
            return '';
        }
        return satz.kontoOffen <= 0.005
            ? { text: 'Ausgeglichen', tag: 'green' }
            : { text: `Offen: ${convertEuro(satz.kontoOffen)}`, tag: 'red' };
    }

    // Getter (nummer) überleben kein Spread — explizit übernehmen.
    const toRow = (satz: WalterBuchungssatzEntry): DataTableRow => ({
        ...satz,
        nummer: satz.nummer,
        status: statusTag(satz),
        opos: oposTag(satz)
    });

    $: mappedRows = rows?.map(toRow);

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.buchungssatz(e.detail.id);
    const rowHref = (row: DataTableRow) => `/buchungssaetze/${row.id}`;

    const fetchData =
        rows === undefined
            ? (
                  p: NonNullable<
                      Parameters<typeof WalterBuchungssatzEntry.GetPaged>[1]
                  >
              ) =>
                  kontoId !== undefined
                      ? WalterBuchungssatzEntry.GetPagedByKonto(
                            fetchImpl,
                            p,
                            kontoId
                        )
                      : WalterBuchungssatzEntry.GetPaged<WalterBuchungssatzEntry>(
                            fetchImpl,
                            p
                        )
            : undefined;

    const transformRow = (row: DataTableRow) =>
        toRow(row as WalterBuchungssatzEntry);
</script>

<WalterDataTable
    {on_click_row}
    {rowHref}
    layout={title !== undefined ? 'accordion' : 'inline'}
    accordionTitle={title}
    rows={mappedRows}
    {fetchData}
    {transformRow}
    initialSortBy="buchungsdatum"
    initialSortDir="desc"
    {headers}
    {fullHeight}
/>

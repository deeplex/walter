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

    import {
        WalterBetriebskostenrechnung,
        WalterDataTable
    } from '$walter/components';
    import {
        WalterBetriebskostenrechnungEntry,
        validateBetriebskostenrechnung
    } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let fetchImpl: typeof fetch;
    export let rows: WalterBetriebskostenrechnungEntry[] | undefined =
        undefined;
    export let entry: Partial<WalterBetriebskostenrechnungEntry> | undefined =
        {};

    const headers = [
        { key: 'typ.text', value: 'Typ' },
        { key: 'umlage.text', value: 'Wohnungen' },
        { key: 'betreffendesJahr', value: 'Betreffendes Jahr' },
        { key: 'betrag', value: 'Betrag' },
        { key: 'datum', value: 'Datum' },
        { key: 'ausgeglichen', value: '⚖' }
    ];

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.betriebskostenrechnung(e.detail.id);
    const rowHref = (row: DataTableRow) =>
        `/betriebskostenrechnungen/${row.id}`;

    function enrich(r: WalterBetriebskostenrechnungEntry) {
        return { ...r, ausgeglichen: r.isBalanced ? '✓' : '✗' };
    }

    const transformRow = (row: DataTableRow) =>
        enrich(row as WalterBetriebskostenrechnungEntry);

    const fetchData =
        rows === undefined
            ? (
                  p: Parameters<
                      typeof WalterBetriebskostenrechnungEntry.GetPaged
                  >[1]
              ) =>
                  WalterBetriebskostenrechnungEntry.GetPaged<WalterBetriebskostenrechnungEntry>(
                      fetchImpl,
                      p
                  )
            : undefined;

    $: submitDisabled = !validateBetriebskostenrechnung(entry);

    $: enrichedRows = rows
        ? [...rows].map(enrich).sort((a, b) => {
              const yearA = a.betreffendesJahr || 0;
              const yearB = b.betreffendesJahr || 0;
              if (yearA !== yearB) return yearB - yearA;
              const dateA = new Date(a.datum).getTime();
              const dateB = new Date(b.datum).getTime();
              if (!Number.isNaN(dateA) && !Number.isNaN(dateB))
                  return dateB - dateA;
              return `${b.datum || ''}`.localeCompare(`${a.datum || ''}`);
          })
        : undefined;
</script>

<WalterDataTable
    addUrl={WalterBetriebskostenrechnungEntry.ApiURL}
    addEntry={entry}
    {submitDisabled}
    layout={title !== undefined ? 'accordion' : 'inline'}
    accordionTitle={title}
    quickAddTitle={title}
    {on_click_row}
    {rowHref}
    rows={enrichedRows}
    {fetchData}
    {transformRow}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterBetriebskostenrechnung {fetchImpl} bind:entry />
    {/if}
</WalterDataTable>

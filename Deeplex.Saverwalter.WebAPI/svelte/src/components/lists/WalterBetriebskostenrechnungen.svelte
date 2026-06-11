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
    import { convertEuro } from '$walter/services/utils';
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
        { key: 'bezahltStatus', value: 'Bezahlt', sort: false as const },
        { key: 'nkStatus', value: 'NK-Verteilung', sort: false as const }
    ];

    // OPOS-Sicht der Rechnung: Zahlungseingang und Verteilung auf die
    // NK-Anteile als Status-Tags direkt in der Rechnungsliste.
    function enrich(r: WalterBetriebskostenrechnungEntry) {
        const nkOffen = Math.max(0, r.betrag - r.verteilt);
        return {
            ...r,
            bezahltStatus: r.isBezahlt
                ? { text: 'Bezahlt', tag: 'green' }
                : { text: 'Offen', tag: 'red' },
            nkStatus:
                nkOffen <= 0.005
                    ? { text: 'Verteilt', tag: 'green' }
                    : {
                          text: `Offen: ${convertEuro(nkOffen)}`,
                          tag: 'warm-gray'
                      }
        };
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
    rows={enrichedRows}
    {fetchData}
    {transformRow}
    {headers}
    {fullHeight}
    on_click_row={(e) => navigation.buchungssatz(e.detail.id)}
>
    {#if entry}
        <WalterBetriebskostenrechnung {fetchImpl} bind:entry />
    {/if}
</WalterDataTable>

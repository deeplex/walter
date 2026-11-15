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

<!--
    Transaktionen als aufklappbare Tabelle: aufgeklappt die Zuordnung, d. h. wofür die
    einzelnen Teile gebucht wurden, und der Link zur Transaktion.
-->
<script lang="ts">
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import { WalterDataTable, WalterTransaktion } from '$walter/components';
    import WalterItemList from '../elements/WalterItemList.svelte';
    import { fileURL } from '$walter/services/files';
    import {
        WalterTransaktionEntry,
        zuordnungAsItems,
        zuordnungZelle
    } from '$walter/lib';

    export let fetchImpl: typeof fetch;
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterTransaktionEntry> | undefined = {};
    export let rows: WalterTransaktionEntry[] | undefined = undefined;
    /** Ohne `rows`: seitenweise vom Server, eingeschränkt z. B. auf `{ kontaktId }`. */
    export let filter: Record<string, string | number> | undefined = undefined;

    let transaktionIsValid = false;

    const headers = [
        { key: 'zahler.text', value: 'Zahler' },
        { key: 'betrag', value: 'Betrag' },
        { key: 'zahlungsempfaenger.text', value: 'Zahlungsempfänger' },
        { key: 'zahlungsdatum', value: 'Zahlungsdatum' },
        { key: 'zuordnungZelle', value: 'Zuordnung', sort: false as const },
        { key: 'verwendungszweck', value: 'Memo' }
    ];

    // „Zuordnung“ kommt vom Server (bebuchte Konten und Ausgleiche).
    const transformRow = (row: DataTableRow) => ({
        ...row,
        zuordnungZelle: zuordnungZelle(
            (row as WalterTransaktionEntry).zuordnung
        )
    });
    $: tableRows = rows?.map((r) => transformRow(r as unknown as DataTableRow));

    const zuordnungVon = (row: DataTableRow) =>
        (row as unknown as WalterTransaktionEntry).zuordnung ?? [];
    const notizVon = (row: DataTableRow) =>
        (row as unknown as WalterTransaktionEntry).notiz;

    const fetchData =
        rows === undefined
            ? (p: Parameters<typeof WalterTransaktionEntry.GetPaged>[1]) =>
                  WalterTransaktionEntry.GetPaged<WalterTransaktionEntry>(
                      fetchImpl,
                      { ...p, filter }
                  )
            : undefined;
</script>

<WalterDataTable
    addUrl={WalterTransaktionEntry.ApiURL}
    expandable
    addEntry={entry}
    submitDisabled={!transaktionIsValid}
    layout={title !== undefined ? 'accordion' : 'inline'}
    accordionTitle={title}
    quickAddTitle={title}
    rows={tableRows}
    {fetchData}
    {transformRow}
    initialSortBy="zahlungsdatum"
    initialSortDir="desc"
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterTransaktion {fetchImpl} bind:isValid={transaktionIsValid} />
    {/if}
    <svelte:fragment slot="expanded-row" let:row>
        <WalterItemList
            items={zuordnungAsItems(zuordnungVon(row))}
            leerText="Nicht zugeordnet."
            moreHref={`/transaktionen/${row.id}`}
            moreText="Transaktion öffnen →"
            notiz={notizVon(row)}
            attachmentsUrl={fileURL.transaktion(`${row.id}`)}
            {fetchImpl}
        />
    </svelte:fragment>
</WalterDataTable>

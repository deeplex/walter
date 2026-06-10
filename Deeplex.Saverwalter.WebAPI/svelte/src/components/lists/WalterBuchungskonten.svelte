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
    import {
        kontoAnzeigeSaldo,
        kontoStatusTag,
        type WalterBuchungskontoEntry
    } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let rows: Partial<WalterBuchungskontoEntry>[] = [];

    // Konten-Referenzen einer Entität tragen ihre Funktion ("Mietforderungen",
    // "Zahlungseingänge", ...) — die erklärt das Konto besser als der Kontotyp.
    $: hasFunktion = rows.some((row) => row.funktion);
    $: headers = [
        ...(hasFunktion
            ? [{ key: 'funktion', value: 'Funktion' }]
            : [{ key: 'kontotyp', value: 'Typ' }]),
        { key: 'bezeichnung', value: 'Bezeichnung' },
        { key: 'saldo', value: 'Saldo' },
        { key: 'status', value: 'Status', sort: false as const }
    ];

    // Saldo auf die natürliche Kontoseite gedreht; Status nur für
    // Ausgleichskonten (Ausgeglichen / Offen / Guthaben).
    $: tableRows = rows.map((row) => ({
        ...row,
        id: row.id,
        saldo: kontoAnzeigeSaldo(row),
        status: kontoStatusTag(row) ?? ''
    })) as DataTableRow[];

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.buchungskonto(e.detail.id);
    const rowHref = (row: DataTableRow) => `/buchungskonten/${row.id}`;
</script>

<WalterDataTable
    {on_click_row}
    {rowHref}
    layout={title !== undefined ? 'accordion' : 'inline'}
    accordionTitle={title}
    rows={tableRows}
    {headers}
    {fullHeight}
/>

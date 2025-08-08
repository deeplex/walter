<!-- Copyright (C) 2023-2025  Kai Lawrence -->
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
    import { WalterDataWrapper } from '$walter/components';
    import { WalterAbrechnungsresultatEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    const headers = [
        { key: 'jahr', value: 'Jahr' },
        { key: 'saldo', value: 'Saldo' },
        { key: 'istAbgesendet', value: 'Ist abgesendet' },
        { key: 'istBeglichen', value: 'Ist beglichen' }
    ];
    export let rows: WalterAbrechnungsresultatEntry[];

    let actualRows = rows
        .map((row) => {
            return {
                id: row.id,
                jahr: row.jahr,
                saldo: row.vorauszahlung - row.kaltmiete - row.rechnungsbetrag,
                istAbgesendet: row.abgesendet ? 'Ja' : 'Nein',
                istBeglichen: row.istBeglichen ? 'Ja' : 'Nein'
            };
        })
        .sort((a, b) => b.jahr - a.jahr);

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.abrechnungsresultat(`${e.detail.id}`);

    export let title: string | undefined = undefined;
    const readonly = true;
    const fullHeight = false;
</script>

<WalterDataWrapper
    {on_click_row}
    {readonly}
    {fullHeight}
    {title}
    rows={actualRows}
    {headers}
></WalterDataWrapper>

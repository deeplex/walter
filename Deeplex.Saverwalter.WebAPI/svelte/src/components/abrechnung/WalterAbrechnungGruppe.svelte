<!-- Copyright (C) 2023-2024  Kai Lawrence -->
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
    import { WalterDataTable } from '$walter/components';
    import type { WalterBetriebskostenrechnungEntry } from '$walter/lib';
    import { convertDateCanadian } from '$walter/services/utils';
    import type { WalterRechnungEntry } from '$walter/types/WalterBetriebskostenabrechnung.type';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
    import WalterBetriebskostenrechnung from '../details/WalterBetriebskostenrechnung.svelte';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import { goto_or_create_rechnung } from './WalterAbrechnungGruppe';

    export let rows: WalterRechnungEntry[];
    export let year: number;
    export let fetchImpl: typeof fetch;

    let quickAddEntry: Partial<WalterBetriebskostenrechnungEntry> = {};

    const headers = [
        { key: 'typ', value: 'Kostenanteil' },
        { key: 'schluessel', value: 'Schlüssel' },
        { key: 'gesamtBetrag', value: 'Betrag' },
        { key: 'betragLetztesJahr', value: 'Betrag (letztes Jahr)' },
        { key: 'anteil', value: 'Anteil' },
        { key: 'betrag', value: 'Kosten' },
        { key: 'button', value: 'Hinzufügen' }
    ];

    function add(e: CustomEvent, rechnung: WalterRechnungEntry) {
        e.stopPropagation();

        quickAddEntry = {
            betrag: rechnung.gesamtBetrag || rechnung.betragLetztesJahr,
            betreffendesJahr: year,
            datum: convertDateCanadian(new Date()),
            typ: { id: `${rechnung.typId}`, text: rechnung.typ },
            umlage: { id: `${rechnung.id}`, text: rechnung.typ },
            id: rechnung.rechnungId || undefined
        };

        open = true;
    }

    const rowsAdd = rows.map((row) => ({
        ...row,
        button: row.rechnungId ? 'disabled' : (e: CustomEvent) => add(e, row)
    }));

    const on_click_row = (e: CustomEvent<DataTableRow>) => {
        goto_or_create_rechnung(e.detail as WalterRechnungEntry, year);
    };

    let open = false;
</script>

<WalterDataWrapperQuickAdd
    title={quickAddEntry.typ?.text || 'Rechnung'}
    addEntry={quickAddEntry}
    addUrl={quickAddEntry.id ? '' : '/api/betriebskostenrechnung/'}
    bind:addModalOpen={open}
>
    <WalterBetriebskostenrechnung
        readonly={quickAddEntry.id !== undefined}
        {fetchImpl}
        entry={quickAddEntry}
    />
</WalterDataWrapperQuickAdd>

<WalterDataTable
    size="short"
    fullHeight
    {on_click_row}
    {headers}
    rows={rowsAdd}
/>

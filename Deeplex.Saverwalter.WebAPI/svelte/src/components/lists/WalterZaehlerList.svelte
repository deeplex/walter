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
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import { WalterDataWrapper, WalterZaehler } from '$walter/components';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import WalterZaehlerstand from '../details/WalterZaehlerstand.svelte';
    import { convertDateCanadian } from '$walter/services/utils';
    import { WalterZaehlerEntry, WalterZaehlerstandEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    const headers = [
        { key: 'kennnummer', value: 'Kennnummer' },
        { key: 'adresse.anschrift', value: 'Adresse'},
        { key: 'wohnung.text', value: 'Wohnung' },
        { key: 'typ.text', value: 'Typ' },
        { key: 'lastZaehlerstand.datum', value: 'Letztes Ablesedatum' },
        { key: 'lastZaehlerstand.stand', value: 'Letzter Stand' },
        { key: 'lastZaehlerstand.einheit', value: 'Einheit' },
        { key: 'ende', value: 'Ende' },
        { key: 'button', value: 'Stand hinzufügen' }
    ];

    export let rows: WalterZaehlerEntry[];
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterZaehlerEntry> | undefined = undefined;
    export let fetchImpl: typeof fetch;

    export let ablesedatum: Date = new Date();
    let quickAddEntry: Partial<WalterZaehlerstandEntry> = {};

    function add(e: CustomEvent, zaehler: WalterZaehlerEntry) {
        e.stopPropagation();
        quickAddEntry = {
            datum: convertDateCanadian(ablesedatum),
            stand: zaehler.lastZaehlerstand?.stand,
            einheit: zaehler.lastZaehlerstand?.einheit,
            zaehler: { id: `${zaehler.id}`, text: zaehler.kennnummer }
        };

        open = true;
    }
    const rowsAdd = rows.map((row) => ({
        ...row,
        button:
            row.lastZaehlerstand?.datum === convertDateCanadian(ablesedatum)
                ? 'disabled'
                : (e: CustomEvent) => add(e, row)
    }));

    let open = false;

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.zaehler(e.detail.id);
</script>

<WalterDataWrapperQuickAdd
    title={quickAddEntry.zaehler?.text || 'Zähler'}
    addEntry={quickAddEntry}
    addUrl={WalterZaehlerstandEntry.ApiURL}
    bind:addModalOpen={open}
>
    <WalterZaehlerstand {fetchImpl} entry={quickAddEntry} />
</WalterDataWrapperQuickAdd>

<WalterDataWrapper
    addUrl={WalterZaehlerEntry.ApiURL}
    addEntry={entry}
    {title}
    {on_click_row}
    rows={rowsAdd}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterZaehler {fetchImpl} {entry} />
    {/if}
</WalterDataWrapper>

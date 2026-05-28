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
    import WalterErhaltungsaufwendung from '../details/WalterErhaltungsaufwendung.svelte';
    import WalterSimpleList from './WalterSimpleList.svelte';
    import {
        WalterErhaltungsaufwendungEntry,
        validateErhaltungsaufwendung
    } from '$walter/lib';

    const headers = [
        { key: 'bezeichnung', value: 'Bezeichnung' },
        { key: 'aussteller.text', value: 'Aussteller' },
        { key: 'wohnung.text', value: 'Wohnung' },
        { key: 'betrag', value: 'Betrag' },
        { key: 'datum', value: 'Datum' }
    ];

    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let fetchImpl: typeof fetch;
    export let rows: WalterErhaltungsaufwendungEntry[] | undefined = undefined;
    export let entry: Partial<WalterErhaltungsaufwendungEntry> | undefined = {};
</script>

<WalterSimpleList
    entityClass={WalterErhaltungsaufwendungEntry}
    validate={validateErhaltungsaufwendung}
    {headers}
    formComponent={WalterErhaltungsaufwendung}
    {fetchImpl}
    {entry}
    {rows}
    {title}
    {fullHeight}
/>

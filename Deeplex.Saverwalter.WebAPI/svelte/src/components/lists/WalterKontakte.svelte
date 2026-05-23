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
    import WalterKontakt from '../details/WalterKontakt.svelte';
    import WalterSimpleList from './WalterSimpleList.svelte';
    import { WalterKontaktEntry, validateKontakt } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    const headers = [
        { key: 'bezeichnung', value: 'Name' },
        { key: 'adresse.anschrift', value: 'Anschrift' },
        { key: 'telefon', value: 'Telefon' },
        { key: 'mobil', value: 'Mobil' },
        { key: 'email', value: 'E-Mail' }
    ];

    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let fetchImpl: typeof fetch;
    export let rows: WalterKontaktEntry[] | undefined = undefined;
    export let entry: Partial<WalterKontaktEntry> = {
        permissions: { read: true, update: true, remove: true }
    };
</script>

<WalterSimpleList
    entityClass={WalterKontaktEntry}
    validate={validateKontakt}
    {headers}
    navFn={navigation.kontakt}
    routeBase="kontakte"
    formComponent={WalterKontakt}
    {fetchImpl}
    {entry}
    {rows}
    {title}
    {fullHeight}
    initialSortBy="bezeichnung"
    initialSortDir="asc"
/>

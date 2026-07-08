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
    import WalterGarage from '../details/WalterGarage.svelte';
    import WalterSimpleList from './WalterSimpleList.svelte';
    import { WalterGarageEntry, validateGarage } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';
    import { get } from 'svelte/store';
    import { UserRole, authState } from '$walter/services/auth';

    const headers = [
        { key: 'kennung', value: 'Kennung' },
        { key: 'besitzer.text', value: 'Besitzer' },
        { key: 'adresse.anschrift', value: 'Adresse' },
        { key: 'aktuelleMieter', value: 'Aktueller Mieter' }
    ];

    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let fetchImpl: typeof fetch;
    export let entry: Partial<WalterGarageEntry> | undefined = {};
    export let rows: WalterGarageEntry[] | undefined = undefined;

    const userRole = authState && get(authState)?.role;
    const readonly = userRole !== UserRole.Owner && userRole !== UserRole.Admin;
</script>

<WalterSimpleList
    entityClass={WalterGarageEntry}
    validate={validateGarage}
    {headers}
    navFn={navigation.garage}
    routeBase="garagen"
    formComponent={WalterGarage}
    {fetchImpl}
    {entry}
    {rows}
    {title}
    {fullHeight}
    {readonly}
    initialSortDir="asc"
/>

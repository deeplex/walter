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
    import WalterUmlagetyp from '../details/WalterUmlagetyp.svelte';
    import WalterSimpleList from './WalterSimpleList.svelte';
    import { WalterUmlagetypEntry, validateUmlagetyp } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    const headers = [{ key: 'bezeichnung', value: 'Bezeichnung' }];

    export let fullHeight = false;
    export let rows: WalterUmlagetypEntry[] | undefined = undefined;
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterUmlagetypEntry> | undefined = {};
    export let fetchImpl: typeof fetch | undefined = undefined;
</script>

<WalterSimpleList
    entityClass={WalterUmlagetypEntry}
    validate={validateUmlagetyp}
    {headers}
    navFn={navigation.umlagetyp}
    routeBase="umlagetypen"
    formComponent={WalterUmlagetyp}
    {fetchImpl}
    {entry}
    {rows}
    {title}
    {fullHeight}
/>

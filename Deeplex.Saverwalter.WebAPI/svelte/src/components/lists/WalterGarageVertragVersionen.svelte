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
    import WalterGarageVertragVersion from '../details/WalterGarageVertragVersion.svelte';
    import WalterSimpleList from './WalterSimpleList.svelte';
    import {
        WalterGarageVertragVersionEntry,
        validateGarageVertragVersion
    } from '$walter/lib';

    const headers = [
        { key: 'beginn', value: 'Beginn' },
        { key: 'garagenMiete', value: 'Garagenmiete (€)' }
    ];

    export let rows: WalterGarageVertragVersionEntry[] | undefined = undefined;
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterGarageVertragVersionEntry> | undefined = {};
    export let fetchImpl: typeof fetch;

    const noNav = (_id: number) => {};
</script>

<WalterSimpleList
    entityClass={WalterGarageVertragVersionEntry}
    validate={validateGarageVertragVersion}
    {headers}
    navFn={noNav}
    routeBase="garage-vertrag-versionen"
    formComponent={WalterGarageVertragVersion}
    {fetchImpl}
    {entry}
    {rows}
    {title}
    {fullHeight}
/>

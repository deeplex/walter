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
    import WalterVertragVersion from '../details/WalterVertragVersion.svelte';
    import WalterSimpleList from './WalterSimpleList.svelte';
    import {
        WalterVertragVersionEntry,
        validateVertragVersion
    } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    const headers = [
        { key: 'beginn', value: 'Beginn' },
        { key: 'personenzahl', value: 'Personenzahl' },
        { key: 'grundmiete', value: 'Grundmiete' },
        { key: 'notiz', value: 'Notiz' }
    ];

    export let rows: WalterVertragVersionEntry[] | undefined = undefined;
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterVertragVersionEntry> | undefined = {};
</script>

<WalterSimpleList
    entityClass={WalterVertragVersionEntry}
    validate={validateVertragVersion}
    {headers}
    navFn={navigation.vertragversion}
    routeBase="vertragversionen"
    formComponent={WalterVertragVersion}
    {entry}
    {rows}
    {title}
    {fullHeight}
/>

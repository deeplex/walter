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
    import WalterZaehlerstand from '../details/WalterZaehlerstand.svelte';
    import WalterSimpleList from './WalterSimpleList.svelte';
    import { WalterZaehlerstandEntry, validateZaehlerstand } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    const headers = [
        { key: 'datum', value: 'Datum' },
        { key: 'stand', value: 'Stand' },
        { key: 'einheit', value: 'Einheit' }
    ];

    export let fetchImpl: typeof fetch;
    export let rows: WalterZaehlerstandEntry[] | undefined = undefined;
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterZaehlerstandEntry> | undefined = {};
</script>

<WalterSimpleList
    entityClass={WalterZaehlerstandEntry}
    validate={validateZaehlerstand}
    {headers}
    navFn={navigation.zaehlerstand}
    routeBase="zaehlerstaende"
    formComponent={WalterZaehlerstand}
    {fetchImpl}
    {entry}
    {rows}
    {title}
    {fullHeight}
/>

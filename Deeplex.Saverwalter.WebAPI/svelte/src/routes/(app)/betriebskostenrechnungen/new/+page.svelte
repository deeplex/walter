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
    import { page } from '$app/stores';
    import {
        WalterGrid,
        WalterHeaderNew,
        WalterBetriebskostenrechnung
    } from '$walter/components';
    import type { WalterBetriebskostenrechnungEntry } from '$walter/lib';
    import { convertDateCanadian } from '$walter/services/utils';
    import { onMount } from 'svelte';
    import type { PageData } from './$types';
    import { walter_selection } from '$walter/services/requests';

    export let data: PageData;

    const entry: Partial<WalterBetriebskostenrechnungEntry> = {
        datum: convertDateCanadian(new Date())
    };
    let searchParams: URLSearchParams = new URL($page.url).searchParams;

    onMount(async () => {
        const betriebskostenTypId = searchParams.get('typ');

        const umlagetypen = await walter_selection.umlagetypen(data.fetchImpl);
        const umlagen_wohnungen = await walter_selection.umlagen_wohnungen(
            data.fetchImpl
        );

        if (betriebskostenTypId) {
            entry.typ = umlagetypen.find((e) => +e.id === +betriebskostenTypId);
        }

        const umlageId = searchParams.get('umlage');
        if (umlageId) {
            entry.umlage = umlagen_wohnungen.find((e) => +e.id === +umlageId);
        }

        const jahr = searchParams.get('jahr');
        if (jahr) {
            entry.betreffendesJahr = +jahr;
        } else {
            entry.betreffendesJahr = new Date().getFullYear() - 1;
        }
    });
</script>

<WalterHeaderNew apiURL={data.apiURL} {entry} title={data.title} />

<WalterGrid>
    <WalterBetriebskostenrechnung fetchImpl={data.fetchImpl} {entry} />
</WalterGrid>

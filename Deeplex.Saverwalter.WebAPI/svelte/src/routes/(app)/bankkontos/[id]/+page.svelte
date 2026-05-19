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
    import {
        WalterHeaderDetail,
        WalterGrid,
        WalterBankkonto,
        WalterLinks,
        WalterTransaktionen
    } from '$walter/components';
    import type { WalterTransaktionEntry } from '$walter/lib';
    import { convertDateCanadian } from '$walter/services/utils';
    import type { PageData } from './$types';

    export let data: PageData;

    let title = data.entry.bezeichnung ?? `Bankkonto ${data.id}`;
    $: {
        title = data.entry.bezeichnung ?? `Bankkonto ${data.id}`;
    }

    const transaktion: Partial<WalterTransaktionEntry> = {
        zahler: { id: data.entry.id, text: data.entry.bezeichnung },
        zahlungsdatum: convertDateCanadian(new Date()),
        permissions: data.entry.permissions
    };
</script>

<WalterHeaderDetail entry={data.entry} apiURL={data.apiURL} {title} />

<WalterGrid>
    <WalterBankkonto bind:entry={data.entry} fetchImpl={data.fetchImpl} />

    <WalterLinks>
        <WalterTransaktionen
            fetchImpl={data.fetchImpl}
            title="Transaktionen"
            entry={transaktion}
            rows={data.entry.transaktionen}
        />
    </WalterLinks>
</WalterGrid>

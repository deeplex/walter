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
    import type { PageData } from './$types';
    import {
        WalterGrid,
        WalterHeader,
        WalterLinks,
        WalterLinkTile,
        WalterVertragKaution
    } from '$walter/components';

    export let data: PageData;

    $: mieter = data.entry.mieter?.map((m) => m.name).join(', ');
    $: title = `Kaution – ${data.entry.wohnung?.text} – ${mieter}`;
</script>

<WalterHeader {title} />

<WalterGrid>
    <WalterVertragKaution
        bind:entry={data.entry}
        fetchImpl={data.fetchImpl}
        readonly={data.entry.permissions?.update === false}
    />

    <WalterLinks>
        <WalterLinkTile
            name={`Vertrag: ${data.entry.wohnung?.text} - ${mieter}`}
            href={`/vertraege/${data.id}`}
        />
    </WalterLinks>
</WalterGrid>

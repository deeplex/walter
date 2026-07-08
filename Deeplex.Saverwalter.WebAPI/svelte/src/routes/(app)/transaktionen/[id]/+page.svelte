<!-- Copyright (C) 2023-2025  Kai Lawrence -->
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
        WalterBuchungssaetze,
        WalterHeaderDetail,
        WalterGrid,
        WalterLinks,
        WalterLinkTile,
        WalterTransaktionRaw
    } from '$walter/components';
    import type { PageData } from './$types';
    import { WalterFileWrapper } from '$walter/lib';
    import { fileURL } from '$walter/services/files';

    export let data: PageData;

    let title = 'Transaktion';

    let fileWrapper = new WalterFileWrapper(data.fetchImpl);
    fileWrapper.registerStack();
    fileWrapper.register(title, data.fileURL);
</script>

<WalterHeaderDetail
    entry={data.entry}
    apiURL={data.apiURL}
    {title}
    bind:fileWrapper
/>

<WalterGrid>
    <WalterTransaktionRaw fetchImpl={data.fetchImpl} entry={data.entry} />

    <WalterLinks>
        <WalterBuchungssaetze
            fetchImpl={data.fetchImpl}
            title="Buchungssätze"
            rows={data.entry.buchungssaetze}
        />
    </WalterLinks>

    {#if data.entry.zahler}
        <WalterLinkTile
            bind:fileWrapper
            fileref={fileURL.kontakt(`${data.entry.zahler.id}`)}
            name={`Zahler: ${data.entry.zahler.text}`}
            href={`/kontakte/${data.entry.zahler.id}`}
        />
    {/if}

    {#if data.entry.zahlungsempfaenger}
        <WalterLinkTile
            bind:fileWrapper
            fileref={fileURL.kontakt(`${data.entry.zahlungsempfaenger.id}`)}
            name={`Zahlungsempfänger: ${data.entry.zahlungsempfaenger.text}`}
            href={`/kontakte/${data.entry.zahlungsempfaenger.id}`}
        />
    {/if}
</WalterGrid>

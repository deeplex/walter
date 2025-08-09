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
    import type { PageData } from './$types';
    import {
        WalterGrid,
        WalterKontakte,
        WalterWohnungen,
        WalterVertraege,
        WalterHeaderDetail,
        WalterLinks,
        WalterLinkTile,
        WalterKontakt
    } from '$walter/components';
    import { WalterFileWrapper, WalterTransaktionEntry } from '$walter/lib';
    import { fileURL } from '$walter/services/files';
    import WalterTransaktion from '$walter/components/details/WalterTransaktion.svelte';
    import WalterTransaktionen from '$walter/components/lists/WalterTransaktionen.svelte';
    import { convertDateCanadian } from '$walter/services/utils';

    export let data: PageData;

    let title = data.entry.name;
    $: {
        title = data.entry.name;
    }

    const transaktion: Partial<WalterTransaktionEntry> = {
        zahler: { id: data.entry.id, text: data.entry.bezeichnung },
        zahlungsdatum: convertDateCanadian(new Date()),
        permissions: data.entry.permissions
    };

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
    <WalterKontakt bind:entry={data.entry} fetchImpl={data.fetchImpl} />

    <WalterLinks>
        <WalterKontakte
            fetchImpl={data.fetchImpl}
            title="Juristische Personen"
            rows={data.entry.juristischePersonen}
        />
        {#if data.entry.rechtsform.id !== 0}
            <WalterKontakte
                fetchImpl={data.fetchImpl}
                title="Mitglieder"
                rows={data.entry.mitglieder}
            />
        {/if}
        <WalterWohnungen
            fetchImpl={data.fetchImpl}
            title="Wohnungen"
            rows={data.entry.wohnungen}
        />
        <WalterVertraege
            fetchImpl={data.fetchImpl}
            title="Verträge"
            rows={data.entry.vertraege}
        />

        <WalterTransaktionen
            fetchImpl={data.fetchImpl}
            title="Transaktionen"
            entry={transaktion}
            rows={data.entry.transaktionen}
        />

        {#if data.entry.adresse}
            <WalterLinkTile
                bind:fileWrapper
                fileref={fileURL.adresse(`${data.entry.adresse.id}`)}
                name={`Adresse: ${data.entry.adresse.anschrift}`}
                href={`/adressen/${data.entry.adresse.id}`}
            />
        {/if}
    </WalterLinks>
</WalterGrid>

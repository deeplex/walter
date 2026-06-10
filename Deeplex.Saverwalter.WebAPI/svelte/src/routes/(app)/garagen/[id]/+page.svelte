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
        WalterBuchungskonten,
        WalterGarage,
        WalterGarageVertraege,
        WalterGrid,
        WalterHeaderDetail,
        WalterLinks,
        WalterLinkTile
    } from '$walter/components';
    import {
        WalterFileWrapper,
        WalterGarageVertragEntry,
        validateGarage
    } from '$walter/lib';
    import { fileURL } from '$walter/services/files';

    export let data: PageData;

    const garageVertragEntry: Partial<WalterGarageVertragEntry> = {
        garage: {
            id: `${data.entry.id}`,
            text: data.entry.kennung
        },
        permissions: data.entry.permissions
    };

    let title = data.entry.kennung;
    $: {
        title = data.entry.kennung;
    }

    let fileWrapper = new WalterFileWrapper(data.fetchImpl);
    fileWrapper.registerStack();
    fileWrapper.register(title, data.fileURL);
</script>

<WalterHeaderDetail
    entry={data.entry}
    apiURL={data.apiURL}
    {title}
    bind:fileWrapper
    disabled={!validateGarage(data.entry)}
/>

<WalterGrid>
    <WalterGarage fetchImpl={data.fetchImpl} bind:entry={data.entry} />

    <WalterLinks>
        <WalterGarageVertraege
            fetchImpl={data.fetchImpl}
            entry={garageVertragEntry}
            title="Verträge"
            rows={data.entry.vertraege}
        />
        <WalterBuchungskonten title="Konten" rows={data.entry.konten} />

        {#if data.entry.besitzer?.id}
            <WalterLinkTile
                bind:fileWrapper
                fileref={fileURL.kontakt(`${data.entry.besitzer.id}`)}
                name={`Besitzer: ${data.entry.besitzer.text}`}
                href={`/kontakte/${data.entry.besitzer.id}`}
            />
        {/if}

        {#if data.entry.adresse?.id}
            <WalterLinkTile
                bind:fileWrapper
                fileref={fileURL.adresse(`${data.entry.adresse.id}`)}
                name={`Adresse: ${data.entry.adresse.anschrift}`}
                href={`/adressen/${data.entry.adresse.id}`}
            />
        {/if}
    </WalterLinks>
</WalterGrid>

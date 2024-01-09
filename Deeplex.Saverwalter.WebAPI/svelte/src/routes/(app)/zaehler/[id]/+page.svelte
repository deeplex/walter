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
        WalterHeaderDetail,
        WalterGrid,
        WalterZaehler,
        WalterZaehlerstaende,
        WalterUmlagen,
        WalterLinks,
        WalterLinkTile
    } from '$walter/components';
    import { convertDateCanadian } from '$walter/services/utils';
    import {
        WalterFileWrapper,
        type WalterZaehlerstandEntry
    } from '$walter/lib';
    import { fileURL } from '$walter/services/files';

    export let data: PageData;

    const staende = data.entry.staende;

    const lastZaehlerstand = staende.length
        ? staende[staende.length - 1]
        : undefined;
    const zaehlerstandEntry: Partial<WalterZaehlerstandEntry> = {
        zaehler: { id: '' + data.entry.id, text: data.entry.kennnummer },
        datum: convertDateCanadian(new Date()),
        stand: lastZaehlerstand?.stand || 0,
        einheit: lastZaehlerstand?.einheit,
        permissions: data.entry.permissions
    };

    let title = data.entry.kennnummer;
    $: {
        title = data.entry.kennnummer;
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
/>

<WalterGrid>
    <WalterZaehler fetchImpl={data.fetchImpl} entry={data.entry} />

    <WalterLinks>
        <WalterZaehlerstaende
            fetchImpl={data.fetchImpl}
            entry={zaehlerstandEntry}
            title="Zählerstände"
            rows={data.entry.staende}
        />
        <WalterUmlagen
            title="Umlagen"
            fetchImpl={data.fetchImpl}
            rows={data.entry.umlagen}
        />

        {#if data.entry.adresse !== null}
            <WalterLinkTile
                bind:fileWrapper
                fileref={fileURL.adresse(`${data.entry.adresse.id}`)}
                name={`Adresse: ${data.entry.adresse?.anschrift}`}
                href={`/adressen/${data.entry.adresse?.id}`}
            />
        {/if}
        {#if data.entry.wohnung !== null}
            <WalterLinkTile
                bind:fileWrapper
                fileref={fileURL.wohnung(`${data.entry.wohnung?.id}`)}
                name={`Wohnung: ${data.entry.wohnung?.text}`}
                href={`/wohnungen/${data.entry.wohnung?.id}`}
            />
        {/if}
    </WalterLinks>
</WalterGrid>

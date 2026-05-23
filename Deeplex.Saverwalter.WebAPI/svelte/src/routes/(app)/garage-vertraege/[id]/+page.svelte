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
        WalterGarageVertrag,
        WalterGarageVertragVersionen,
        WalterGrid,
        WalterHeaderDetail,
        WalterKontakte,
        WalterLinks,
        WalterLinkTile
    } from '$walter/components';
    import {
        WalterFileWrapper,
        WalterGarageVertragVersionEntry,
        WalterKontaktEntry,
        validateGarageVertrag
    } from '$walter/lib';

    export let data: PageData;

    const versionEntry: Partial<WalterGarageVertragVersionEntry> = {};
    const mieterEntry: Partial<WalterKontaktEntry> = {};

    let title = `${data.entry.garage?.text} - ${data.entry.mieterAuflistung ?? ''}`;
    $: {
        title = `${data.entry.garage?.text} - ${data.entry.mieterAuflistung ?? ''}`;
    }

    let fileWrapper = new WalterFileWrapper(data.fetchImpl);
    fileWrapper.registerStack();

    $: submitDisabled = !validateGarageVertrag(data.entry);
</script>

<WalterHeaderDetail
    entry={data.entry}
    apiURL={data.apiURL}
    {title}
    bind:fileWrapper
    disabled={submitDisabled}
/>

<WalterGrid>
    <WalterGarageVertrag fetchImpl={data.fetchImpl} bind:entry={data.entry} />

    <WalterLinks>
        <WalterKontakte
            fetchImpl={data.fetchImpl}
            entry={mieterEntry}
            title="Mieter"
            rows={data.entry.mieter}
        />
        <WalterGarageVertragVersionen
            fetchImpl={data.fetchImpl}
            entry={versionEntry}
            title="Nachträge"
            rows={data.entry.versionen}
        />

        {#if data.entry.garage?.id}
            <WalterLinkTile
                bind:fileWrapper
                fileref=""
                name={`Garage: ${data.entry.garage.text}`}
                href={`/garagen/${data.entry.garage.id}`}
            />
        {/if}

        {#if data.entry.vertrag?.id}
            <WalterLinkTile
                bind:fileWrapper
                fileref=""
                name={`Wohnungsvertrag: ${data.entry.vertrag.text}`}
                href={`/vertraege/${data.entry.vertrag.id}`}
            />
        {/if}
    </WalterLinks>
</WalterGrid>

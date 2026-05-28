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
        WalterAbrechnungsresultate,
        WalterGarageVertraege,
        WalterKontakte,
        WalterMietminderungen,
        WalterMietOpos,
        WalterHeaderDetail,
        WalterGrid,
        WalterVertrag,
        WalterVertragVersionen,
        WalterLinkTile,
        WalterLinks,
        WalterVertragsNkAnteile
    } from '$walter/components';
    import { getMietminderungEntry, getVertragversionEntry } from './utils';
    import {
        WalterFileWrapper,
        type WalterMietminderungEntry,
        WalterKontaktEntry,
        validateVertrag
    } from '$walter/lib';
    import { changeTracker } from '$walter/store';
    import WalterVertragTransaktionen from '$walter/components/lists/WalterVertragTransaktionen.svelte';
    import { fileURL } from '$walter/services/files';
    export let data: PageData;

    const mietminderungEntry: Partial<WalterMietminderungEntry> =
        getMietminderungEntry(data.entry);

    $: vertragversionEntry = getVertragversionEntry(data.entry);

    const mieterEntry: Partial<WalterKontaktEntry> = {};

    let title = `${data.entry.wohnung?.text} - ${data.entry.mieter
        ?.map((mieter) => mieter.name)
        .join(', ')}`;
    $: {
        title = `${data.entry.wohnung?.text} - ${data.entry.mieter
            ?.map((mieter) => mieter.name)
            .join(', ')}`;
    }

    let fileWrapper = new WalterFileWrapper(data.fetchImpl);
    fileWrapper.registerStack();
    fileWrapper.register(title, data.fileURL);

    let blockSave = false;
    let commitVersionIfPending: () => Promise<void>;
    $: submitDisabled =
        $changeTracker === 0 || !validateVertrag(data.entry) || blockSave;
</script>

<WalterHeaderDetail
    entry={data.entry}
    apiURL={data.apiURL}
    {title}
    bind:fileWrapper
    disabled={submitDisabled}
    beforeSave={commitVersionIfPending}
/>

<WalterGrid>
    <WalterVertrag
        fetchImpl={data.fetchImpl}
        bind:entry={data.entry}
        bind:blockSave
        bind:commitVersionIfPending
    />

    <WalterLinks>
        <WalterKontakte
            fetchImpl={data.fetchImpl}
            entry={mieterEntry}
            title="Mieter"
            rows={data.entry.mieter}
        />
        <WalterVertragVersionen
            entry={vertragversionEntry}
            title="Nachträge"
            rows={data.entry.versionen}
        />
        <WalterVertragTransaktionen
            fetchImpl={data.fetchImpl}
            vertrag={data.entry}
            title="Transaktionen"
            rows={data.transaktionen}
        />
        <WalterMietminderungen
            entry={mietminderungEntry}
            title="Mietminderungen"
            rows={data.entry.mietminderungen}
        />
        <WalterGarageVertraege
            fetchImpl={data.fetchImpl}
            title="Garagenverträge"
            rows={data.entry.garageVertraege}
        />

        <WalterMietOpos title="Mieten" rows={data.mietOpos} />

        <WalterVertragsNkAnteile
            fetchImpl={data.fetchImpl}
            vertragId={data.entry.id}
            title="NK-Anteile"
        />

        <WalterAbrechnungsresultate
            title="Abrechnungsresultate"
            rows={data.entry.abrechnungsresultate}
        />

        {#if data.entry.ansprechpartner?.id}
            <WalterLinkTile
                bind:fileWrapper
                fileref={fileURL.kontakt(`${data.entry.ansprechpartner.id}`)}
                name={`Ansprechpartner: ${data.entry.ansprechpartner.text}`}
                href={`/kontakte/${data.entry.ansprechpartner.id}`}
            />
        {/if}
        <WalterLinkTile
            bind:fileWrapper
            fileref={fileURL.wohnung(`${data.entry.wohnung.id}`)}
            name={`Wohnung: ${data.entry.wohnung.text}`}
            href={`/wohnungen/${data.entry.wohnung.id}`}
        />
    </WalterLinks>
</WalterGrid>

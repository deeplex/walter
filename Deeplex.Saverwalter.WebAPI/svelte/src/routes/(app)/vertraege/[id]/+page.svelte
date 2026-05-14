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
        WalterKontakte,
        WalterMieten,
        WalterMietminderungen,
        WalterHeaderDetail,
        WalterGrid,
        WalterVertrag,
        WalterVertragVersionen,
        WalterLinkTile,
        WalterLinks
    } from '$walter/components';
    import {
        getMietzahlungEntry,
        getMietminderungEntry,
        getVertragversionEntry
    } from './utils';
    import {
        WalterFileWrapper,
        type WalterMietminderungEntry,
        type WalterVertragVersionEntry,
        WalterBetriebskostenrechnungEntry,
        WalterKontaktEntry,
        type WalterMietzahlungInput
    } from '$walter/lib';
    import WalterBetriebskostenrechnungen from '$walter/components/lists/WalterBetriebskostenrechnungen.svelte';
    import { ClickableTile } from 'carbon-components-svelte';
    import { fileURL } from '$walter/services/files';
    export let data: PageData;

    const mietminderungEntry: Partial<WalterMietminderungEntry> =
        getMietminderungEntry(data.entry);

    const vertragversionEntry: Partial<WalterVertragVersionEntry> =
        getVertragversionEntry(data.entry);

    const mietzahlungEntry: Partial<WalterMietzahlungInput> =
        getMietzahlungEntry(data.entry);

    const mieterEntry: Partial<WalterKontaktEntry> = {};

    const betriebskostenrechnungEntry: Partial<WalterBetriebskostenrechnungEntry> =
        {};

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
</script>

<WalterHeaderDetail
    entry={data.entry}
    apiURL={data.apiURL}
    {title}
    bind:fileWrapper
/>

<WalterGrid>
    <WalterVertrag fetchImpl={data.fetchImpl} bind:entry={data.entry} />

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
        <WalterMieten
            entry={mietzahlungEntry}
            vertrag={data.entry}
            title="Mietzahlungen"
            rows={data.mietzahlungen}
        />
        <WalterMietminderungen
            entry={mietminderungEntry}
            title="Mietminderungen"
            rows={data.entry.mietminderungen}
        />
        <WalterBetriebskostenrechnungen
            entry={betriebskostenrechnungEntry}
            fetchImpl={data.fetchImpl}
            title="Betriebskostenrechnungen"
            rows={data.entry.betriebskostenrechnungen}
        />

        <WalterAbrechnungsresultate
            title="Abrechnungsresultate"
            rows={data.entry.abrechnungsresultate}
        />

        <WalterLinkTile
            bind:fileWrapper
            fileref={fileURL.kontakt(`${data.entry.ansprechpartner.id}`)}
            name={`Ansprechpartner: ${data.entry.ansprechpartner.text}`}
            href={`/kontakte/${data.entry.ansprechpartner.id}`}
        />
        <WalterLinkTile
            bind:fileWrapper
            fileref={fileURL.wohnung(`${data.entry.wohnung.id}`)}
            name={`Wohnung: ${data.entry.wohnung.text}`}
            href={`/wohnungen/${data.entry.wohnung.id}`}
        />
    </WalterLinks>
</WalterGrid>

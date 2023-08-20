<script lang="ts">
    import type { PageData } from './$types';
    import {
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
        getMieteEntry,
        getMietminderungEntry,
        getVertragversionEntry
    } from './utils';
    import {
        WalterS3FileWrapper,
        type WalterMieteEntry,
        type WalterMietminderungEntry,
        type WalterPersonEntry,
        type WalterVertragVersionEntry,
        WalterBetriebskostenrechnungEntry
    } from '$walter/lib';
    import WalterBetriebskostenrechnungen from '$walter/components/lists/WalterBetriebskostenrechnungen.svelte';
    import { ClickableTile } from 'carbon-components-svelte';
    export let data: PageData;

    const versionen = data.entry.versionen;
    const mietminderungEntry: Partial<WalterMietminderungEntry> =
        getMietminderungEntry(`${data.id}`);
    const vertragversionEntry: Partial<WalterVertragVersionEntry> =
        getVertragversionEntry(`${data.id}`, versionen[versionen.length - 1]);
    const mieteEntry: Partial<WalterMieteEntry> = getMieteEntry(
        `${data.id}`,
        versionen[versionen.length - 1]
    );
    const mieterEntry: Partial<WalterPersonEntry> = {};
    const betriebskostenrechnungEntry: Partial<WalterBetriebskostenrechnungEntry> = {};

    const title = `${data.entry.wohnung?.text} - ${data.entry.mieter
        ?.map((mieter) => mieter.name)
        .join(', ')}`;
    let fileWrapper = new WalterS3FileWrapper(data.fetchImpl);
    fileWrapper.registerStack()
    fileWrapper.register(title, data.S3URL);
</script>

<WalterHeaderDetail
    bind:entry={data.entry}
    apiURL={data.apiURL}
    {title}
    bind:fileWrapper
/>

<WalterGrid>
    <WalterVertrag fetchImpl={data.fetchImpl} bind:entry={data.entry} />

    <WalterLinks>
        <ClickableTile href="/abrechnung?vertrag={data.id}">Betriebskostenabrechnung erstellen</ClickableTile>

        <WalterKontakte
            entry={mieterEntry}
            title="Mieter"
            rows={data.entry.mieter}
        />
        <WalterVertragVersionen
            entry={vertragversionEntry}
            title="Versionen:"
            rows={data.entry.versionen}
        />
        <WalterMieten
            entry={mieteEntry}
            title="Mieten"
            rows={data.entry.mieten}
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
            rows={data.entry.betriebskostenrechnungen} />

        <!-- TODO id is GUID -->
        <!-- 
        <WalterLinkTile
            bind:fileWrapper
            name={`Ansprechpartner: ${data.entry.ansprechpartner?.text}`}
            href={`/nat/${data.entry.ansprechpartner?.id}`}
        /> -->
        <WalterLinkTile
            bind:fileWrapper
            name={`Wohnung: ${data.entry.wohnung.text}`}
            href={`/wohnungen/${data.entry.wohnung.id}`}
        />
    </WalterLinks>
</WalterGrid>

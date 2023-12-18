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
        type WalterVertragVersionEntry,
        WalterBetriebskostenrechnungEntry,
        WalterKontaktEntry
    } from '$walter/lib';
    import WalterBetriebskostenrechnungen from '$walter/components/lists/WalterBetriebskostenrechnungen.svelte';
    import { ClickableTile, Row } from 'carbon-components-svelte';
    import { walter_data_mieten } from '$walter/components/data/WalterData';
    import WalterDataScatterChart from '$walter/components/data/WalterDataScatterChart.svelte';
    import { S3URL } from '$walter/services/s3';
    export let data: PageData;

    const mietminderungEntry: Partial<WalterMietminderungEntry> =
        getMietminderungEntry(data.entry);

    const vertragversionEntry: Partial<WalterVertragVersionEntry> =
        getVertragversionEntry(data.entry);

    const mieteEntry: Partial<WalterMieteEntry> = getMieteEntry(data.entry);

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

    let fileWrapper = new WalterS3FileWrapper(data.fetchImpl);
    fileWrapper.registerStack();
    fileWrapper.register(title, data.S3URL);
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
        <ClickableTile href="/abrechnung?vertrag={data.id}"
            >Betriebskostenabrechnung erstellen</ClickableTile
        >

        <WalterKontakte
            fetchImpl={data.fetchImpl}
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
            rows={data.entry.betriebskostenrechnungen}
        />

        <WalterLinkTile
            bind:fileWrapper
            s3ref={S3URL.kontakt(`${data.entry.ansprechpartner.id}`)}
            name={`Ansprechpartner: ${data.entry.ansprechpartner.text}`}
            href={`/kontakte/${data.entry.ansprechpartner.id}`}
        />
        <WalterLinkTile
            bind:fileWrapper
            s3ref={S3URL.wohnung(`${data.entry.wohnung.id}`)}
            name={`Wohnung: ${data.entry.wohnung.text}`}
            href={`/wohnungen/${data.entry.wohnung.id}`}
        />
    </WalterLinks>

    {#if data.entry.mieten.length > 1}
        <Row>
            <WalterDataScatterChart
                config={walter_data_mieten('Mieten', data.entry.mieten)}
            />
        </Row>
    {/if}
</WalterGrid>

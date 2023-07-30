<script lang="ts">
    import { Truncate } from 'carbon-components-svelte';
    import type { PageData } from './$types';

    import {
        WalterKontakte,
        WalterMieten,
        WalterMietminderungen,
        WalterHeaderDetail,
        WalterGrid,
        WalterVertrag,
        WalterVertragVersionen,
        WalterAbrechnung,
        WalterAbrechnungControl,
        WalterLink
    } from '$walter/components';
    import type { WalterBetriebskostenabrechnungKostengruppenEntry } from '$walter/types';
    import { page } from '$app/stores';
    import { onMount } from 'svelte';
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
        type WalterVertragVersionEntry
    } from '$walter/lib';
    import { loadAbrechnung } from '$walter/services/abrechnung';
    import WalterLinks from '../../../../components/subdetails/WalterLinks.svelte';
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

    let abrechnung: WalterBetriebskostenabrechnungKostengruppenEntry;
    let searchParams: URLSearchParams = new URL($page.url).searchParams;

    onMount(async () => {
        const year = searchParams.get('abrechnung');
        if (year) {
            abrechnung = await loadAbrechnung(data.id, year, data.fetchImpl);
        }
    });

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

        <!-- TODO id is GUID -->
        <!-- 
        <WalterLink
            bind:fileWrapper
            name={`Ansprechpartner: ${data.entry.ansprechpartner?.text}`}
            href={`/nat/${data.entry.ansprechpartner?.id}`}
        /> -->
        <WalterLink
            bind:fileWrapper
            name={`Wohnung: ${data.entry.wohnung.text}`}
            href={`/wohnungen/${data.entry.wohnung.id}`}
        />
    </WalterLinks>

    <hr style="margin: 2em" />
    <Truncate>Betriebskostenabrechnung:</Truncate>

    <WalterAbrechnungControl
        {title}
        vertragId={data.id}
        fetchImpl={data.fetchImpl}
        S3URL={data.S3URL}
        firstYear={new Date(data.entry.beginn).getFullYear()}
        bind:S3files={data.files}
        bind:abrechnung
    />

    {#if abrechnung}
        <WalterAbrechnung {abrechnung} />
    {/if}
</WalterGrid>
